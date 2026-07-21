using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace ReStore
{
    public partial class Form2 : Form
    {
    
        public Form2()
        {
            InitializeComponent();
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;
            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (!char.IsWhiteSpace(c)) hasSpecial = true;
            }

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string ad = textBoxName.Text.Trim();
            string mail = textBoxMail.Text.Trim();
            string? telefon = textBoxPhone?.Text?.Trim();
            string sifre = textBoxPass.Text.Trim();

            if (string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(sifre))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Basit e-posta doğrulaması: en az '@' içermeli
            if (!mail.Contains("@"))
            {
                MessageBox.Show("Geçersiz e-posta adresi. Lütfen geçerli bir e-posta girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Şifre doğrulama: en az 8 karakter, büyük, küçük, rakam ve özel karakter
            if (!IsValidPassword(sifre))
            {
                MessageBox.Show("Şifre en az 8 karakter olmalı; büyük harf, küçük harf, rakam ve özel karakter içermelidir.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dao = new ReStore.Dao.KullaniciDao();
            // create a unique username (kullaniciAd) from name + random digits if needed
            string baseName = Regex.Replace(ad.ToLowerInvariant().Trim(), "[^a-z0-9]", "");
            if (string.IsNullOrWhiteSpace(baseName)) baseName = "user";
            string userNameToUse = baseName;
            int suffix = 0;
            while (dao.UserNameExists(userNameToUse))
            {
                suffix++;
                userNameToUse = baseName + suffix.ToString();
            }

            bool ok = dao.KayitOl(userNameToUse, mail, sifre, telefon);
            if (ok)
            {
                try
                {
                    var couponDao = new ReStore.Dao.CouponDao();
                    // ensure general coupon exists (restore10) and assign to all users
                    if (couponDao.GetByCode("restore10") == null)
                    {
                        couponDao.AddCoupon(new ReStore.Models.Coupon { Code = "restore10", Percent = 10m, Amount = null, Active = true });
                    }
                    // ensure new-registration coupon exists (yenikayit30)
                    if (couponDao.GetByCode("yenikayit30") == null)
                    {
                        couponDao.AddCoupon(new ReStore.Models.Coupon { Code = "yenikayit30", Percent = 30m, Amount = null, Active = true, ExpiresAt = DateTime.Now.AddDays(30) });
                    }

                    // assign general coupon to all users
                    couponDao.AssignCouponToAllUsers("restore10");
                    // assign new-registration coupon only to this new user
                    couponDao.AssignCouponToUser(mail, "yenikayit30");

                    MessageBox.Show("Hoş geldin! Hesabına indiriminiz eklendi (yenikayit30 ve restore10).", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
                    MessageBox.Show("Aramıza hoş geldin! Hesabına kuponlar eklenemeyebilir.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("Kayıt başarısız. Lütfen tekrar deneyin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // removed unused textBox1 TextChanged handler

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void buttonShowPassword_Click(object sender, EventArgs e)
        {
            // Toggle visibility on each click. Use PasswordChar = '\0' to show plain text.
            if (textBoxPass.PasswordChar == '\0')
            {
                // currently visible -> hide
                textBoxPass.PasswordChar = '*';
                textBoxPass.UseSystemPasswordChar = false;
                buttonShowPassword.Text = "👁";
            }
            else
            {
                
                textBoxPass.UseSystemPasswordChar = false;
                textBoxPass.PasswordChar = '\0';
                buttonShowPassword.Text = "🙈";
            }
        }
    }
}

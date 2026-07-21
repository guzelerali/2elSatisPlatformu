using ReStore.Config;
using ReStore.Dao;
using ReStore.Models;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace ReStore
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void linkLabelForgot_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var panel = new ForgotPasswordForm())
            {
                panel.ShowDialog(this);
            }
        }

        private bool SendPasswordResetEmail(string toEmail)
        {
            try
            {
                // TODO: SMTP ayarlarını kendi sunucuna göre güncelle
                var smtpHost = "smtp.example.com";
                var smtpUser = "you@example.com";
                var smtpPass = "yourpassword";
                var smtpPort = 587;
                var from = "no-reply@example.com";

                string token = Guid.NewGuid().ToString();
                string resetLink = $"https://yourdomain.com/resetpassword?token={token}";

                var msg = new MailMessage(from, toEmail)
                {
                    Subject = "ReStore - Şifre Sıfırlama",
                    Body = $"Şifrenizi sıfırlamak için lütfen şu bağlantıya tıklayın:\n{resetLink}",
                    IsBodyHtml = false
                };

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);
                    client.Send(msg);
                }

                return true;
            }
            catch (Exception)
            {
                // Gönderim hatasını kullanıcıya göstermiyoruz; loglama ekleyin
                return false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Open registration form
            Form2 kayitFormu = new Form2();
            kayitFormu.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string mail = textBox1.Text.Trim();
            string? telefon = textBoxPhone?.Text?.Trim();
            string sifre = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(sifre))
            {
                MessageBox.Show("Lütfen mail ve şifre alanlarını doldurun.", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            KullaniciDao kullaniciDao = new KullaniciDao();

            Kullanici? kullanici = kullaniciDao.GirisYap(mail, sifre);

            if (kullanici != null)
            {
                if (kullanici.IsAdmin)
                {
                    MessageBox.Show($"Hoş geldiniz, {kullanici.KullaniciAd}! Admin olarak giriş yaptınız.", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var adminPanel = new AdminForm();
                    adminPanel.ShowDialog();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show($"Hoş geldiniz, {kullanici.KullaniciAd}!", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Ensure telefon is available in DB and pass to user panel
                    // update telefon in DB if user provided one on login
                    if (!string.IsNullOrEmpty(telefon))
                    {
                        var _dao = new KullaniciDao();
                        _dao.UpdateTelefon(mail, telefon);
                        kullanici.Telefon = telefon;
                    }

                    var userPanel = new UserPanel(kullanici.Id, kullanici.KullaniciEmail, kullanici.KullaniciAd, kullanici.Telefon);
                    userPanel.ShowDialog();
                    this.Hide();
                }
            }
            else
            {
                MessageBox.Show("Giriş başarısız. Lütfen mail ve şifrenizi kontrol edin.", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
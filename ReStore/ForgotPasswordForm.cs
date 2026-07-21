using System;
using System.Drawing;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReStore.Dao;

namespace ReStore
{
    public class ForgotPasswordForm : Form
    {
        private Label labelInfo;
        private TextBox textBoxEmail;
        private Button buttonSend;
        private Button buttonCancel;

        public ForgotPasswordForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Şifre Yenileme";
            this.ClientSize = new Size(420, 160);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            labelInfo = new Label
            {
                Text = "Kayıtlı e-posta adresinizi girin. Şifre yenileme bağlantısı gönderilecektir.",
                Location = new Point(12, 10),
                Size = new Size(396, 40)
            };

            textBoxEmail = new TextBox
            {
                PlaceholderText = "E-posta adresi",
                Location = new Point(12, 58),
                Size = new Size(396, 27)
            };

            buttonSend = new Button
            {
                Text = "Gönder",
                Location = new Point(222, 100),
                Size = new Size(90, 30)
            };
            buttonSend.Click += ButtonSend_Click;

            buttonCancel = new Button
            {
                Text = "İptal",
                Location = new Point(318, 100),
                Size = new Size(90, 30)
            };
            buttonCancel.Click += (s, e) => this.Close();

            this.Controls.Add(labelInfo);
            this.Controls.Add(textBoxEmail);
            this.Controls.Add(buttonSend);
            this.Controls.Add(buttonCancel);
        }

        private void ButtonSend_Click(object? sender, EventArgs e)
        {
            string mail = textBoxEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(mail))
            {
                MessageBox.Show("Lütfen e-posta adresi girin.");
                return;
            }

            var dao = new KullaniciDao();
            bool varsa = dao.MailVarMi(mail);

            if (!varsa)
            {
                MessageBox.Show("Kayıtlı e-posta bulunamadı.");
                return;
            }

            // Arka planda mail gönder
            Task.Run(() => SendPasswordResetEmail(mail));

            MessageBox.Show("Şifre yenileme bağlantısı gönderildi.");
            this.Close();
        }

        private void SendPasswordResetEmail(string toEmail)
        {
            try
            {
                var smtpHost = "smtp.gmail.com";
                var smtpUser = "restoredestek@gmail.com";
                var smtpPass = "cevzcsljoqktsuoh"; // buraya app password yaz
                var smtpPort = 587;

                string token = Guid.NewGuid().ToString();

                // Fake link (çalışmasına gerek yok)
                string resetLink = $"https://fake-site.com/reset?token={token}";

                var message = new MailMessage();
                message.From = new MailAddress(smtpUser, "ReStore Destek");
                message.To.Add(toEmail);
                message.Subject = "Şifre Sıfırlama";

                message.IsBodyHtml = true;
                message.Body = $@"
                    <h3>Şifre Sıfırlama</h3>
                    <p>Şifrenizi sıfırlamak için aşağıdaki butona tıklayın:</p>
                    <a href='{resetLink}' style='padding:10px 20px;background:#007bff;color:white;text-decoration:none;border-radius:5px;'>Şifreyi Sıfırla</a>
                    <p>Bu link geçicidir.</p>
                ";

                using (var smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    smtp.EnableSsl = true;
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mail gönderme hatası: " + ex.Message);
            }
        }
    }
}
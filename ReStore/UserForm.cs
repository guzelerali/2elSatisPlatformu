using System;
using System.Drawing;
using System.Windows.Forms;

namespace ReStore
{
    public class UserForm : Form
    {
        private Label label = null!;
        public UserForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Kullanıcı Paneli";
            this.ClientSize = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            label = new Label
            {
                Text = "Kullanıcı paneline hoş geldiniz.",
                Location = new Point(20, 20),
                Size = new Size(560, 40),
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };

            this.Controls.Add(label);
        }
    }
}

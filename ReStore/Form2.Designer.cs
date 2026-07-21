namespace ReStore
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            textBoxName = new TextBox();
            textBoxMail = new TextBox();
            textBoxPass = new TextBox();
            button1 = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = false;
            label1.Location = new Point(157, 20);
            label1.Name = "label1";
            label1.Size = new Size(600, 60);
            label1.TabIndex = 0;
            label1.Text = "Hemen kayıt ol, ReStore’lu ol!";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point, 162);
            label1.Click += label1_Click;
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(330, 180);
            textBoxName.Name = "textBoxName";
            textBoxName.PlaceholderText = "Ad Soyad";
            textBoxName.Size = new Size(200, 27);
            textBoxName.TabIndex = 1;
            // textBoxMail
            // 
            textBoxMail.Location = new Point(330, 220);
            textBoxMail.Name = "textBoxMail";
            textBoxMail.PlaceholderText = "E-Mail";
            textBoxMail.Size = new Size(200, 27);
            textBoxMail.TabIndex = 2;
            // textBoxPhone
            textBoxPhone = new TextBox();
            textBoxPhone.Location = new Point(330, 260);
            textBoxPhone.Name = "textBoxPhone";
            textBoxPhone.PlaceholderText = "Telefon (opsiyonel)";
            textBoxPhone.Size = new Size(200, 27);
            textBoxPhone.TabIndex = 3;
            // textBoxPass
            // 
            textBoxPass.Location = new Point(330, 300);
            textBoxPass.Name = "textBoxPass";
            textBoxPass.PlaceholderText = "Şifre";
            textBoxPass.PasswordChar = '*';
            textBoxPass.UseSystemPasswordChar = true;
            textBoxPass.Size = new Size(200, 27);
            textBoxPass.TabIndex = 4;
            // buttonShowPassword (göz ikonu)
            // 
            buttonShowPassword = new Button();
            buttonShowPassword.Location = new Point(538, 298);
            buttonShowPassword.Name = "buttonShowPassword";
            buttonShowPassword.Size = new Size(32, 28);
            buttonShowPassword.TabIndex = 4;
            buttonShowPassword.Text = "👁";
            buttonShowPassword.FlatStyle = FlatStyle.Flat;
            buttonShowPassword.FlatAppearance.BorderSize = 0;
            buttonShowPassword.BackColor = Color.Transparent;
            buttonShowPassword.Click += buttonShowPassword_Click;
            // button1
            // 
            button1.Location = new Point(402, 365);
            button1.Margin = new Padding(3, 4, 3, 4);
            button1.Name = "button1";
            button1.Size = new Size(111, 40);
            button1.TabIndex = 4;
            button1.Text = "Kayıt Ol";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // (removed unused textBox1)
            // 
            // labelPasswordInfo
            // 
            labelPasswordInfo = new Label();
            labelPasswordInfo.Location = new Point(330, 336);
            labelPasswordInfo.Name = "labelPasswordInfo";
            labelPasswordInfo.Size = new Size(300, 20);
            labelPasswordInfo.TabIndex = 5;
            labelPasswordInfo.Text = "Şifre: en az 8 karakter, büyük/küçük harf, rakam, özel karakter";
            labelPasswordInfo.ForeColor = Color.DarkRed;
            labelPasswordInfo.BackColor = Color.Transparent;
            labelPasswordInfo.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            labelPasswordInfo.AutoSize = false;
            labelPasswordInfo.TextAlign = ContentAlignment.MiddleLeft;
            // Form2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(914, 600);
            // Add controls in visual/top-to-bottom order
            Controls.Add(textBoxName);
            Controls.Add(textBoxMail);
            Controls.Add(textBoxPhone);
            Controls.Add(textBoxPass);
            Controls.Add(buttonShowPassword);
            Controls.Add(labelPasswordInfo);
            Controls.Add(button1);
            Controls.Add(label1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form2";
            Text = "Form2";
            Load += Form2_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

            private Label label1;
        private TextBox textBoxName;

        private TextBox textBoxMail;
        private TextBox textBoxPhone;
        private TextBox textBoxPass;
        private Button button1;
        private Label labelPasswordInfo;
        private Button buttonShowPassword;
    }
}

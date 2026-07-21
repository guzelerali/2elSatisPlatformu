namespace ReStore
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            pictureBox1 = new PictureBox();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            button1 = new Button();
            label1 = new Label();
            button2 = new Button();
            linkLabelForgot = new LinkLabel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(329, 75);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(145, 56);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(333, 166);
            textBox1.Name = "textBox1";
            textBox1.PlaceholderText = "E-Mail";
            textBox1.Size = new Size(133, 23);
            textBox1.TabIndex = 1;
            // textBoxPhone
            textBoxPhone = new TextBox();
            textBoxPhone.Location = new Point(333, 196);
            textBoxPhone.Name = "textBoxPhone";
            textBoxPhone.PlaceholderText = "Telefon (opsiyonel)";
            textBoxPhone.Size = new Size(133, 23);
            textBoxPhone.TabIndex = 2;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(333, 228);
            textBox2.Name = "textBox2";
            textBox2.PasswordChar = '*';
            textBox2.PlaceholderText = "Şifre";
            textBox2.Size = new Size(133, 23);
            textBox2.TabIndex = 3;
            // 
            // button1
            // 
            button1.Location = new Point(361, 277);
            button1.Name = "button1";
            button1.Size = new Size(71, 27);
            button1.TabIndex = 3;
            button1.Text = "Giriş";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // linkLabelForgot
            // 
            linkLabelForgot.Location = new Point(333, 258);
            linkLabelForgot.Name = "linkLabelForgot";
            linkLabelForgot.Size = new Size(133, 20);
            linkLabelForgot.TabIndex = 6;
            linkLabelForgot.TabStop = true;
            linkLabelForgot.Text = "Şifreni mi unuttun?";
            linkLabelForgot.LinkBehavior = LinkBehavior.HoverUnderline;
            linkLabelForgot.LinkColor = Color.White;
            linkLabelForgot.ActiveLinkColor = Color.LightYellow;
            linkLabelForgot.VisitedLinkColor = Color.LightGray;
            linkLabelForgot.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            linkLabelForgot.LinkClicked += linkLabelForgot_LinkClicked;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BorderStyle = BorderStyle.Fixed3D;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            label1.ForeColor = SystemColors.ButtonFace;
            label1.Location = new Point(270, 338);
            label1.Name = "label1";
            label1.Size = new Size(114, 17);
            label1.TabIndex = 4;
            label1.Text = "Hesabınız yok mu? :";
            // 
            // button2
            // 
            button2.Location = new Point(420, 334);
            button2.Name = "button2";
            button2.Size = new Size(64, 23);
            button2.TabIndex = 5;
            button2.Text = "Kayıt Ol";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.IndianRed;
            ClientSize = new Size(800, 450);
            Controls.Add(linkLabelForgot);
            Controls.Add(button2);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(textBox2);
            Controls.Add(textBoxPhone);
            Controls.Add(textBox1);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private TextBox textBox1;
        private TextBox textBoxPhone;
        private TextBox textBox2;
        private Button button1;
        private Label label1;
        private Button button2;
        private LinkLabel linkLabelForgot;
    }
}

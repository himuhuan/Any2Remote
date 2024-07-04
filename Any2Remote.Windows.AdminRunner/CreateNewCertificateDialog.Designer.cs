namespace Any2Remote.Windows.AdminRunner
{
    partial class CreateNewCertificateDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateNewCertificateDialog));
            label1 = new Label();
            pictureBox1 = new PictureBox();
            label2 = new Label();
            certDnsName = new TextBox();
            label3 = new Label();
            label4 = new Label();
            buttonCreateCert = new Button();
            progressBar1 = new ProgressBar();
            labelTips = new Label();
            labelInfo = new Label();
            certPassword = new TextBox();
            ((System.ComponentModel.ISupportInitialize) pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(88, 9);
            label1.Name = "label1";
            label1.Size = new Size(413, 17);
            label1.TabIndex = 0;
            label1.Text = "为了保证 Any2Remote 连接的安全，需要为你的机器创建局域网安全证书。";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image) resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(12, 9);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(48, 48);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // label2
            // 
            label2.Location = new Point(88, 39);
            label2.Name = "label2";
            label2.Size = new Size(413, 63);
            label2.TabIndex = 2;
            label2.Text = "创建的证书将被保存到你的本地计算机上，服务器的启动与客户端建立 Any2Remote 连接时，该证书将验证连接安全。";
            // 
            // certDnsName
            // 
            certDnsName.Enabled = false;
            certDnsName.Location = new Point(88, 105);
            certDnsName.Name = "certDnsName";
            certDnsName.Size = new Size(396, 23);
            certDnsName.TabIndex = 3;
            certDnsName.Text = "any2remote.local";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 108);
            label3.Name = "label3";
            label3.Size = new Size(68, 17);
            label3.TabIndex = 4;
            label3.Text = "证书域名：";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 160);
            label4.Name = "label4";
            label4.Size = new Size(68, 17);
            label4.TabIndex = 6;
            label4.Text = "证书私钥：";
            // 
            // buttonCreateCert
            // 
            buttonCreateCert.Location = new Point(409, 267);
            buttonCreateCert.Name = "buttonCreateCert";
            buttonCreateCert.Size = new Size(75, 23);
            buttonCreateCert.TabIndex = 7;
            buttonCreateCert.Text = "创建";
            buttonCreateCert.UseVisualStyleBackColor = true;
            buttonCreateCert.Click += buttonCreateCert_Click;
            // 
            // progressBar1
            // 
            progressBar1.ForeColor = Color.FromArgb(  0,   64,   0);
            progressBar1.Location = new Point(12, 220);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(470, 23);
            progressBar1.TabIndex = 8;
            progressBar1.Visible = false;
            // 
            // labelTips
            // 
            labelTips.AutoSize = true;
            labelTips.Location = new Point(12, 200);
            labelTips.Name = "labelTips";
            labelTips.Size = new Size(442, 17);
            labelTips.TabIndex = 9;
            labelTips.Text = "Any2Remote 正在执行一项系统关键任务，请不要关闭 Any2Remote 或计算机。";
            labelTips.Visible = false;
            // 
            // labelInfo
            // 
            labelInfo.AutoSize = true;
            labelInfo.Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Italic);
            labelInfo.Location = new Point(12, 246);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(0, 16);
            labelInfo.TabIndex = 10;
            // 
            // certPassword
            // 
            certPassword.Location = new Point(88, 157);
            certPassword.Name = "certPassword";
            certPassword.PasswordChar = '*';
            certPassword.Size = new Size(394, 23);
            certPassword.TabIndex = 11;
            // 
            // CreateNewCertificateDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(495, 297);
            Controls.Add(certPassword);
            Controls.Add(labelInfo);
            Controls.Add(labelTips);
            Controls.Add(progressBar1);
            Controls.Add(buttonCreateCert);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(certDnsName);
            Controls.Add(label2);
            Controls.Add(pictureBox1);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon) resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CreateNewCertificateDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "创建 Any2Remote Server 证书";
            ((System.ComponentModel.ISupportInitialize) pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private PictureBox pictureBox1;
        private Label label2;
        private TextBox certDnsName;
        private Label label3;
        private Label label4;
        private Button buttonCreateCert;
        private ProgressBar progressBar1;
        private Label labelTips;
        private Label labelInfo;
        private TextBox certPassword;
    }
}
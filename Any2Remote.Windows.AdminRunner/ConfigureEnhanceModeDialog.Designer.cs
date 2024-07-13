namespace Any2Remote.Windows.AdminRunner
{
    partial class ConfigureEnhanceModeDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigureEnhanceModeDialog));
            pictureBox1 = new PictureBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            groupBox1 = new GroupBox();
            buttonUninstall = new Button();
            labelTermsrvVersion = new Label();
            labelWindowsVersion = new Label();
            labelStatus = new Label();
            label11 = new Label();
            label10 = new Label();
            label9 = new Label();
            labelProgress = new Label();
            progressBar = new ProgressBar();
            buttonInstall = new Button();
            label8 = new Label();
            ((System.ComponentModel.ISupportInitialize) pictureBox1).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image) resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(64, 64);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point,  0);
            label1.Location = new Point(92, 12);
            label1.Name = "label1";
            label1.Size = new Size(138, 17);
            label1.TabIndex = 3;
            label1.Text = "Any2Remote 增强模式";
            // 
            // label2
            // 
            label2.Location = new Point(92, 40);
            label2.Name = "label2";
            label2.Size = new Size(557, 49);
            label2.TabIndex = 4;
            label2.Text = "Any2Remote 增强模式包括了一系列对于远程服务的额外增强功能（包括应用多开，多会话功能支持等）。如果你使用的是 Microsoft Windows 家庭版，你必须启用 Any2Remote 增强模式。";
            // 
            // label3
            // 
            label3.Location = new Point(12, 89);
            label3.Name = "label3";
            label3.Size = new Size(637, 43);
            label3.TabIndex = 5;
            label3.Text = "要启用增强模式，Any2Remote 需要修改多个系统关键敏感文件或配置，可能导致系统不稳定。如果你不是管理员，请立即关闭本窗口。";
            // 
            // label4
            // 
            label4.Location = new Point(12, 132);
            label4.Name = "label4";
            label4.Size = new Size(213, 23);
            label4.TabIndex = 6;
            label4.Text = "如果你是本设备的管理员，请注意：\\n\\n 1. 安装";
            // 
            // label5
            // 
            label5.Image = (Image) resources.GetObject("label5.Image");
            label5.ImageAlign = ContentAlignment.MiddleLeft;
            label5.Location = new Point(42, 155);
            label5.Name = "label5";
            label5.Size = new Size(217, 23);
            label5.TabIndex = 7;
            label5.Text = "请关闭您的防病毒软件";
            label5.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            label6.Image = (Image) resources.GetObject("label6.Image");
            label6.ImageAlign = ContentAlignment.MiddleLeft;
            label6.Location = new Point(42, 178);
            label6.Name = "label6";
            label6.Size = new Size(205, 23);
            label6.TabIndex = 8;
            label6.Text = "点击“安装增强功能”";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            label7.Image = (Image) resources.GetObject("label7.Image");
            label7.ImageAlign = ContentAlignment.MiddleLeft;
            label7.Location = new Point(42, 201);
            label7.Name = "label7";
            label7.Size = new Size(444, 23);
            label7.TabIndex = 9;
            label7.Text = "将 C:\\Program Files\\RDP Wrapper 添加到防病毒软件的信任区";
            label7.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(buttonUninstall);
            groupBox1.Controls.Add(labelTermsrvVersion);
            groupBox1.Controls.Add(labelWindowsVersion);
            groupBox1.Controls.Add(labelStatus);
            groupBox1.Controls.Add(label11);
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(label9);
            groupBox1.Location = new Point(7, 254);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(642, 128);
            groupBox1.TabIndex = 11;
            groupBox1.TabStop = false;
            groupBox1.Text = "Any2Remote 增强模式信息";
            // 
            // buttonUninstall
            // 
            buttonUninstall.Location = new Point(505, 99);
            buttonUninstall.Name = "buttonUninstall";
            buttonUninstall.Size = new Size(129, 23);
            buttonUninstall.TabIndex = 15;
            buttonUninstall.Text = "卸载增强功能";
            buttonUninstall.UseVisualStyleBackColor = true;
            buttonUninstall.Click += buttonUninstall_Click;
            // 
            // labelTermsrvVersion
            // 
            labelTermsrvVersion.AutoSize = true;
            labelTermsrvVersion.Location = new Point(190, 83);
            labelTermsrvVersion.Name = "labelTermsrvVersion";
            labelTermsrvVersion.Size = new Size(0, 17);
            labelTermsrvVersion.TabIndex = 5;
            // 
            // labelWindowsVersion
            // 
            labelWindowsVersion.AutoSize = true;
            labelWindowsVersion.Location = new Point(190, 55);
            labelWindowsVersion.Name = "labelWindowsVersion";
            labelWindowsVersion.Size = new Size(0, 17);
            labelWindowsVersion.TabIndex = 4;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new Point(190, 28);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(0, 17);
            labelStatus.TabIndex = 3;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(35, 83);
            label11.Name = "label11";
            label11.Size = new Size(107, 17);
            label11.TabIndex = 2;
            label11.Text = "Termsrv 版本号：";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(35, 55);
            label10.Name = "label10";
            label10.Size = new Size(137, 17);
            label10.TabIndex = 1;
            label10.Text = "Windows 内核版本号：";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(35, 28);
            label9.Name = "label9";
            label9.Size = new Size(121, 17);
            label9.TabIndex = 0;
            label9.Text = "Any2Remote 状态：";
            // 
            // labelProgress
            // 
            labelProgress.AutoSize = true;
            labelProgress.Location = new Point(12, 397);
            labelProgress.Name = "labelProgress";
            labelProgress.Size = new Size(289, 17);
            labelProgress.TabIndex = 12;
            labelProgress.Text = "Any2Remote 正在安装增强模式，这需要一点时间。";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 417);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(633, 23);
            progressBar.TabIndex = 13;
            // 
            // buttonInstall
            // 
            buttonInstall.Location = new Point(466, 446);
            buttonInstall.Name = "buttonInstall";
            buttonInstall.Size = new Size(183, 23);
            buttonInstall.TabIndex = 14;
            buttonInstall.Text = "安装增强功能";
            buttonInstall.UseVisualStyleBackColor = true;
            buttonInstall.Click += buttonInstall_Click;
            // 
            // label8
            // 
            label8.Image = (Image) resources.GetObject("label8.Image");
            label8.ImageAlign = ContentAlignment.MiddleLeft;
            label8.Location = new Point(42, 224);
            label8.Name = "label8";
            label8.Size = new Size(205, 23);
            label8.TabIndex = 10;
            label8.Text = "重启你的防病毒软件";
            label8.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ConfigureEnhanceModeDialog
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            ClientSize = new Size(653, 489);
            Controls.Add(buttonInstall);
            Controls.Add(progressBar);
            Controls.Add(labelProgress);
            Controls.Add(groupBox1);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon) resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConfigureEnhanceModeDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Any2Remote 增强模式";
            Load += ConfigureEnhanceModeDialog_Load;
            ((System.ComponentModel.ISupportInitialize) pictureBox1).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private GroupBox groupBox1;
        private Label labelTermsrvVersion;
        private Label labelWindowsVersion;
        private Label labelStatus;
        private Label label11;
        private Label label10;
        private Label label9;
        private Label labelProgress;
        private ProgressBar progressBar;
        private Button buttonInstall;
        private Button buttonUninstall;
        private Label label8;
    }
}
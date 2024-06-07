namespace Any2Remote.Windows.AdminRunner
{
    partial class ServerActionDialog
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
            messageLabel = new Label();
            progressBar = new ProgressBar();
            iconBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize) iconBox).BeginInit();
            SuspendLayout();
            // 
            // messageLabel
            // 
            messageLabel.AutoSize = true;
            messageLabel.Location = new Point(58, 22);
            messageLabel.Name = "messageLabel";
            messageLabel.Size = new Size(263, 17);
            messageLabel.TabIndex = 0;
            messageLabel.Text = "Any2Remote 正在配置 Microsoft Windows ...";
            messageLabel.UseWaitCursor = true;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 68);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(455, 23);
            progressBar.TabIndex = 1;
            progressBar.UseWaitCursor = true;
            // 
            // iconBox
            // 
            iconBox.Location = new Point(12, 12);
            iconBox.Name = "iconBox";
            iconBox.Size = new Size(40, 40);
            iconBox.TabIndex = 2;
            iconBox.TabStop = false;
            iconBox.UseWaitCursor = true;
            // 
            // ServerActionDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(472, 96);
            ControlBox = false;
            Controls.Add(iconBox);
            Controls.Add(progressBar);
            Controls.Add(messageLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "ServerActionDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "初始化服务器";
            UseWaitCursor = true;
            Load += InitializeServerForm_Load;
            ((System.ComponentModel.ISupportInitialize) iconBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label messageLabel;
        private ProgressBar progressBar;
        private PictureBox iconBox;
    }
}
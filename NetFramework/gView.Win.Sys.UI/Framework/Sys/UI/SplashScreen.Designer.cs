namespace gView.Framework.system.UI
{
    partial class SplashScreen
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
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblBuild = new System.Windows.Forms.Label();
            this.lblBit = new System.Windows.Forms.Label();
            this.lblDemo = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblProductName = new System.Windows.Forms.Label();
            this.lblParseAssembly = new System.Windows.Forms.Label();
            this.lblAddPluginType = new System.Windows.Forms.Label();
            this.lblVersionNumber = new System.Windows.Forms.Label();
            this.lblBuildNumber = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 3000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(15, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(172, 10);
            this.label1.TabIndex = 1;
            this.label1.Text = "(c) 2006-2019 gView. All rights reserved.";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.BackColor = System.Drawing.Color.Transparent;
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.ForeColor = System.Drawing.Color.White;
            this.lblVersion.Location = new System.Drawing.Point(14, 13);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(45, 13);
            this.lblVersion.TabIndex = 3;
            this.lblVersion.Text = "Version:";
            this.lblVersion.Visible = false;
            // 
            // lblBuild
            // 
            this.lblBuild.AutoSize = true;
            this.lblBuild.BackColor = System.Drawing.Color.Transparent;
            this.lblBuild.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBuild.ForeColor = System.Drawing.Color.White;
            this.lblBuild.Location = new System.Drawing.Point(123, 13);
            this.lblBuild.Name = "lblBuild";
            this.lblBuild.Size = new System.Drawing.Size(33, 13);
            this.lblBuild.TabIndex = 6;
            this.lblBuild.Text = "Build:";
            // 
            // lblBit
            // 
            this.lblBit.AutoSize = true;
            this.lblBit.BackColor = System.Drawing.Color.Transparent;
            this.lblBit.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBit.ForeColor = System.Drawing.Color.White;
            this.lblBit.Location = new System.Drawing.Point(182, 13);
            this.lblBit.Name = "lblBit";
            this.lblBit.Size = new System.Drawing.Size(34, 13);
            this.lblBit.TabIndex = 7;
            this.lblBit.Text = "32 Bit";
            // 
            // lblDemo
            // 
            this.lblDemo.AutoSize = true;
            this.lblDemo.BackColor = System.Drawing.Color.White;
            this.lblDemo.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDemo.ForeColor = System.Drawing.Color.Red;
            this.lblDemo.Location = new System.Drawing.Point(685, 9);
            this.lblDemo.Name = "lblDemo";
            this.lblDemo.Size = new System.Drawing.Size(99, 31);
            this.lblDemo.TabIndex = 2;
            this.lblDemo.Text = "DEMO";
            this.lblDemo.Visible = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Blue;
            this.panel1.Controls.Add(this.lblBuildNumber);
            this.panel1.Controls.Add(this.lblVersionNumber);
            this.panel1.Controls.Add(this.lblVersion);
            this.panel1.Controls.Add(this.lblBit);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.lblBuild);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 335);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(841, 64);
            this.panel1.TabIndex = 8;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Black;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Image = global::gView.Win.Sys.UI.Properties.Resources.gview5_100x100_w;
            this.pictureBox1.InitialImage = global::gView.Win.Sys.UI.Properties.Resources.gview5_100x100_w;
            this.pictureBox1.Location = new System.Drawing.Point(36, 125);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(113, 101);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // lblProductName
            // 
            this.lblProductName.AutoSize = true;
            this.lblProductName.Font = new System.Drawing.Font("Arial", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProductName.ForeColor = System.Drawing.Color.White;
            this.lblProductName.Location = new System.Drawing.Point(173, 141);
            this.lblProductName.Name = "lblProductName";
            this.lblProductName.Size = new System.Drawing.Size(274, 45);
            this.lblProductName.TabIndex = 9;
            this.lblProductName.Text = "Product Name";
            // 
            // lblParseAssembly
            // 
            this.lblParseAssembly.AutoSize = true;
            this.lblParseAssembly.ForeColor = System.Drawing.Color.White;
            this.lblParseAssembly.Location = new System.Drawing.Point(14, 272);
            this.lblParseAssembly.Name = "lblParseAssembly";
            this.lblParseAssembly.Size = new System.Drawing.Size(37, 13);
            this.lblParseAssembly.TabIndex = 10;
            this.lblParseAssembly.Text = "Parse:";
            // 
            // lblAddPluginType
            // 
            this.lblAddPluginType.AutoSize = true;
            this.lblAddPluginType.ForeColor = System.Drawing.Color.White;
            this.lblAddPluginType.Location = new System.Drawing.Point(12, 303);
            this.lblAddPluginType.Name = "lblAddPluginType";
            this.lblAddPluginType.Size = new System.Drawing.Size(32, 13);
            this.lblAddPluginType.TabIndex = 11;
            this.lblAddPluginType.Text = "Add: ";
            // 
            // lblVersionNumber
            // 
            this.lblVersionNumber.AutoSize = true;
            this.lblVersionNumber.BackColor = System.Drawing.Color.Transparent;
            this.lblVersionNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersionNumber.ForeColor = System.Drawing.Color.White;
            this.lblVersionNumber.Location = new System.Drawing.Point(65, 13);
            this.lblVersionNumber.Name = "lblVersionNumber";
            this.lblVersionNumber.Size = new System.Drawing.Size(16, 13);
            this.lblVersionNumber.TabIndex = 8;
            this.lblVersionNumber.Text = "...";
            this.lblVersionNumber.Visible = false;
            // 
            // lblBuildNumber
            // 
            this.lblBuildNumber.AutoSize = true;
            this.lblBuildNumber.BackColor = System.Drawing.Color.Transparent;
            this.lblBuildNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBuildNumber.ForeColor = System.Drawing.Color.White;
            this.lblBuildNumber.Location = new System.Drawing.Point(160, 13);
            this.lblBuildNumber.Name = "lblBuildNumber";
            this.lblBuildNumber.Size = new System.Drawing.Size(16, 13);
            this.lblBuildNumber.TabIndex = 9;
            this.lblBuildNumber.Text = "...";
            this.lblBuildNumber.Visible = false;
            // 
            // SplashScreen
            // 
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(841, 399);
            this.Controls.Add(this.lblAddPluginType);
            this.Controls.Add(this.lblParseAssembly);
            this.Controls.Add(this.lblProductName);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblDemo);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SplashScreen";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblBuild;
        private System.Windows.Forms.Label lblBit;
        private System.Windows.Forms.Label lblDemo;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblProductName;
        private System.Windows.Forms.Label lblParseAssembly;
        private System.Windows.Forms.Label lblAddPluginType;
        private System.Windows.Forms.Label lblBuildNumber;
        private System.Windows.Forms.Label lblVersionNumber;
    }
}


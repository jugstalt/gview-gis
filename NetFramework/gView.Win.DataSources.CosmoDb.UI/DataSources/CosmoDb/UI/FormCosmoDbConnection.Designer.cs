namespace gView.DataSources.CosmoDb.UI
{
    partial class FormCosmoDbConnection
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
            this.panelContent = new System.Windows.Forms.Panel();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.txtAccountKey = new System.Windows.Forms.TextBox();
            this.lblAccountKey = new System.Windows.Forms.Label();
            this.txtAccountEndpoint = new System.Windows.Forms.TextBox();
            this.lblAccountEndpoint = new System.Windows.Forms.Label();
            this.panelFooter = new System.Windows.Forms.Panel();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.panelContent.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelContent
            // 
            this.panelContent.Controls.Add(this.txtDatabase);
            this.panelContent.Controls.Add(this.lblDatabase);
            this.panelContent.Controls.Add(this.txtAccountKey);
            this.panelContent.Controls.Add(this.lblAccountKey);
            this.panelContent.Controls.Add(this.txtAccountEndpoint);
            this.panelContent.Controls.Add(this.lblAccountEndpoint);
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(0, 130);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(991, 248);
            this.panelContent.TabIndex = 5;
            // 
            // txtDatabase
            // 
            this.txtDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDatabase.Location = new System.Drawing.Point(163, 174);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(799, 26);
            this.txtDatabase.TabIndex = 5;
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(21, 177);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(83, 20);
            this.lblDatabase.TabIndex = 4;
            this.lblDatabase.Text = "Database:";
            // 
            // txtAccountKey
            // 
            this.txtAccountKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAccountKey.Location = new System.Drawing.Point(163, 125);
            this.txtAccountKey.Name = "txtAccountKey";
            this.txtAccountKey.Size = new System.Drawing.Size(799, 26);
            this.txtAccountKey.TabIndex = 3;
            // 
            // lblAccountKey
            // 
            this.lblAccountKey.AutoSize = true;
            this.lblAccountKey.Location = new System.Drawing.Point(21, 128);
            this.lblAccountKey.Name = "lblAccountKey";
            this.lblAccountKey.Size = new System.Drawing.Size(98, 20);
            this.lblAccountKey.TabIndex = 2;
            this.lblAccountKey.Text = "AccountKey:";
            // 
            // txtAccountEndpoint
            // 
            this.txtAccountEndpoint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAccountEndpoint.Location = new System.Drawing.Point(163, 75);
            this.txtAccountEndpoint.Name = "txtAccountEndpoint";
            this.txtAccountEndpoint.Size = new System.Drawing.Size(799, 26);
            this.txtAccountEndpoint.TabIndex = 1;
            // 
            // lblAccountEndpoint
            // 
            this.lblAccountEndpoint.AutoSize = true;
            this.lblAccountEndpoint.Location = new System.Drawing.Point(21, 78);
            this.lblAccountEndpoint.Name = "lblAccountEndpoint";
            this.lblAccountEndpoint.Size = new System.Drawing.Size(136, 20);
            this.lblAccountEndpoint.TabIndex = 0;
            this.lblAccountEndpoint.Text = "AccountEndpoint:";
            // 
            // panelFooter
            // 
            this.panelFooter.Controls.Add(this.btnConnect);
            this.panelFooter.Controls.Add(this.btnCancel);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Location = new System.Drawing.Point(0, 378);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(991, 92);
            this.panelFooter.TabIndex = 4;
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnConnect.Location = new System.Drawing.Point(841, 21);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(121, 47);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(25, 21);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(121, 47);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(991, 130);
            this.panelHeader.TabIndex = 3;
            // 
            // FormCosmoDbConnection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(991, 470);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelFooter);
            this.Controls.Add(this.panelHeader);
            this.Name = "FormCosmoDbConnection";
            this.Text = "New CosmoDb Connection";
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            this.panelFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.TextBox txtAccountKey;
        private System.Windows.Forms.Label lblAccountKey;
        private System.Windows.Forms.TextBox txtAccountEndpoint;
        private System.Windows.Forms.Label lblAccountEndpoint;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panelHeader;
    }
}
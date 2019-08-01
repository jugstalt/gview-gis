namespace gView.DataSources.MongoDb.UI
{
    partial class FormBeforeCreateFeatureclass
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
            this.panelHeader = new System.Windows.Forms.Panel();
            this.panelFooter = new System.Windows.Forms.Panel();
            this.btnCloser = new System.Windows.Forms.Button();
            this.panelContent = new System.Windows.Forms.Panel();
            this.panelFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.White;
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(586, 119);
            this.panelHeader.TabIndex = 0;
            // 
            // panelFooter
            // 
            this.panelFooter.Controls.Add(this.btnCloser);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Location = new System.Drawing.Point(0, 680);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(586, 100);
            this.panelFooter.TabIndex = 1;
            // 
            // btnCloser
            // 
            this.btnCloser.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnCloser.Location = new System.Drawing.Point(351, 20);
            this.btnCloser.Name = "btnCloser";
            this.btnCloser.Size = new System.Drawing.Size(196, 54);
            this.btnCloser.TabIndex = 0;
            this.btnCloser.Text = "Close";
            this.btnCloser.UseVisualStyleBackColor = true;
            // 
            // panelContent
            // 
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(0, 119);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(586, 561);
            this.panelContent.TabIndex = 2;
            // 
            // FormBeforeCreateFeatureclass
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 780);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelFooter);
            this.Controls.Add(this.panelHeader);
            this.Name = "FormBeforeCreateFeatureclass";
            this.Text = "Generalize";
            this.Load += new System.EventHandler(this.FormBeforeCreateFeatureclass_Load);
            this.panelFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.Button btnCloser;
        private System.Windows.Forms.Panel panelContent;
    }
}
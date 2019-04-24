namespace gView.Plugins.Snapping
{
    partial class FormSnappingLauncher
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.optionPageSnapping1 = new gView.Plugins.Snapping.OptionPageSnapping();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // optionPageSnapping1
            // 
            this.optionPageSnapping1.Location = new System.Drawing.Point(3, 2);
            this.optionPageSnapping1.Name = "optionPageSnapping1";
            this.optionPageSnapping1.Size = new System.Drawing.Size(338, 371);
            this.optionPageSnapping1.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(225, 379);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(101, 37);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // FormSnappingLauncher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(338, 428);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.optionPageSnapping1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormSnappingLauncher";
            this.Text = "Snapping Options";
            this.ResumeLayout(false);

        }

        #endregion

        private OptionPageSnapping optionPageSnapping1;
        private System.Windows.Forms.Button btnOK;
    }
}
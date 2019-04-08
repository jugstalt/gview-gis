namespace gView.Interoperability.OGC.UI.Dataset.WFS
{
    partial class WFSMetadata
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

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.epsgSelector1 = new gView.Interoperability.OGC.UI.Dataset.WMS.EPSGSelector();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.epsgSelector1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(427, 286);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Supported EPSG Codes";
            // 
            // epsgSelector1
            // 
            this.epsgSelector1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.epsgSelector1.Location = new System.Drawing.Point(3, 16);
            this.epsgSelector1.Metadata = null;
            this.epsgSelector1.Name = "epsgSelector1";
            this.epsgSelector1.Size = new System.Drawing.Size(421, 267);
            this.epsgSelector1.TabIndex = 0;
            // 
            // WFSMetadata
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "WFSMetadata";
            this.Size = new System.Drawing.Size(454, 286);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private gView.Interoperability.OGC.UI.Dataset.WMS.EPSGSelector epsgSelector1;
    }
}

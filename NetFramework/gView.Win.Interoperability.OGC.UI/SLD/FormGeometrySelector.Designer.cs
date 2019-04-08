namespace gView.Interoperability.OGC.UI.SLD
{
    partial class FormGeometrySelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGeometrySelector));
            this.btnOK = new System.Windows.Forms.Button();
            this.radioPoint = new System.Windows.Forms.RadioButton();
            this.radioLine = new System.Windows.Forms.RadioButton();
            this.radioPolygon = new System.Windows.Forms.RadioButton();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.AccessibleDescription = null;
            this.btnOK.AccessibleName = null;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.BackgroundImage = null;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Font = null;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // radioPoint
            // 
            this.radioPoint.AccessibleDescription = null;
            this.radioPoint.AccessibleName = null;
            resources.ApplyResources(this.radioPoint, "radioPoint");
            this.radioPoint.BackgroundImage = null;
            this.radioPoint.Checked = true;
            this.radioPoint.Font = null;
            this.radioPoint.Name = "radioPoint";
            this.radioPoint.TabStop = true;
            this.radioPoint.UseVisualStyleBackColor = true;
            // 
            // radioLine
            // 
            this.radioLine.AccessibleDescription = null;
            this.radioLine.AccessibleName = null;
            resources.ApplyResources(this.radioLine, "radioLine");
            this.radioLine.BackgroundImage = null;
            this.radioLine.Font = null;
            this.radioLine.Name = "radioLine";
            this.radioLine.UseVisualStyleBackColor = true;
            // 
            // radioPolygon
            // 
            this.radioPolygon.AccessibleDescription = null;
            this.radioPolygon.AccessibleName = null;
            resources.ApplyResources(this.radioPolygon, "radioPolygon");
            this.radioPolygon.BackgroundImage = null;
            this.radioPolygon.Font = null;
            this.radioPolygon.Name = "radioPolygon";
            this.radioPolygon.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleDescription = null;
            this.btnCancel.AccessibleName = null;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.BackgroundImage = null;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = null;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // FormGeometrySelector
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.radioPolygon);
            this.Controls.Add(this.radioLine);
            this.Controls.Add(this.radioPoint);
            this.Controls.Add(this.btnOK);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormGeometrySelector";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.RadioButton radioPoint;
        private System.Windows.Forms.RadioButton radioLine;
        private System.Windows.Forms.RadioButton radioPolygon;
        private System.Windows.Forms.Button btnCancel;
    }
}
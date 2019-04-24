namespace gView.Plugins.Editor.Dialogs
{
    partial class FormChooseGeometry
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkPoint = new System.Windows.Forms.RadioButton();
            this.chkLine = new System.Windows.Forms.RadioButton();
            this.chkPolygon = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(246, 129);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(12, 129);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // chkPoint
            // 
            this.chkPoint.AutoSize = true;
            this.chkPoint.Location = new System.Drawing.Point(34, 25);
            this.chkPoint.Name = "chkPoint";
            this.chkPoint.Size = new System.Drawing.Size(97, 17);
            this.chkPoint.TabIndex = 2;
            this.chkPoint.TabStop = true;
            this.chkPoint.Text = "Point Geometry";
            this.chkPoint.UseVisualStyleBackColor = true;
            // 
            // chkLine
            // 
            this.chkLine.AutoSize = true;
            this.chkLine.Location = new System.Drawing.Point(34, 56);
            this.chkLine.Name = "chkLine";
            this.chkLine.Size = new System.Drawing.Size(109, 17);
            this.chkLine.TabIndex = 3;
            this.chkLine.TabStop = true;
            this.chkLine.Text = "Polyline Geometry";
            this.chkLine.UseVisualStyleBackColor = true;
            // 
            // chkPolygon
            // 
            this.chkPolygon.AutoSize = true;
            this.chkPolygon.Location = new System.Drawing.Point(34, 91);
            this.chkPolygon.Name = "chkPolygon";
            this.chkPolygon.Size = new System.Drawing.Size(111, 17);
            this.chkPolygon.TabIndex = 4;
            this.chkPolygon.TabStop = true;
            this.chkPolygon.Text = "Polygon Geometry";
            this.chkPolygon.UseVisualStyleBackColor = true;
            // 
            // FormChooseGeometry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(333, 164);
            this.Controls.Add(this.chkPolygon);
            this.Controls.Add(this.chkLine);
            this.Controls.Add(this.chkPoint);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormChooseGeometry";
            this.Text = "Choose Geometry Type";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton chkPoint;
        private System.Windows.Forms.RadioButton chkLine;
        private System.Windows.Forms.RadioButton chkPolygon;
    }
}
namespace gView.Framework.UI.Dialogs.Network
{
    partial class NetworkToleranceControl
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
            this.chkUseSnapTolerance = new System.Windows.Forms.CheckBox();
            this.numSnapTolerance = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numSnapTolerance)).BeginInit();
            this.SuspendLayout();
            // 
            // chkUseSnapTolerance
            // 
            this.chkUseSnapTolerance.AutoSize = true;
            this.chkUseSnapTolerance.Location = new System.Drawing.Point(28, 116);
            this.chkUseSnapTolerance.Name = "chkUseSnapTolerance";
            this.chkUseSnapTolerance.Size = new System.Drawing.Size(127, 17);
            this.chkUseSnapTolerance.TabIndex = 0;
            this.chkUseSnapTolerance.Text = "Use Snap Tolerance:";
            this.chkUseSnapTolerance.UseVisualStyleBackColor = true;
            // 
            // numSnapTolerance
            // 
            this.numSnapTolerance.DecimalPlaces = 7;
            this.numSnapTolerance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numSnapTolerance.Location = new System.Drawing.Point(162, 116);
            this.numSnapTolerance.Maximum = new decimal(new int[] {
            276447231,
            23283,
            0,
            0});
            this.numSnapTolerance.Name = "numSnapTolerance";
            this.numSnapTolerance.Size = new System.Drawing.Size(193, 20);
            this.numSnapTolerance.TabIndex = 1;
            this.numSnapTolerance.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            // 
            // NetworkToleranceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.numSnapTolerance);
            this.Controls.Add(this.chkUseSnapTolerance);
            this.Name = "NetworkToleranceControl";
            this.Size = new System.Drawing.Size(510, 338);
            ((System.ComponentModel.ISupportInitialize)(this.numSnapTolerance)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkUseSnapTolerance;
        private System.Windows.Forms.NumericUpDown numSnapTolerance;
    }
}

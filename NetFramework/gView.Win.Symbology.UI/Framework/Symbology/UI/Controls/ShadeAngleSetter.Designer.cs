namespace gView.Framework.Symbology.UI.Controls
{
    partial class ShadeAngleSetter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShadeAngleSetter));
            this.panelCone = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.numAngle = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numAngle)).BeginInit();
            this.SuspendLayout();
            // 
            // panelCone
            // 
            this.panelCone.AccessibleDescription = null;
            this.panelCone.AccessibleName = null;
            resources.ApplyResources(this.panelCone, "panelCone");
            this.panelCone.BackgroundImage = null;
            this.panelCone.Font = null;
            this.panelCone.Name = "panelCone";
            this.panelCone.Paint += new System.Windows.Forms.PaintEventHandler(this.ShadeAngleSetter_Paint);
            this.panelCone.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ShadeAngleSetter_MouseMove);
            this.panelCone.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ShadeAngleSetter_MouseDown);
            this.panelCone.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ShadeAngleSetter_MouseUp);
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // numAngle
            // 
            this.numAngle.AccessibleDescription = null;
            this.numAngle.AccessibleName = null;
            resources.ApplyResources(this.numAngle, "numAngle");
            this.numAngle.DecimalPlaces = 2;
            this.numAngle.Font = null;
            this.numAngle.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numAngle.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numAngle.Name = "numAngle";
            this.numAngle.ValueChanged += new System.EventHandler(this.numAngle_ValueChanged);
            // 
            // ShadeAngleSetter
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.numAngle);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panelCone);
            this.Font = null;
            this.Name = "ShadeAngleSetter";
            ((System.ComponentModel.ISupportInitialize)(this.numAngle)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelCone;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numAngle;
    }
}

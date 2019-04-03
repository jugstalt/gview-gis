namespace gView.Framework.UI.Dialogs
{
    partial class FormGridSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGridSettings));
            this.gridControl1 = new gView.Framework.UI.Controls.GridControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            resources.ApplyResources(this.gridControl1, "gridControl1");
            this.gridControl1.EnableHillShade = true;
            this.gridControl1.GridColorClasses = null;
            this.gridControl1.HillShadeVector = new double[] {
        0,
        0,
        0};
            this.gridControl1.MaxValue = 10000;
            this.gridControl1.MinValue = 0;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.UseHillShade = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.gridControl1);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // FormGridSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "FormGridSettings";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private gView.Framework.UI.Controls.GridControl gridControl1;
        private System.Windows.Forms.Panel panel1;
    }
}
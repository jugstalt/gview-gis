namespace gView.Framework.UI.Dialogs
{
    partial class FormLayerFilter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLayerFilter));
            this.panelPage = new System.Windows.Forms.Panel();
            this.txtExpression = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnQueryBuilder = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panelPage.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelPage
            // 
            this.panelPage.AccessibleDescription = null;
            this.panelPage.AccessibleName = null;
            resources.ApplyResources(this.panelPage, "panelPage");
            this.panelPage.BackgroundImage = null;
            this.panelPage.Controls.Add(this.txtExpression);
            this.panelPage.Controls.Add(this.panel2);
            this.panelPage.Controls.Add(this.panel1);
            this.panelPage.Font = null;
            this.panelPage.Name = "panelPage";
            // 
            // txtExpression
            // 
            this.txtExpression.AccessibleDescription = null;
            this.txtExpression.AccessibleName = null;
            resources.ApplyResources(this.txtExpression, "txtExpression");
            this.txtExpression.BackgroundImage = null;
            this.txtExpression.Font = null;
            this.txtExpression.Name = "txtExpression";
            // 
            // panel2
            // 
            this.panel2.AccessibleDescription = null;
            this.panel2.AccessibleName = null;
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.BackgroundImage = null;
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Font = null;
            this.panel2.Name = "panel2";
            // 
            // panel3
            // 
            this.panel3.AccessibleDescription = null;
            this.panel3.AccessibleName = null;
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.BackgroundImage = null;
            this.panel3.Controls.Add(this.btnQueryBuilder);
            this.panel3.Font = null;
            this.panel3.Name = "panel3";
            // 
            // btnQueryBuilder
            // 
            this.btnQueryBuilder.AccessibleDescription = null;
            this.btnQueryBuilder.AccessibleName = null;
            resources.ApplyResources(this.btnQueryBuilder, "btnQueryBuilder");
            this.btnQueryBuilder.BackgroundImage = null;
            this.btnQueryBuilder.Font = null;
            this.btnQueryBuilder.Name = "btnQueryBuilder";
            this.btnQueryBuilder.UseVisualStyleBackColor = true;
            this.btnQueryBuilder.Click += new System.EventHandler(this.btnQueryBuilder_Click);
            // 
            // panel1
            // 
            this.panel1.AccessibleDescription = null;
            this.panel1.AccessibleName = null;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackgroundImage = null;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Font = null;
            this.panel1.Name = "panel1";
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // FormLayerFilter
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.panelPage);
            this.Font = null;
            this.Icon = null;
            this.Name = "FormLayerFilter";
            this.panelPage.ResumeLayout(false);
            this.panelPage.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelPage;
        private System.Windows.Forms.TextBox txtExpression;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnQueryBuilder;
    }
}
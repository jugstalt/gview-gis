namespace gView.Plugins.MapTools.Dialogs
{
    partial class FormImportRenderers
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImportRenderers));
            this.chkFeatureRenderer = new System.Windows.Forms.CheckBox();
            this.chkLabelRenderer = new System.Windows.Forms.CheckBox();
            this.chkSelectionRenderer = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbRenderers = new System.Windows.Forms.GroupBox();
            this.gbScales = new System.Windows.Forms.GroupBox();
            this.chkRenderScales = new System.Windows.Forms.CheckBox();
            this.chkLabelScales = new System.Windows.Forms.CheckBox();
            this.gbProperties = new System.Windows.Forms.GroupBox();
            this.chkFilterQuery = new System.Windows.Forms.CheckBox();
            this.gbRenderers.SuspendLayout();
            this.gbScales.SuspendLayout();
            this.gbProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkFeatureRenderer
            // 
            resources.ApplyResources(this.chkFeatureRenderer, "chkFeatureRenderer");
            this.chkFeatureRenderer.Checked = true;
            this.chkFeatureRenderer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFeatureRenderer.Name = "chkFeatureRenderer";
            this.chkFeatureRenderer.UseVisualStyleBackColor = true;
            // 
            // chkLabelRenderer
            // 
            resources.ApplyResources(this.chkLabelRenderer, "chkLabelRenderer");
            this.chkLabelRenderer.Checked = true;
            this.chkLabelRenderer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLabelRenderer.Name = "chkLabelRenderer";
            this.chkLabelRenderer.UseVisualStyleBackColor = true;
            // 
            // chkSelectionRenderer
            // 
            resources.ApplyResources(this.chkSelectionRenderer, "chkSelectionRenderer");
            this.chkSelectionRenderer.Checked = true;
            this.chkSelectionRenderer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSelectionRenderer.Name = "chkSelectionRenderer";
            this.chkSelectionRenderer.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // gbRenderers
            // 
            resources.ApplyResources(this.gbRenderers, "gbRenderers");
            this.gbRenderers.Controls.Add(this.chkFeatureRenderer);
            this.gbRenderers.Controls.Add(this.chkLabelRenderer);
            this.gbRenderers.Controls.Add(this.chkSelectionRenderer);
            this.gbRenderers.Name = "gbRenderers";
            this.gbRenderers.TabStop = false;
            // 
            // gbScales
            // 
            resources.ApplyResources(this.gbScales, "gbScales");
            this.gbScales.Controls.Add(this.chkLabelScales);
            this.gbScales.Controls.Add(this.chkRenderScales);
            this.gbScales.Name = "gbScales";
            this.gbScales.TabStop = false;
            // 
            // chkRenderScales
            // 
            resources.ApplyResources(this.chkRenderScales, "chkRenderScales");
            this.chkRenderScales.Checked = true;
            this.chkRenderScales.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRenderScales.Name = "chkRenderScales";
            this.chkRenderScales.UseVisualStyleBackColor = true;
            // 
            // chkLabelScales
            // 
            resources.ApplyResources(this.chkLabelScales, "chkLabelScales");
            this.chkLabelScales.Checked = true;
            this.chkLabelScales.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLabelScales.Name = "chkLabelScales";
            this.chkLabelScales.UseVisualStyleBackColor = true;
            // 
            // gbProperties
            // 
            resources.ApplyResources(this.gbProperties, "gbProperties");
            this.gbProperties.Controls.Add(this.chkFilterQuery);
            this.gbProperties.Name = "gbProperties";
            this.gbProperties.TabStop = false;
            // 
            // chkFilterQuery
            // 
            resources.ApplyResources(this.chkFilterQuery, "chkFilterQuery");
            this.chkFilterQuery.Checked = true;
            this.chkFilterQuery.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFilterQuery.Name = "chkFilterQuery";
            this.chkFilterQuery.UseVisualStyleBackColor = true;
            // 
            // FormImportRenderers
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbProperties);
            this.Controls.Add(this.gbScales);
            this.Controls.Add(this.gbRenderers);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormImportRenderers";
            this.gbRenderers.ResumeLayout(false);
            this.gbRenderers.PerformLayout();
            this.gbScales.ResumeLayout(false);
            this.gbScales.PerformLayout();
            this.gbProperties.ResumeLayout(false);
            this.gbProperties.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkFeatureRenderer;
        private System.Windows.Forms.CheckBox chkLabelRenderer;
        private System.Windows.Forms.CheckBox chkSelectionRenderer;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox gbRenderers;
        private System.Windows.Forms.GroupBox gbScales;
        private System.Windows.Forms.CheckBox chkLabelScales;
        private System.Windows.Forms.CheckBox chkRenderScales;
        private System.Windows.Forms.GroupBox gbProperties;
        private System.Windows.Forms.CheckBox chkFilterQuery;
    }
}
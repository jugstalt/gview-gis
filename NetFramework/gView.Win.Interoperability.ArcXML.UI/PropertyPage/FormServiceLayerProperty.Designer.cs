namespace gView.Interoperability.ArcXML.UI.PropertyPage
{
    partial class FormServiceLayerProperty
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormServiceLayerProperty));
            this.panel1 = new System.Windows.Forms.Panel();
            this.tvProperties = new System.Windows.Forms.TreeView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.txtAttributeText = new System.Windows.Forms.TextBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtAttributeText);
            this.panel1.Controls.Add(this.splitter1);
            this.panel1.Controls.Add(this.tvProperties);
            this.panel1.Location = new System.Drawing.Point(2, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(398, 338);
            this.panel1.TabIndex = 0;
            // 
            // tvProperties
            // 
            this.tvProperties.Dock = System.Windows.Forms.DockStyle.Top;
            this.tvProperties.HideSelection = false;
            this.tvProperties.ImageIndex = 0;
            this.tvProperties.ImageList = this.imageList1;
            this.tvProperties.Location = new System.Drawing.Point(0, 0);
            this.tvProperties.Name = "tvProperties";
            this.tvProperties.SelectedImageIndex = 0;
            this.tvProperties.Size = new System.Drawing.Size(398, 186);
            this.tvProperties.TabIndex = 0;
            this.tvProperties.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvProperties_AfterSelect);
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 186);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(398, 3);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // txtAttributeText
            // 
            this.txtAttributeText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAttributeText.Enabled = false;
            this.txtAttributeText.Location = new System.Drawing.Point(0, 189);
            this.txtAttributeText.Multiline = true;
            this.txtAttributeText.Name = "txtAttributeText";
            this.txtAttributeText.Size = new System.Drawing.Size(398, 149);
            this.txtAttributeText.TabIndex = 2;
            this.txtAttributeText.TextChanged += new System.EventHandler(this.txtAttributeText_TextChanged);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "tag.png");
            this.imageList1.Images.SetKeyName(1, "value.png");
            // 
            // FormServiceLayerProperty
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 378);
            this.Controls.Add(this.panel1);
            this.Name = "FormServiceLayerProperty";
            this.Text = "FormServiceLayerProperty";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.TreeView tvProperties;
        private System.Windows.Forms.TextBox txtAttributeText;
        private System.Windows.Forms.ImageList imageList1;
    }
}
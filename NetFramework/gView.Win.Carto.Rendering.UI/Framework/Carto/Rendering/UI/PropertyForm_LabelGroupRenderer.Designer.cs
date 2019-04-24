namespace gView.Framework.Carto.Rendering.UI
{
    partial class PropertyForm_LabelGroupRenderer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyForm_LabelGroupRenderer));
            this.panel1 = new System.Windows.Forms.Panel();
            this.RendererBox = new gView.Framework.UI.Controls.ScrollingListBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panelScale = new System.Windows.Forms.Panel();
            this.propertyGridScaleDependent = new System.Windows.Forms.PropertyGrid();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnAdd = new System.Windows.Forms.ToolStripButton();
            this.btnRemove = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnUp = new System.Windows.Forms.ToolStripButton();
            this.btnDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnProperties = new System.Windows.Forms.ToolStripButton();
            this.panel1.SuspendLayout();
            this.panelScale.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.RendererBox);
            this.panel1.Controls.Add(this.splitter1);
            this.panel1.Controls.Add(this.panelScale);
            this.panel1.Controls.Add(this.toolStrip1);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(408, 332);
            this.panel1.TabIndex = 1;
            // 
            // RendererBox
            // 
            this.RendererBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RendererBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.RendererBox.FormattingEnabled = true;
            this.RendererBox.HorizontalScrollPos = 0;
            this.RendererBox.IntegralHeight = false;
            this.RendererBox.Location = new System.Drawing.Point(0, 25);
            this.RendererBox.Name = "RendererBox";
            this.RendererBox.Size = new System.Drawing.Size(408, 152);
            this.RendererBox.TabIndex = 1;
            this.RendererBox.VerticalScrollPos = 0;
            this.RendererBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.RendererBox_DrawItem);
            this.RendererBox.DoubleClick += new System.EventHandler(this.RendererBox_DoubleClick);
            this.RendererBox.SelectedIndexChanged += new System.EventHandler(this.RendererBox_SelectedIndexChanged);
            this.RendererBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.RendererBox_MeasureItem);
            this.RendererBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RendererBox_MouseMove);
            this.RendererBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RendererBox_MouseDown);
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 177);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(408, 3);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // panelScale
            // 
            this.panelScale.Controls.Add(this.propertyGridScaleDependent);
            this.panelScale.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelScale.Location = new System.Drawing.Point(0, 180);
            this.panelScale.Name = "panelScale";
            this.panelScale.Size = new System.Drawing.Size(408, 152);
            this.panelScale.TabIndex = 2;
            this.panelScale.Visible = false;
            // 
            // propertyGridScaleDependent
            // 
            this.propertyGridScaleDependent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridScaleDependent.Location = new System.Drawing.Point(0, 0);
            this.propertyGridScaleDependent.Name = "propertyGridScaleDependent";
            this.propertyGridScaleDependent.Size = new System.Drawing.Size(408, 152);
            this.propertyGridScaleDependent.TabIndex = 0;
            this.propertyGridScaleDependent.ToolbarVisible = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAdd,
            this.btnRemove,
            this.toolStripSeparator1,
            this.btnUp,
            this.btnDown,
            this.toolStripSeparator2,
            this.btnProperties});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(408, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnAdd
            // 
            this.btnAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAdd.Image = ((System.Drawing.Image)(resources.GetObject("btnAdd.Image")));
            this.btnAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(23, 22);
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRemove.Enabled = false;
            this.btnRemove.Image = ((System.Drawing.Image)(resources.GetObject("btnRemove.Image")));
            this.btnRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(23, 22);
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnUp
            // 
            this.btnUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnUp.Enabled = false;
            this.btnUp.Image = ((System.Drawing.Image)(resources.GetObject("btnUp.Image")));
            this.btnUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(23, 22);
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDown.Enabled = false;
            this.btnDown.Image = ((System.Drawing.Image)(resources.GetObject("btnDown.Image")));
            this.btnDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(23, 22);
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnProperties
            // 
            this.btnProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnProperties.Enabled = false;
            this.btnProperties.Image = ((System.Drawing.Image)(resources.GetObject("btnProperties.Image")));
            this.btnProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnProperties.Name = "btnProperties";
            this.btnProperties.Size = new System.Drawing.Size(23, 22);
            this.btnProperties.Click += new System.EventHandler(this.btnProperties_Click);
            // 
            // PropertyForm_LabelGroupRenderer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(447, 358);
            this.Controls.Add(this.panel1);
            this.Name = "PropertyForm_LabelGroupRenderer";
            this.Text = "PropertyForm_LabelGroupRenderer";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panelScale.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Panel panel1;
        private gView.Framework.UI.Controls.ScrollingListBox RendererBox;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panelScale;
        private System.Windows.Forms.PropertyGrid propertyGridScaleDependent;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnAdd;
        private System.Windows.Forms.ToolStripButton btnRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnUp;
        private System.Windows.Forms.ToolStripButton btnDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnProperties;
    }
}
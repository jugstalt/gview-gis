namespace gView.Plugins.Snapping
{
    partial class OptionPageSnapping
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionPageSnapping));
            this.panelPage = new System.Windows.Forms.Panel();
            this.dgSchemas = new System.Windows.Forms.DataGridView();
            this.panel3 = new System.Windows.Forms.Panel();
            this.numScale = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cmbSchemas = new System.Windows.Forms.ComboBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolAddSchema = new System.Windows.Forms.ToolStripButton();
            this.toolRemoveSchema = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolMoveUp = new System.Windows.Forms.ToolStripButton();
            this.toolMoveDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolRenameSchema = new System.Windows.Forms.ToolStripButton();
            this.Layer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVertex = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colEdge = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colEndPoint = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.panelPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgSchemas)).BeginInit();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelPage
            // 
            this.panelPage.Controls.Add(this.dgSchemas);
            this.panelPage.Controls.Add(this.panel3);
            this.panelPage.Controls.Add(this.panel1);
            this.panelPage.Controls.Add(this.toolStrip1);
            resources.ApplyResources(this.panelPage, "panelPage");
            this.panelPage.Name = "panelPage";
            // 
            // dgSchemas
            // 
            this.dgSchemas.AllowUserToAddRows = false;
            this.dgSchemas.AllowUserToDeleteRows = false;
            this.dgSchemas.AllowUserToResizeRows = false;
            this.dgSchemas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgSchemas.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Layer,
            this.colVertex,
            this.colEdge,
            this.colEndPoint});
            resources.ApplyResources(this.dgSchemas, "dgSchemas");
            this.dgSchemas.Name = "dgSchemas";
            this.dgSchemas.RowHeadersVisible = false;
            this.dgSchemas.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgSchemas_CellValueChanged);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.numScale);
            this.panel3.Controls.Add(this.label2);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // numScale
            // 
            resources.ApplyResources(this.numScale, "numScale");
            this.numScale.Maximum = new decimal(new int[] {
            268435455,
            1042612833,
            542101086,
            0});
            this.numScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numScale.Name = "numScale";
            this.numScale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numScale.ValueChanged += new System.EventHandler(this.numScale_ValueChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmbSchemas);
            this.panel1.Controls.Add(this.panel4);
            this.panel1.Controls.Add(this.panel2);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // cmbSchemas
            // 
            resources.ApplyResources(this.cmbSchemas, "cmbSchemas");
            this.cmbSchemas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSchemas.FormattingEnabled = true;
            this.cmbSchemas.Name = "cmbSchemas";
            this.cmbSchemas.SelectedIndexChanged += new System.EventHandler(this.cmbSchemas_SelectedIndexChanged);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.label1);
            this.panel4.Controls.Add(this.panel5);
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // panel5
            // 
            resources.ApplyResources(this.panel5, "panel5");
            this.panel5.Name = "panel5";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolAddSchema,
            this.toolRemoveSchema,
            this.toolStripSeparator1,
            this.toolMoveUp,
            this.toolMoveDown,
            this.toolStripSeparator2,
            this.toolRenameSchema});
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Name = "toolStrip1";
            // 
            // toolAddSchema
            // 
            this.toolAddSchema.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolAddSchema.Image = global::gView.Win.Plugins.Snapping.Properties.Resources.add_16;
            resources.ApplyResources(this.toolAddSchema, "toolAddSchema");
            this.toolAddSchema.Name = "toolAddSchema";
            this.toolAddSchema.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // toolRemoveSchema
            // 
            this.toolRemoveSchema.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolRemoveSchema.Image = global::gView.Win.Plugins.Snapping.Properties.Resources.delete_16;
            resources.ApplyResources(this.toolRemoveSchema, "toolRemoveSchema");
            this.toolRemoveSchema.Name = "toolRemoveSchema";
            this.toolRemoveSchema.Click += new System.EventHandler(this.toolRemoveSchema_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // toolMoveUp
            // 
            this.toolMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolMoveUp.Image = global::gView.Win.Plugins.Snapping.Properties.Resources.arrow_up_16;
            resources.ApplyResources(this.toolMoveUp, "toolMoveUp");
            this.toolMoveUp.Name = "toolMoveUp";
            this.toolMoveUp.Click += new System.EventHandler(this.toolMoveUp_Click);
            // 
            // toolMoveDown
            // 
            this.toolMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolMoveDown.Image = global::gView.Win.Plugins.Snapping.Properties.Resources.arrow_down_16;
            resources.ApplyResources(this.toolMoveDown, "toolMoveDown");
            this.toolMoveDown.Name = "toolMoveDown";
            this.toolMoveDown.Click += new System.EventHandler(this.toolMoveDown_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // toolRenameSchema
            // 
            this.toolRenameSchema.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolRenameSchema, "toolRenameSchema");
            this.toolRenameSchema.Name = "toolRenameSchema";
            this.toolRenameSchema.Click += new System.EventHandler(this.toolRenameSchema_Click);
            // 
            // Layer
            // 
            resources.ApplyResources(this.Layer, "Layer");
            this.Layer.Name = "Layer";
            this.Layer.ReadOnly = true;
            // 
            // colVertex
            // 
            resources.ApplyResources(this.colVertex, "colVertex");
            this.colVertex.Name = "colVertex";
            // 
            // colEdge
            // 
            resources.ApplyResources(this.colEdge, "colEdge");
            this.colEdge.Name = "colEdge";
            // 
            // colEndPoint
            // 
            resources.ApplyResources(this.colEndPoint, "colEndPoint");
            this.colEndPoint.Name = "colEndPoint";
            // 
            // OptionPageSnapping
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelPage);
            this.Name = "OptionPageSnapping";
            this.panelPage.ResumeLayout(false);
            this.panelPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgSchemas)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Panel panelPage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cmbSchemas;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.DataGridView dgSchemas;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolAddSchema;
        private System.Windows.Forms.ToolStripButton toolRemoveSchema;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolMoveUp;
        private System.Windows.Forms.ToolStripButton toolMoveDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolRenameSchema;
        private System.Windows.Forms.NumericUpDown numScale;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Layer;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colVertex;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colEdge;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colEndPoint;
    }
}

namespace gView.Framework.UI.Controls
{
    partial class SpatialIndexControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpatialIndexControl));
            this.gpExtent = new System.Windows.Forms.GroupBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.numBottom = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numRight = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numLeft = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numTop = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.numCellSizeX = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numLevels = new System.Windows.Forms.NumericUpDown();
            this.btnImportDef = new System.Windows.Forms.Button();
            this.panelLevels = new System.Windows.Forms.Panel();
            this.panelRaster = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.numCellsPerObject = new System.Windows.Forms.NumericUpDown();
            this.cmbLevel1 = new System.Windows.Forms.ComboBox();
            this.cmbLevel4 = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.cmbLevel3 = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.cmbLevel2 = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.cmbIndexType = new System.Windows.Forms.ComboBox();
            this.gpExtent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTop)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCellSizeX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLevels)).BeginInit();
            this.panelLevels.SuspendLayout();
            this.panelRaster.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCellsPerObject)).BeginInit();
            this.SuspendLayout();
            // 
            // gpExtent
            // 
            this.gpExtent.Controls.Add(this.btnImport);
            this.gpExtent.Controls.Add(this.numBottom);
            this.gpExtent.Controls.Add(this.label4);
            this.gpExtent.Controls.Add(this.numRight);
            this.gpExtent.Controls.Add(this.label3);
            this.gpExtent.Controls.Add(this.numLeft);
            this.gpExtent.Controls.Add(this.label2);
            this.gpExtent.Controls.Add(this.numTop);
            this.gpExtent.Controls.Add(this.label1);
            resources.ApplyResources(this.gpExtent, "gpExtent");
            this.gpExtent.Name = "gpExtent";
            this.gpExtent.TabStop = false;
            // 
            // btnImport
            // 
            resources.ApplyResources(this.btnImport, "btnImport");
            this.btnImport.Name = "btnImport";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // numBottom
            // 
            this.numBottom.DecimalPlaces = 2;
            resources.ApplyResources(this.numBottom, "numBottom");
            this.numBottom.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numBottom.Minimum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            -2147483648});
            this.numBottom.Name = "numBottom";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // numRight
            // 
            this.numRight.DecimalPlaces = 2;
            resources.ApplyResources(this.numRight, "numRight");
            this.numRight.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numRight.Minimum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            -2147483648});
            this.numRight.Name = "numRight";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // numLeft
            // 
            this.numLeft.DecimalPlaces = 2;
            resources.ApplyResources(this.numLeft, "numLeft");
            this.numLeft.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numLeft.Minimum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            -2147483648});
            this.numLeft.Name = "numLeft";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // numTop
            // 
            this.numTop.DecimalPlaces = 2;
            resources.ApplyResources(this.numTop, "numTop");
            this.numTop.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numTop.Minimum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            -2147483648});
            this.numTop.Name = "numTop";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.numCellSizeX);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.numLevels);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // numCellSizeX
            // 
            this.numCellSizeX.DecimalPlaces = 2;
            resources.ApplyResources(this.numCellSizeX, "numCellSizeX");
            this.numCellSizeX.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numCellSizeX.Name = "numCellSizeX";
            this.numCellSizeX.ReadOnly = true;
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // numLevels
            // 
            resources.ApplyResources(this.numLevels, "numLevels");
            this.numLevels.Maximum = new decimal(new int[] {
            62,
            0,
            0,
            0});
            this.numLevels.Name = "numLevels";
            this.numLevels.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numLevels.ValueChanged += new System.EventHandler(this.numLevels_ValueChanged);
            // 
            // btnImportDef
            // 
            resources.ApplyResources(this.btnImportDef, "btnImportDef");
            this.btnImportDef.Name = "btnImportDef";
            this.btnImportDef.UseVisualStyleBackColor = true;
            this.btnImportDef.Click += new System.EventHandler(this.btnImportDef_Click);
            // 
            // panelLevels
            // 
            this.panelLevels.Controls.Add(this.btnImportDef);
            this.panelLevels.Controls.Add(this.groupBox2);
            resources.ApplyResources(this.panelLevels, "panelLevels");
            this.panelLevels.Name = "panelLevels";
            // 
            // panelRaster
            // 
            this.panelRaster.Controls.Add(this.groupBox3);
            resources.ApplyResources(this.panelRaster, "panelRaster");
            this.panelRaster.Name = "panelRaster";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.numCellsPerObject);
            this.groupBox3.Controls.Add(this.cmbLevel1);
            this.groupBox3.Controls.Add(this.cmbLevel4);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.cmbLevel3);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.cmbLevel2);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // numCellsPerObject
            // 
            resources.ApplyResources(this.numCellsPerObject, "numCellsPerObject");
            this.numCellsPerObject.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numCellsPerObject.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCellsPerObject.Name = "numCellsPerObject";
            this.numCellsPerObject.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
            // 
            // cmbLevel1
            // 
            this.cmbLevel1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevel1.FormattingEnabled = true;
            this.cmbLevel1.Items.AddRange(new object[] {
            resources.GetString("cmbLevel1.Items"),
            resources.GetString("cmbLevel1.Items1"),
            resources.GetString("cmbLevel1.Items2"),
            resources.GetString("cmbLevel1.Items3")});
            resources.ApplyResources(this.cmbLevel1, "cmbLevel1");
            this.cmbLevel1.Name = "cmbLevel1";
            // 
            // cmbLevel4
            // 
            this.cmbLevel4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevel4.FormattingEnabled = true;
            this.cmbLevel4.Items.AddRange(new object[] {
            resources.GetString("cmbLevel4.Items"),
            resources.GetString("cmbLevel4.Items1"),
            resources.GetString("cmbLevel4.Items2"),
            resources.GetString("cmbLevel4.Items3")});
            resources.ApplyResources(this.cmbLevel4, "cmbLevel4");
            this.cmbLevel4.Name = "cmbLevel4";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // cmbLevel3
            // 
            this.cmbLevel3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevel3.FormattingEnabled = true;
            this.cmbLevel3.Items.AddRange(new object[] {
            resources.GetString("cmbLevel3.Items"),
            resources.GetString("cmbLevel3.Items1"),
            resources.GetString("cmbLevel3.Items2"),
            resources.GetString("cmbLevel3.Items3")});
            resources.ApplyResources(this.cmbLevel3, "cmbLevel3");
            this.cmbLevel3.Name = "cmbLevel3";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // cmbLevel2
            // 
            this.cmbLevel2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevel2.FormattingEnabled = true;
            this.cmbLevel2.Items.AddRange(new object[] {
            resources.GetString("cmbLevel2.Items"),
            resources.GetString("cmbLevel2.Items1"),
            resources.GetString("cmbLevel2.Items2"),
            resources.GetString("cmbLevel2.Items3")});
            resources.ApplyResources(this.cmbLevel2, "cmbLevel2");
            this.cmbLevel2.Name = "cmbLevel2";
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // cmbIndexType
            // 
            this.cmbIndexType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbIndexType, "cmbIndexType");
            this.cmbIndexType.FormattingEnabled = true;
            this.cmbIndexType.Items.AddRange(new object[] {
            resources.GetString("cmbIndexType.Items"),
            resources.GetString("cmbIndexType.Items1"),
            resources.GetString("cmbIndexType.Items2")});
            this.cmbIndexType.Name = "cmbIndexType";
            this.cmbIndexType.SelectedIndexChanged += new System.EventHandler(this.cmbIndexType_SelectedIndexChanged);
            // 
            // SpatialIndexControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmbIndexType);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.panelLevels);
            this.Controls.Add(this.gpExtent);
            this.Controls.Add(this.panelRaster);
            this.Name = "SpatialIndexControl";
            this.gpExtent.ResumeLayout(false);
            this.gpExtent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTop)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCellSizeX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLevels)).EndInit();
            this.panelLevels.ResumeLayout(false);
            this.panelRaster.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCellsPerObject)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gpExtent;
        private System.Windows.Forms.NumericUpDown numBottom;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numRight;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numLeft;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numTop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numLevels;
        private System.Windows.Forms.NumericUpDown numCellSizeX;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnImportDef;
        private System.Windows.Forms.Panel panelLevels;
        private System.Windows.Forms.Panel panelRaster;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numCellsPerObject;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cmbLevel1;
        private System.Windows.Forms.ComboBox cmbLevel4;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cmbLevel3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cmbLevel2;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox cmbIndexType;
    }
}

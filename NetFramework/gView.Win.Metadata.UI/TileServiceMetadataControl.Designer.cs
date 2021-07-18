namespace gView.Framework.Metadata.UI
{
    partial class TileServiceMetadataControl
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
            this.gpExtent = new System.Windows.Forms.GroupBox();
            this.btnRemoveEpsg = new System.Windows.Forms.Button();
            this.btnAddEpsg = new System.Windows.Forms.Button();
            this.panelExtent = new System.Windows.Forms.Panel();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.numTop = new System.Windows.Forms.NumericUpDown();
            this.numLL_y = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.numLL_x = new System.Windows.Forms.NumericUpDown();
            this.btnImport = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.numBottom = new System.Windows.Forms.NumericUpDown();
            this.numUL_y = new System.Windows.Forms.NumericUpDown();
            this.numLeft = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.numUL_x = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numRight = new System.Windows.Forms.NumericUpDown();
            this.cmbEpsgs = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.chkUseTiling = new System.Windows.Forms.CheckBox();
            this.gpTileSize = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.numTileHeight = new System.Windows.Forms.NumericUpDown();
            this.numTileWidth = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lstScales = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblResolutinDpi = new System.Windows.Forms.Label();
            this.numScale = new System.Windows.Forms.NumericUpDown();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.chkUpperLeftCacheTiles = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.gbOrigin = new System.Windows.Forms.GroupBox();
            this.chkLowerLeft = new System.Windows.Forms.CheckBox();
            this.chkUpperLeft = new System.Windows.Forms.CheckBox();
            this.chkLowerLeftCacheTiles = new System.Windows.Forms.CheckBox();
            this.gbTileFormat = new System.Windows.Forms.GroupBox();
            this.chkJpg = new System.Windows.Forms.CheckBox();
            this.chkPng = new System.Windows.Forms.CheckBox();
            this.chkRenderOnTheFly = new System.Windows.Forms.CheckBox();
            this.chkTransparentPng = new System.Windows.Forms.CheckBox();
            this.gpExtent.SuspendLayout();
            this.panelExtent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLL_y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLL_x)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUL_y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUL_x)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRight)).BeginInit();
            this.gpTileSize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTileHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTileWidth)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).BeginInit();
            this.gbOrigin.SuspendLayout();
            this.gbTileFormat.SuspendLayout();
            this.SuspendLayout();
            // 
            // gpExtent
            // 
            this.gpExtent.AccessibleDescription = "S";
            this.gpExtent.Controls.Add(this.btnRemoveEpsg);
            this.gpExtent.Controls.Add(this.btnAddEpsg);
            this.gpExtent.Controls.Add(this.panelExtent);
            this.gpExtent.Controls.Add(this.cmbEpsgs);
            this.gpExtent.Controls.Add(this.label7);
            this.gpExtent.Location = new System.Drawing.Point(21, 352);
            this.gpExtent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gpExtent.Name = "gpExtent";
            this.gpExtent.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gpExtent.Size = new System.Drawing.Size(699, 400);
            this.gpExtent.TabIndex = 1;
            this.gpExtent.TabStop = false;
            this.gpExtent.Text = "Spatial Reference / Extent";
            // 
            // btnRemoveEpsg
            // 
            this.btnRemoveEpsg.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRemoveEpsg.Location = new System.Drawing.Point(315, 34);
            this.btnRemoveEpsg.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnRemoveEpsg.Name = "btnRemoveEpsg";
            this.btnRemoveEpsg.Size = new System.Drawing.Size(36, 31);
            this.btnRemoveEpsg.TabIndex = 12;
            this.btnRemoveEpsg.Text = "-";
            this.btnRemoveEpsg.UseVisualStyleBackColor = true;
            this.btnRemoveEpsg.Click += new System.EventHandler(this.btnRemoveEpsg_Click);
            // 
            // btnAddEpsg
            // 
            this.btnAddEpsg.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAddEpsg.Location = new System.Drawing.Point(272, 34);
            this.btnAddEpsg.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAddEpsg.Name = "btnAddEpsg";
            this.btnAddEpsg.Size = new System.Drawing.Size(36, 31);
            this.btnAddEpsg.TabIndex = 11;
            this.btnAddEpsg.Text = "+";
            this.btnAddEpsg.UseVisualStyleBackColor = true;
            this.btnAddEpsg.Click += new System.EventHandler(this.btnAddEpsg_Click);
            // 
            // panelExtent
            // 
            this.panelExtent.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panelExtent.Controls.Add(this.label15);
            this.panelExtent.Controls.Add(this.label14);
            this.panelExtent.Controls.Add(this.label12);
            this.panelExtent.Controls.Add(this.numTop);
            this.panelExtent.Controls.Add(this.numLL_y);
            this.panelExtent.Controls.Add(this.label1);
            this.panelExtent.Controls.Add(this.label13);
            this.panelExtent.Controls.Add(this.numLL_x);
            this.panelExtent.Controls.Add(this.btnImport);
            this.panelExtent.Controls.Add(this.label2);
            this.panelExtent.Controls.Add(this.label11);
            this.panelExtent.Controls.Add(this.numBottom);
            this.panelExtent.Controls.Add(this.numUL_y);
            this.panelExtent.Controls.Add(this.numLeft);
            this.panelExtent.Controls.Add(this.label10);
            this.panelExtent.Controls.Add(this.numUL_x);
            this.panelExtent.Controls.Add(this.label4);
            this.panelExtent.Controls.Add(this.label3);
            this.panelExtent.Controls.Add(this.numRight);
            this.panelExtent.Enabled = false;
            this.panelExtent.Location = new System.Drawing.Point(14, 75);
            this.panelExtent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelExtent.Name = "panelExtent";
            this.panelExtent.Size = new System.Drawing.Size(663, 297);
            this.panelExtent.TabIndex = 8;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label15.Location = new System.Drawing.Point(10, 214);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(200, 20);
            this.label15.TabIndex = 20;
            this.label15.Text = "Lower Left Tilecache Origin";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label14.Location = new System.Drawing.Point(9, 143);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(201, 20);
            this.label14.TabIndex = 19;
            this.label14.Text = "Upper Left Tilecache Origin";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label12.Location = new System.Drawing.Point(327, 246);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(135, 20);
            this.label12.TabIndex = 17;
            this.label12.Text = "Origin Y (Bottom):";
            // 
            // numTop
            // 
            this.numTop.DecimalPlaces = 6;
            this.numTop.Location = new System.Drawing.Point(200, 14);
            this.numTop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.numTop.Size = new System.Drawing.Size(248, 26);
            this.numTop.TabIndex = 1;
            this.numTop.ValueChanged += new System.EventHandler(this.num_ValueChanged);
            // 
            // numLL_y
            // 
            this.numLL_y.DecimalPlaces = 6;
            this.numLL_y.Location = new System.Drawing.Point(470, 243);
            this.numLL_y.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numLL_y.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numLL_y.Minimum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            -2147483648});
            this.numLL_y.Name = "numLL_y";
            this.numLL_y.Size = new System.Drawing.Size(183, 26);
            this.numLL_y.TabIndex = 18;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(152, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Top:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label13.Location = new System.Drawing.Point(9, 248);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(111, 20);
            this.label13.TabIndex = 15;
            this.label13.Text = "Origin X (Left):";
            // 
            // numLL_x
            // 
            this.numLL_x.DecimalPlaces = 6;
            this.numLL_x.Location = new System.Drawing.Point(130, 243);
            this.numLL_x.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numLL_x.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numLL_x.Minimum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            -2147483648});
            this.numLL_x.Name = "numLL_x";
            this.numLL_x.Size = new System.Drawing.Size(186, 26);
            this.numLL_x.TabIndex = 16;
            // 
            // btnImport
            // 
            this.btnImport.BackColor = System.Drawing.SystemColors.Control;
            this.btnImport.Enabled = false;
            this.btnImport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnImport.Location = new System.Drawing.Point(480, 98);
            this.btnImport.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(144, 35);
            this.btnImport.TabIndex = 8;
            this.btnImport.Text = "Import Extent...";
            this.btnImport.UseVisualStyleBackColor = false;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(9, 60);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Left:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label11.Location = new System.Drawing.Point(346, 178);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(110, 20);
            this.label11.TabIndex = 13;
            this.label11.Text = "Origin Y (Top):";
            // 
            // numBottom
            // 
            this.numBottom.DecimalPlaces = 6;
            this.numBottom.Location = new System.Drawing.Point(200, 98);
            this.numBottom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.numBottom.Size = new System.Drawing.Size(248, 26);
            this.numBottom.TabIndex = 7;
            this.numBottom.ValueChanged += new System.EventHandler(this.num_ValueChanged);
            // 
            // numUL_y
            // 
            this.numUL_y.DecimalPlaces = 6;
            this.numUL_y.Location = new System.Drawing.Point(470, 174);
            this.numUL_y.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numUL_y.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numUL_y.Minimum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            -2147483648});
            this.numUL_y.Name = "numUL_y";
            this.numUL_y.Size = new System.Drawing.Size(183, 26);
            this.numUL_y.TabIndex = 14;
            // 
            // numLeft
            // 
            this.numLeft.DecimalPlaces = 6;
            this.numLeft.Location = new System.Drawing.Point(57, 55);
            this.numLeft.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.numLeft.Size = new System.Drawing.Size(248, 26);
            this.numLeft.TabIndex = 3;
            this.numLeft.ValueChanged += new System.EventHandler(this.num_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label10.Location = new System.Drawing.Point(9, 178);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(111, 20);
            this.label10.TabIndex = 11;
            this.label10.Text = "Origin X (Left):";
            // 
            // numUL_x
            // 
            this.numUL_x.DecimalPlaces = 6;
            this.numUL_x.Location = new System.Drawing.Point(130, 174);
            this.numUL_x.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numUL_x.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numUL_x.Minimum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            -2147483648});
            this.numUL_x.Name = "numUL_x";
            this.numUL_x.Size = new System.Drawing.Size(186, 26);
            this.numUL_x.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(138, 103);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Bottom:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(308, 60);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Right:";
            // 
            // numRight
            // 
            this.numRight.DecimalPlaces = 6;
            this.numRight.Location = new System.Drawing.Point(357, 55);
            this.numRight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.numRight.Size = new System.Drawing.Size(248, 26);
            this.numRight.TabIndex = 5;
            this.numRight.ValueChanged += new System.EventHandler(this.num_ValueChanged);
            // 
            // cmbEpsgs
            // 
            this.cmbEpsgs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEpsgs.FormattingEnabled = true;
            this.cmbEpsgs.Location = new System.Drawing.Point(81, 34);
            this.cmbEpsgs.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbEpsgs.Name = "cmbEpsgs";
            this.cmbEpsgs.Size = new System.Drawing.Size(180, 28);
            this.cmbEpsgs.TabIndex = 10;
            this.cmbEpsgs.SelectedIndexChanged += new System.EventHandler(this.cmbEpsgs_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 40);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 20);
            this.label7.TabIndex = 9;
            this.label7.Text = "EPSG:";
            // 
            // chkUseTiling
            // 
            this.chkUseTiling.AutoSize = true;
            this.chkUseTiling.Location = new System.Drawing.Point(21, 18);
            this.chkUseTiling.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkUseTiling.Name = "chkUseTiling";
            this.chkUseTiling.Size = new System.Drawing.Size(221, 24);
            this.chkUseTiling.TabIndex = 2;
            this.chkUseTiling.Text = "Use Tiling with this Service";
            this.chkUseTiling.UseVisualStyleBackColor = true;
            this.chkUseTiling.CheckedChanged += new System.EventHandler(this.chkUseTiling_CheckedChanged);
            // 
            // gpTileSize
            // 
            this.gpTileSize.Controls.Add(this.label9);
            this.gpTileSize.Controls.Add(this.label8);
            this.gpTileSize.Controls.Add(this.numTileHeight);
            this.gpTileSize.Controls.Add(this.numTileWidth);
            this.gpTileSize.Controls.Add(this.label6);
            this.gpTileSize.Controls.Add(this.label5);
            this.gpTileSize.Location = new System.Drawing.Point(492, 62);
            this.gpTileSize.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gpTileSize.Name = "gpTileSize";
            this.gpTileSize.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gpTileSize.Size = new System.Drawing.Size(225, 109);
            this.gpTileSize.TabIndex = 4;
            this.gpTileSize.TabStop = false;
            this.gpTileSize.Text = "Tile Size";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(165, 65);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(25, 20);
            this.label9.TabIndex = 12;
            this.label9.Text = "px";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(165, 31);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(25, 20);
            this.label8.TabIndex = 11;
            this.label8.Text = "px";
            // 
            // numTileHeight
            // 
            this.numTileHeight.Location = new System.Drawing.Point(81, 62);
            this.numTileHeight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numTileHeight.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numTileHeight.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numTileHeight.Name = "numTileHeight";
            this.numTileHeight.Size = new System.Drawing.Size(82, 26);
            this.numTileHeight.TabIndex = 10;
            this.numTileHeight.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numTileHeight.ValueChanged += new System.EventHandler(this.numTileHeight_ValueChanged);
            // 
            // numTileWidth
            // 
            this.numTileWidth.Location = new System.Drawing.Point(81, 23);
            this.numTileWidth.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numTileWidth.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numTileWidth.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numTileWidth.Name = "numTileWidth";
            this.numTileWidth.Size = new System.Drawing.Size(78, 26);
            this.numTileWidth.TabIndex = 9;
            this.numTileWidth.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numTileWidth.ValueChanged += new System.EventHandler(this.numTileWidth_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 65);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 20);
            this.label6.TabIndex = 1;
            this.label6.Text = "Height:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 31);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 20);
            this.label5.TabIndex = 0;
            this.label5.Text = "Width:";
            // 
            // lstScales
            // 
            this.lstScales.FormattingEnabled = true;
            this.lstScales.ItemHeight = 20;
            this.lstScales.Location = new System.Drawing.Point(12, 29);
            this.lstScales.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lstScales.Name = "lstScales";
            this.lstScales.Size = new System.Drawing.Size(248, 204);
            this.lstScales.TabIndex = 5;
            this.lstScales.SelectedIndexChanged += new System.EventHandler(this.lstScales_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblResolutinDpi);
            this.groupBox2.Controls.Add(this.numScale);
            this.groupBox2.Controls.Add(this.btnRemove);
            this.groupBox2.Controls.Add(this.btnAdd);
            this.groupBox2.Controls.Add(this.lstScales);
            this.groupBox2.Location = new System.Drawing.Point(18, 62);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox2.Size = new System.Drawing.Size(460, 282);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Scales";
            // 
            // lblResolutinDpi
            // 
            this.lblResolutinDpi.AutoSize = true;
            this.lblResolutinDpi.Location = new System.Drawing.Point(18, 248);
            this.lblResolutinDpi.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblResolutinDpi.Name = "lblResolutinDpi";
            this.lblResolutinDpi.Size = new System.Drawing.Size(59, 20);
            this.lblResolutinDpi.TabIndex = 10;
            this.lblResolutinDpi.Text = "Resdpi";
            // 
            // numScale
            // 
            this.numScale.DecimalPlaces = 2;
            this.numScale.Location = new System.Drawing.Point(267, 31);
            this.numScale.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numScale.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numScale.Minimum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            -2147483648});
            this.numScale.Name = "numScale";
            this.numScale.Size = new System.Drawing.Size(184, 26);
            this.numScale.TabIndex = 8;
            // 
            // btnRemove
            // 
            this.btnRemove.Enabled = false;
            this.btnRemove.Location = new System.Drawing.Point(270, 117);
            this.btnRemove.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(130, 35);
            this.btnRemove.TabIndex = 7;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(270, 74);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(130, 35);
            this.btnAdd.TabIndex = 6;
            this.btnAdd.Text = "<< Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // chkUpperLeftCacheTiles
            // 
            this.chkUpperLeftCacheTiles.AutoSize = true;
            this.chkUpperLeftCacheTiles.Location = new System.Drawing.Point(302, 29);
            this.chkUpperLeftCacheTiles.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkUpperLeftCacheTiles.Name = "chkUpperLeftCacheTiles";
            this.chkUpperLeftCacheTiles.Size = new System.Drawing.Size(348, 24);
            this.chkUpperLeftCacheTiles.TabIndex = 7;
            this.chkUpperLeftCacheTiles.Text = "Cache tiles (store tiles in TileCacheDirectory)";
            this.chkUpperLeftCacheTiles.UseVisualStyleBackColor = true;
            this.chkUpperLeftCacheTiles.CheckedChanged += new System.EventHandler(this.chkUpperLeftCacheTiles_CheckedChanged);
            // 
            // btnSave
            // 
            this.btnSave.Image = global::gView.Win.Metadata.UI.Properties.Resources.save_16;
            this.btnSave.Location = new System.Drawing.Point(654, 17);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(40, 35);
            this.btnSave.TabIndex = 8;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Image = global::gView.Win.Metadata.UI.Properties.Resources.folder_open_16;
            this.btnLoad.Location = new System.Drawing.Point(604, 17);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(40, 35);
            this.btnLoad.TabIndex = 9;
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // gbOrigin
            // 
            this.gbOrigin.Controls.Add(this.chkLowerLeft);
            this.gbOrigin.Controls.Add(this.chkUpperLeft);
            this.gbOrigin.Controls.Add(this.chkLowerLeftCacheTiles);
            this.gbOrigin.Controls.Add(this.chkUpperLeftCacheTiles);
            this.gbOrigin.Location = new System.Drawing.Point(21, 762);
            this.gbOrigin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbOrigin.Name = "gbOrigin";
            this.gbOrigin.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbOrigin.Size = new System.Drawing.Size(699, 117);
            this.gbOrigin.TabIndex = 10;
            this.gbOrigin.TabStop = false;
            this.gbOrigin.Text = "Origin";
            // 
            // chkLowerLeft
            // 
            this.chkLowerLeft.AutoSize = true;
            this.chkLowerLeft.Location = new System.Drawing.Point(14, 72);
            this.chkLowerLeft.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkLowerLeft.Name = "chkLowerLeft";
            this.chkLowerLeft.Size = new System.Drawing.Size(245, 24);
            this.chkLowerLeft.TabIndex = 10;
            this.chkLowerLeft.Text = "Lower Left (TMS, Openlayers)";
            this.chkLowerLeft.UseVisualStyleBackColor = true;
            this.chkLowerLeft.CheckedChanged += new System.EventHandler(this.chkLowerLeft_CheckedChanged);
            // 
            // chkUpperLeft
            // 
            this.chkUpperLeft.AutoSize = true;
            this.chkUpperLeft.Location = new System.Drawing.Point(14, 29);
            this.chkUpperLeft.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkUpperLeft.Name = "chkUpperLeft";
            this.chkUpperLeft.Size = new System.Drawing.Size(261, 24);
            this.chkUpperLeft.TabIndex = 9;
            this.chkUpperLeft.Text = "Upper Left (WMS-C, OSM-Tiles)";
            this.chkUpperLeft.UseVisualStyleBackColor = true;
            this.chkUpperLeft.CheckedChanged += new System.EventHandler(this.chkUpperLeft_CheckedChanged);
            // 
            // chkLowerLeftCacheTiles
            // 
            this.chkLowerLeftCacheTiles.AutoSize = true;
            this.chkLowerLeftCacheTiles.Location = new System.Drawing.Point(302, 72);
            this.chkLowerLeftCacheTiles.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkLowerLeftCacheTiles.Name = "chkLowerLeftCacheTiles";
            this.chkLowerLeftCacheTiles.Size = new System.Drawing.Size(348, 24);
            this.chkLowerLeftCacheTiles.TabIndex = 8;
            this.chkLowerLeftCacheTiles.Text = "Cache tiles (store tiles in TileCacheDirectory)";
            this.chkLowerLeftCacheTiles.UseVisualStyleBackColor = true;
            this.chkLowerLeftCacheTiles.CheckedChanged += new System.EventHandler(this.chkLowerLeftCacheTiles_CheckedChanged);
            // 
            // gbTileFormat
            // 
            this.gbTileFormat.Controls.Add(this.chkTransparentPng);
            this.gbTileFormat.Controls.Add(this.chkJpg);
            this.gbTileFormat.Controls.Add(this.chkPng);
            this.gbTileFormat.Location = new System.Drawing.Point(492, 174);
            this.gbTileFormat.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbTileFormat.Name = "gbTileFormat";
            this.gbTileFormat.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbTileFormat.Size = new System.Drawing.Size(225, 156);
            this.gbTileFormat.TabIndex = 11;
            this.gbTileFormat.TabStop = false;
            this.gbTileFormat.Text = "Tile Format";
            // 
            // chkJpg
            // 
            this.chkJpg.AutoSize = true;
            this.chkJpg.Location = new System.Drawing.Point(17, 108);
            this.chkJpg.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkJpg.Name = "chkJpg";
            this.chkJpg.Size = new System.Drawing.Size(112, 24);
            this.chkJpg.TabIndex = 1;
            this.chkJpg.Text = "image/jpeg";
            this.chkJpg.UseVisualStyleBackColor = true;
            this.chkJpg.CheckedChanged += new System.EventHandler(this.chkJpg_CheckedChanged);
            // 
            // chkPng
            // 
            this.chkPng.AutoSize = true;
            this.chkPng.Location = new System.Drawing.Point(20, 35);
            this.chkPng.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkPng.Name = "chkPng";
            this.chkPng.Size = new System.Drawing.Size(109, 24);
            this.chkPng.TabIndex = 0;
            this.chkPng.Text = "image/png";
            this.chkPng.UseVisualStyleBackColor = true;
            this.chkPng.CheckedChanged += new System.EventHandler(this.chkPng_CheckedChanged);
            // 
            // chkRenderOnTheFly
            // 
            this.chkRenderOnTheFly.AutoSize = true;
            this.chkRenderOnTheFly.Location = new System.Drawing.Point(268, 17);
            this.chkRenderOnTheFly.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkRenderOnTheFly.Name = "chkRenderOnTheFly";
            this.chkRenderOnTheFly.Size = new System.Drawing.Size(197, 24);
            this.chkRenderOnTheFly.TabIndex = 2;
            this.chkRenderOnTheFly.Text = "Render Tiles On-the-fly";
            this.chkRenderOnTheFly.UseVisualStyleBackColor = true;
            this.chkRenderOnTheFly.CheckedChanged += new System.EventHandler(this.chkRenderOnTheFly_CheckedChanged);
            // 
            // chkTransparentPng
            // 
            this.chkTransparentPng.AutoSize = true;
            this.chkTransparentPng.Location = new System.Drawing.Point(50, 64);
            this.chkTransparentPng.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkTransparentPng.Name = "chkTransparentPng";
            this.chkTransparentPng.Size = new System.Drawing.Size(158, 24);
            this.chkTransparentPng.TabIndex = 2;
            this.chkTransparentPng.Text = "Transparent BGr.";
            this.chkTransparentPng.UseVisualStyleBackColor = true;
            this.chkTransparentPng.CheckedChanged += new System.EventHandler(this.chkTransparentPng_CheckedChanged);
            // 
            // TileServiceMetadataControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkRenderOnTheFly);
            this.Controls.Add(this.gbTileFormat);
            this.Controls.Add(this.gbOrigin);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.gpTileSize);
            this.Controls.Add(this.chkUseTiling);
            this.Controls.Add(this.gpExtent);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "TileServiceMetadataControl";
            this.Size = new System.Drawing.Size(736, 883);
            this.gpExtent.ResumeLayout(false);
            this.gpExtent.PerformLayout();
            this.panelExtent.ResumeLayout(false);
            this.panelExtent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLL_y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLL_x)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUL_y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUL_x)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRight)).EndInit();
            this.gpTileSize.ResumeLayout(false);
            this.gpTileSize.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTileHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTileWidth)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).EndInit();
            this.gbOrigin.ResumeLayout(false);
            this.gbOrigin.PerformLayout();
            this.gbTileFormat.ResumeLayout(false);
            this.gbTileFormat.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gpExtent;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.NumericUpDown numBottom;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numRight;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numLeft;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numTop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkUseTiling;
        private System.Windows.Forms.GroupBox gpTileSize;
        private System.Windows.Forms.ListBox lstScales;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown numScale;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.NumericUpDown numTileHeight;
        private System.Windows.Forms.NumericUpDown numTileWidth;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkUpperLeftCacheTiles;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnRemoveEpsg;
        private System.Windows.Forms.Button btnAddEpsg;
        private System.Windows.Forms.Panel panelExtent;
        private System.Windows.Forms.ComboBox cmbEpsgs;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.GroupBox gbOrigin;
        private System.Windows.Forms.CheckBox chkLowerLeft;
        private System.Windows.Forms.CheckBox chkUpperLeft;
        private System.Windows.Forms.CheckBox chkLowerLeftCacheTiles;
        private System.Windows.Forms.GroupBox gbTileFormat;
        private System.Windows.Forms.CheckBox chkJpg;
        private System.Windows.Forms.CheckBox chkPng;
        private System.Windows.Forms.CheckBox chkRenderOnTheFly;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown numLL_y;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.NumericUpDown numLL_x;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown numUL_y;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numUL_x;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblResolutinDpi;
        private System.Windows.Forms.CheckBox chkTransparentPng;
    }
}

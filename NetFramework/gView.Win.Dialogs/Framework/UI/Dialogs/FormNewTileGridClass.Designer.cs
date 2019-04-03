namespace gView.Framework.UI.Dialogs
{
    partial class FormNewTileGridClass
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            gView.Framework.Geometry.Envelope envelope1 = new gView.Framework.Geometry.Envelope();
            gView.Framework.Geometry.Point point1 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point2 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point3 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point4 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point5 = new gView.Framework.Geometry.Point();
            gView.Framework.Data.gViewSpatialIndexDef gViewSpatialIndexDef1 = new gView.Framework.Data.gViewSpatialIndexDef();
            gView.Framework.Geometry.Envelope envelope2 = new gView.Framework.Geometry.Envelope();
            gView.Framework.Geometry.Point point6 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point7 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point8 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point9 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point10 = new gView.Framework.Geometry.Point();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnLevelProperties = new System.Windows.Forms.Button();
            this.cmbLevelType = new System.Windows.Forms.ComboBox();
            this.numLevels = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numTileSizeX = new System.Windows.Forms.NumericUpDown();
            this.numTileSizeY = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numResolutionX = new System.Windows.Forms.NumericUpDown();
            this.numResolutionY = new System.Windows.Forms.NumericUpDown();
            this.cmbTileType = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.chkCreateTiles = new System.Windows.Forms.CheckBox();
            this.btnGetCacheDirectory = new System.Windows.Forms.Button();
            this.txtCacheDirectory = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtGridDataset = new System.Windows.Forms.TextBox();
            this.btnGetGridDataset = new System.Windows.Forms.Button();
            this.btnImportFromGridDef = new System.Windows.Forms.Button();
            this.spatialIndexControl1 = new gView.Framework.UI.Controls.SpatialIndexControl();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLevels)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTileSizeX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTileSizeY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numResolutionX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numResolutionY)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(308, 455);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(13, 455);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Grid Dataset:";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 151);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(371, 298);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.spatialIndexControl1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(363, 272);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Spatial Index/Grid Extent";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnLevelProperties);
            this.tabPage2.Controls.Add(this.cmbLevelType);
            this.tabPage2.Controls.Add(this.numLevels);
            this.tabPage2.Controls.Add(this.label10);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Controls.Add(this.cmbTileType);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.chkCreateTiles);
            this.tabPage2.Controls.Add(this.btnGetCacheDirectory);
            this.tabPage2.Controls.Add(this.txtCacheDirectory);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(363, 272);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Tiles";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnLevelProperties
            // 
            this.btnLevelProperties.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLevelProperties.Location = new System.Drawing.Point(313, 114);
            this.btnLevelProperties.Name = "btnLevelProperties";
            this.btnLevelProperties.Size = new System.Drawing.Size(32, 20);
            this.btnLevelProperties.TabIndex = 24;
            this.btnLevelProperties.Text = "...";
            this.btnLevelProperties.UseVisualStyleBackColor = true;
            this.btnLevelProperties.Click += new System.EventHandler(this.btnLevelProperties_Click);
            // 
            // cmbLevelType
            // 
            this.cmbLevelType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevelType.FormattingEnabled = true;
            this.cmbLevelType.Items.AddRange(new object[] {
            "Constant Imagesize",
            "Constant Geographic Tilesize"});
            this.cmbLevelType.Location = new System.Drawing.Point(129, 114);
            this.cmbLevelType.Name = "cmbLevelType";
            this.cmbLevelType.Size = new System.Drawing.Size(178, 21);
            this.cmbLevelType.TabIndex = 22;
            // 
            // numLevels
            // 
            this.numLevels.Location = new System.Drawing.Point(54, 114);
            this.numLevels.Maximum = new decimal(new int[] {
            -727379969,
            232,
            0,
            0});
            this.numLevels.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numLevels.Name = "numLevels";
            this.numLevels.Size = new System.Drawing.Size(52, 20);
            this.numLevels.TabIndex = 10;
            this.numLevels.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numLevels.ValueChanged += new System.EventHandler(this.numLevels_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(104, 117);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(26, 13);
            this.label10.TabIndex = 23;
            this.label10.Text = "with";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numTileSizeX);
            this.groupBox1.Controls.Add(this.numTileSizeY);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.numResolutionX);
            this.groupBox1.Controls.Add(this.numResolutionY);
            this.groupBox1.Location = new System.Drawing.Point(15, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(332, 77);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Geographic Tilesize/Resolution";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Tile Size X:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Tile Size Y:";
            // 
            // numTileSizeX
            // 
            this.numTileSizeX.DecimalPlaces = 2;
            this.numTileSizeX.Location = new System.Drawing.Point(74, 24);
            this.numTileSizeX.Maximum = new decimal(new int[] {
            -727379969,
            232,
            0,
            0});
            this.numTileSizeX.Name = "numTileSizeX";
            this.numTileSizeX.Size = new System.Drawing.Size(67, 20);
            this.numTileSizeX.TabIndex = 7;
            this.numTileSizeX.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // numTileSizeY
            // 
            this.numTileSizeY.DecimalPlaces = 2;
            this.numTileSizeY.Location = new System.Drawing.Point(74, 48);
            this.numTileSizeY.Maximum = new decimal(new int[] {
            -727379969,
            232,
            0,
            0});
            this.numTileSizeY.Name = "numTileSizeY";
            this.numTileSizeY.Size = new System.Drawing.Size(67, 20);
            this.numTileSizeY.TabIndex = 8;
            this.numTileSizeY.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(163, 50);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(70, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Resolution Y:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(163, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Resolution X:";
            // 
            // numResolutionX
            // 
            this.numResolutionX.DecimalPlaces = 3;
            this.numResolutionX.Location = new System.Drawing.Point(235, 24);
            this.numResolutionX.Maximum = new decimal(new int[] {
            -727379969,
            232,
            0,
            0});
            this.numResolutionX.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            458752});
            this.numResolutionX.Name = "numResolutionX";
            this.numResolutionX.Size = new System.Drawing.Size(67, 20);
            this.numResolutionX.TabIndex = 13;
            this.numResolutionX.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numResolutionY
            // 
            this.numResolutionY.DecimalPlaces = 3;
            this.numResolutionY.Location = new System.Drawing.Point(235, 48);
            this.numResolutionY.Maximum = new decimal(new int[] {
            -727379969,
            232,
            0,
            0});
            this.numResolutionY.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            458752});
            this.numResolutionY.Name = "numResolutionY";
            this.numResolutionY.Size = new System.Drawing.Size(67, 20);
            this.numResolutionY.TabIndex = 14;
            this.numResolutionY.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cmbTileType
            // 
            this.cmbTileType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTileType.FormattingEnabled = true;
            this.cmbTileType.Items.AddRange(new object[] {
            "image/jpg\t",
            "image/png",
            "binary/float"});
            this.cmbTileType.Location = new System.Drawing.Point(72, 223);
            this.cmbTileType.Name = "cmbTileType";
            this.cmbTileType.Size = new System.Drawing.Size(121, 21);
            this.cmbTileType.TabIndex = 20;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 226);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(54, 13);
            this.label9.TabIndex = 19;
            this.label9.Text = "Tile Type:";
            // 
            // chkCreateTiles
            // 
            this.chkCreateTiles.AutoSize = true;
            this.chkCreateTiles.Checked = true;
            this.chkCreateTiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCreateTiles.Location = new System.Drawing.Point(15, 188);
            this.chkCreateTiles.Name = "chkCreateTiles";
            this.chkCreateTiles.Size = new System.Drawing.Size(124, 17);
            this.chkCreateTiles.TabIndex = 18;
            this.chkCreateTiles.Text = "Generate Tile Cache";
            this.chkCreateTiles.UseVisualStyleBackColor = true;
            // 
            // btnGetCacheDirectory
            // 
            this.btnGetCacheDirectory.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnGetCacheDirectory.Location = new System.Drawing.Point(313, 162);
            this.btnGetCacheDirectory.Name = "btnGetCacheDirectory";
            this.btnGetCacheDirectory.Size = new System.Drawing.Size(32, 20);
            this.btnGetCacheDirectory.TabIndex = 17;
            this.btnGetCacheDirectory.Text = "...";
            this.btnGetCacheDirectory.UseVisualStyleBackColor = true;
            this.btnGetCacheDirectory.Click += new System.EventHandler(this.btnGetCacheDirectory_Click);
            // 
            // txtCacheDirectory
            // 
            this.txtCacheDirectory.Location = new System.Drawing.Point(15, 162);
            this.txtCacheDirectory.Name = "txtCacheDirectory";
            this.txtCacheDirectory.Size = new System.Drawing.Size(292, 20);
            this.txtCacheDirectory.TabIndex = 16;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 145);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(86, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Cache Directory:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 117);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Levels:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(44, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(88, 18);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(275, 20);
            this.txtName.TabIndex = 8;
            this.txtName.Text = "GRID1";
            // 
            // txtGridDataset
            // 
            this.txtGridDataset.Location = new System.Drawing.Point(88, 52);
            this.txtGridDataset.Name = "txtGridDataset";
            this.txtGridDataset.Size = new System.Drawing.Size(210, 20);
            this.txtGridDataset.TabIndex = 9;
            // 
            // btnGetGridDataset
            // 
            this.btnGetGridDataset.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnGetGridDataset.Location = new System.Drawing.Point(298, 52);
            this.btnGetGridDataset.Name = "btnGetGridDataset";
            this.btnGetGridDataset.Size = new System.Drawing.Size(65, 20);
            this.btnGetGridDataset.TabIndex = 10;
            this.btnGetGridDataset.Text = "...";
            this.btnGetGridDataset.UseVisualStyleBackColor = true;
            this.btnGetGridDataset.Click += new System.EventHandler(this.btnGetGridDataset_Click);
            // 
            // btnImportFromGridDef
            // 
            this.btnImportFromGridDef.Location = new System.Drawing.Point(208, 144);
            this.btnImportFromGridDef.Name = "btnImportFromGridDef";
            this.btnImportFromGridDef.Size = new System.Drawing.Size(172, 23);
            this.btnImportFromGridDef.TabIndex = 11;
            this.btnImportFromGridDef.Text = "Import from Grid Definition";
            this.btnImportFromGridDef.UseVisualStyleBackColor = true;
            this.btnImportFromGridDef.Click += new System.EventHandler(this.btnImportFromGridDef_Click);
            // 
            // spatialIndexControl1
            // 
            point1.M = 0;
            point1.X = 0;
            point1.Y = 0;
            point1.Z = 0;
            envelope1.Center = point1;
            point2.M = 0;
            point2.X = 0;
            point2.Y = 0;
            point2.Z = 0;
            envelope1.LowerLeft = point2;
            point3.M = 0;
            point3.X = 0;
            point3.Y = 0;
            point3.Z = 0;
            envelope1.LowerRight = point3;
            envelope1.maxx = 0;
            envelope1.maxy = 0;
            envelope1.minx = 0;
            envelope1.miny = 0;
            point4.M = 0;
            point4.X = 0;
            point4.Y = 0;
            point4.Z = 0;
            envelope1.UpperLeft = point4;
            point5.M = 0;
            point5.X = 0;
            point5.Y = 0;
            point5.Z = 0;
            envelope1.UpperRight = point5;
            this.spatialIndexControl1.Extent = envelope1;
            this.spatialIndexControl1.IndexTypeIsEditable = false;
            this.spatialIndexControl1.Levels = 10;
            this.spatialIndexControl1.Location = new System.Drawing.Point(6, 6);
            this.spatialIndexControl1.MSIndex = null;
            this.spatialIndexControl1.Name = "spatialIndexControl1";
            this.spatialIndexControl1.Size = new System.Drawing.Size(351, 273);
            gViewSpatialIndexDef1.Levels = 10;
            gViewSpatialIndexDef1.MaxPerNode = 200;
            point6.M = 0;
            point6.X = 0;
            point6.Y = 0;
            point6.Z = 0;
            envelope2.Center = point6;
            point7.M = 0;
            point7.X = 0;
            point7.Y = 0;
            point7.Z = 0;
            envelope2.LowerLeft = point7;
            point8.M = 0;
            point8.X = 0;
            point8.Y = 0;
            point8.Z = 0;
            envelope2.LowerRight = point8;
            envelope2.maxx = 0;
            envelope2.maxy = 0;
            envelope2.minx = 0;
            envelope2.miny = 0;
            point9.M = 0;
            point9.X = 0;
            point9.Y = 0;
            point9.Z = 0;
            envelope2.UpperLeft = point9;
            point10.M = 0;
            point10.X = 0;
            point10.Y = 0;
            point10.Z = 0;
            envelope2.UpperRight = point10;
            gViewSpatialIndexDef1.SpatialIndexBounds = envelope2;
            gViewSpatialIndexDef1.SpatialReference = null;
            gViewSpatialIndexDef1.SplitRatio = 0.55;
            this.spatialIndexControl1.SpatialIndexDef = gViewSpatialIndexDef1;
            this.spatialIndexControl1.TabIndex = 0;
            this.spatialIndexControl1.Type = gView.Framework.UI.Controls.SpatialIndexControl.IndexType.gView;
            // 
            // FormNewTileGridClass
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 490);
            this.Controls.Add(this.btnImportFromGridDef);
            this.Controls.Add(this.btnGetGridDataset);
            this.Controls.Add(this.txtGridDataset);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormNewTileGridClass";
            this.Text = "New Tile Grid Class";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLevels)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTileSizeX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTileSizeY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numResolutionX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numResolutionY)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private gView.Framework.UI.Controls.SpatialIndexControl spatialIndexControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtGridDataset;
        private System.Windows.Forms.Button btnGetGridDataset;
        private System.Windows.Forms.NumericUpDown numLevels;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numTileSizeY;
        private System.Windows.Forms.NumericUpDown numTileSizeX;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numResolutionY;
        private System.Windows.Forms.NumericUpDown numResolutionX;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnGetCacheDirectory;
        private System.Windows.Forms.TextBox txtCacheDirectory;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkCreateTiles;
        private System.Windows.Forms.ComboBox cmbTileType;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnImportFromGridDef;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbLevelType;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnLevelProperties;
    }
}
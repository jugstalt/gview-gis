namespace gView.DataSources.TileCache.UI
{
    partial class FormTileCacheConnection
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
            this.gbOrigin = new System.Windows.Forms.GroupBox();
            this.cmbOrigin = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.panelExtent = new System.Windows.Forms.Panel();
            this.numTop = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.btnImport = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.numBottom = new System.Windows.Forms.NumericUpDown();
            this.numLeft = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numRight = new System.Windows.Forms.NumericUpDown();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSpatialReference = new System.Windows.Forms.Button();
            this.txtSpatialReference = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.numScale = new System.Windows.Forms.NumericUpDown();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lstScales = new System.Windows.Forms.ListBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtTileUrl = new System.Windows.Forms.TextBox();
            this.gpTileSize = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.numTileHeight = new System.Windows.Forms.NumericUpDown();
            this.numTileWidth = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.txtCopyright = new System.Windows.Forms.TextBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.gbOrigin.SuspendLayout();
            this.panelExtent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRight)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.gpTileSize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTileHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTileWidth)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbOrigin
            // 
            this.gbOrigin.Controls.Add(this.cmbOrigin);
            this.gbOrigin.Location = new System.Drawing.Point(22, 187);
            this.gbOrigin.Name = "gbOrigin";
            this.gbOrigin.Size = new System.Drawing.Size(416, 56);
            this.gbOrigin.TabIndex = 2;
            this.gbOrigin.TabStop = false;
            this.gbOrigin.Text = "Origin";
            // 
            // cmbOrigin
            // 
            this.cmbOrigin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbOrigin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOrigin.FormattingEnabled = true;
            this.cmbOrigin.Items.AddRange(new object[] {
            "Lower Left (TMS, Openlayers)",
            "Upper Left (WMS-C, OSM-Tiles)"});
            this.cmbOrigin.Location = new System.Drawing.Point(15, 19);
            this.cmbOrigin.Name = "cmbOrigin";
            this.cmbOrigin.Size = new System.Drawing.Size(380, 21);
            this.cmbOrigin.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(383, 388);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 26);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(55, 54);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(409, 20);
            this.txtName.TabIndex = 5;
            this.txtName.Text = "Tile_Cache1";
            // 
            // panelExtent
            // 
            this.panelExtent.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panelExtent.Controls.Add(this.numTop);
            this.panelExtent.Controls.Add(this.label2);
            this.panelExtent.Controls.Add(this.btnImport);
            this.panelExtent.Controls.Add(this.label3);
            this.panelExtent.Controls.Add(this.numBottom);
            this.panelExtent.Controls.Add(this.numLeft);
            this.panelExtent.Controls.Add(this.label4);
            this.panelExtent.Controls.Add(this.label5);
            this.panelExtent.Controls.Add(this.numRight);
            this.panelExtent.Location = new System.Drawing.Point(22, 18);
            this.panelExtent.Name = "panelExtent";
            this.panelExtent.Size = new System.Drawing.Size(420, 94);
            this.panelExtent.TabIndex = 8;
            // 
            // numTop
            // 
            this.numTop.DecimalPlaces = 6;
            this.numTop.Location = new System.Drawing.Point(133, 9);
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
            this.numTop.Size = new System.Drawing.Size(165, 20);
            this.numTop.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(101, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Top:";
            // 
            // btnImport
            // 
            this.btnImport.BackColor = System.Drawing.SystemColors.Control;
            this.btnImport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnImport.Location = new System.Drawing.Point(320, 64);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(96, 23);
            this.btnImport.TabIndex = 8;
            this.btnImport.Text = "Import Extent...";
            this.btnImport.UseVisualStyleBackColor = false;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(6, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Left:";
            // 
            // numBottom
            // 
            this.numBottom.DecimalPlaces = 6;
            this.numBottom.Location = new System.Drawing.Point(133, 64);
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
            this.numBottom.Size = new System.Drawing.Size(165, 20);
            this.numBottom.TabIndex = 7;
            // 
            // numLeft
            // 
            this.numLeft.DecimalPlaces = 6;
            this.numLeft.Location = new System.Drawing.Point(38, 36);
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
            this.numLeft.Size = new System.Drawing.Size(165, 20);
            this.numLeft.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(92, 67);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Bottom:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(205, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Right:";
            // 
            // numRight
            // 
            this.numRight.DecimalPlaces = 6;
            this.numRight.Location = new System.Drawing.Point(238, 36);
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
            this.numRight.Size = new System.Drawing.Size(165, 20);
            this.numRight.TabIndex = 5;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(6, 90);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(458, 292);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.panelExtent);
            this.tabPage1.Controls.Add(this.gbOrigin);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(450, 266);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Cache Extent Definition";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnSpatialReference);
            this.groupBox1.Controls.Add(this.txtSpatialReference);
            this.groupBox1.Location = new System.Drawing.Point(22, 127);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(416, 54);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Spatial Reference System";
            // 
            // btnSpatialReference
            // 
            this.btnSpatialReference.Location = new System.Drawing.Point(335, 17);
            this.btnSpatialReference.Name = "btnSpatialReference";
            this.btnSpatialReference.Size = new System.Drawing.Size(68, 23);
            this.btnSpatialReference.TabIndex = 9;
            this.btnSpatialReference.Text = "...";
            this.btnSpatialReference.UseVisualStyleBackColor = true;
            this.btnSpatialReference.Click += new System.EventHandler(this.btnSpatialReference_Click);
            // 
            // txtSpatialReference
            // 
            this.txtSpatialReference.Location = new System.Drawing.Point(15, 19);
            this.txtSpatialReference.Name = "txtSpatialReference";
            this.txtSpatialReference.ReadOnly = true;
            this.txtSpatialReference.Size = new System.Drawing.Size(314, 20);
            this.txtSpatialReference.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.numScale);
            this.tabPage2.Controls.Add(this.btnRemove);
            this.tabPage2.Controls.Add(this.btnAdd);
            this.tabPage2.Controls.Add(this.lstScales);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(450, 266);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Scales / Levels";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // numScale
            // 
            this.numScale.DecimalPlaces = 2;
            this.numScale.Location = new System.Drawing.Point(241, 6);
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
            this.numScale.Size = new System.Drawing.Size(123, 20);
            this.numScale.TabIndex = 12;
            // 
            // btnRemove
            // 
            this.btnRemove.Enabled = false;
            this.btnRemove.Location = new System.Drawing.Point(243, 62);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(87, 23);
            this.btnRemove.TabIndex = 11;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(243, 34);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(87, 23);
            this.btnAdd.TabIndex = 10;
            this.btnAdd.Text = "<< Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // lstScales
            // 
            this.lstScales.FormattingEnabled = true;
            this.lstScales.Location = new System.Drawing.Point(7, 6);
            this.lstScales.Name = "lstScales";
            this.lstScales.Size = new System.Drawing.Size(230, 251);
            this.lstScales.TabIndex = 9;
            this.lstScales.SelectedIndexChanged += new System.EventHandler(this.lstScales_SelectedIndexChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox2);
            this.tabPage3.Controls.Add(this.gpTileSize);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(450, 266);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Tiles (Size & Url)";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.txtTileUrl);
            this.groupBox2.Location = new System.Drawing.Point(7, 71);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(440, 176);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Tile Url";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(13, 96);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(195, 52);
            this.label10.TabIndex = 1;
            this.label10.Text = "Use the following placeholders:\r\n{2} ... integer number of level\r\n{0},{1} ... int" +
                "eger number of column, row\r\n{3} ... Bing Maps Quadkey";
            // 
            // txtTileUrl
            // 
            this.txtTileUrl.Location = new System.Drawing.Point(13, 20);
            this.txtTileUrl.Multiline = true;
            this.txtTileUrl.Name = "txtTileUrl";
            this.txtTileUrl.Size = new System.Drawing.Size(409, 69);
            this.txtTileUrl.TabIndex = 0;
            // 
            // gpTileSize
            // 
            this.gpTileSize.Controls.Add(this.label9);
            this.gpTileSize.Controls.Add(this.label8);
            this.gpTileSize.Controls.Add(this.numTileHeight);
            this.gpTileSize.Controls.Add(this.numTileWidth);
            this.gpTileSize.Controls.Add(this.label6);
            this.gpTileSize.Controls.Add(this.label7);
            this.gpTileSize.Location = new System.Drawing.Point(7, 19);
            this.gpTileSize.Name = "gpTileSize";
            this.gpTileSize.Size = new System.Drawing.Size(440, 46);
            this.gpTileSize.TabIndex = 5;
            this.gpTileSize.TabStop = false;
            this.gpTileSize.Text = "Tile Size";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(311, 20);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(18, 13);
            this.label9.TabIndex = 12;
            this.label9.Text = "px";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(134, 20);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(18, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "px";
            // 
            // numTileHeight
            // 
            this.numTileHeight.Location = new System.Drawing.Point(221, 15);
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
            this.numTileHeight.Size = new System.Drawing.Size(87, 20);
            this.numTileHeight.TabIndex = 10;
            this.numTileHeight.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // numTileWidth
            // 
            this.numTileWidth.Location = new System.Drawing.Point(54, 15);
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
            this.numTileWidth.Size = new System.Drawing.Size(77, 20);
            this.numTileWidth.TabIndex = 9;
            this.numTileWidth.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(174, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Height:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 20);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Width:";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.txtCopyright);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(450, 266);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Copyright Information";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // txtCopyright
            // 
            this.txtCopyright.Location = new System.Drawing.Point(7, 3);
            this.txtCopyright.Multiline = true;
            this.txtCopyright.Name = "txtCopyright";
            this.txtCopyright.Size = new System.Drawing.Size(310, 255);
            this.txtCopyright.TabIndex = 0;
            // 
            // btnLoad
            // 
            this.btnLoad.BackgroundImage = global::gView.Win.DataSources.TileCache.UI.Properties.Resources.folder_open_16;
            this.btnLoad.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnLoad.Location = new System.Drawing.Point(10, 12);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(126, 23);
            this.btnLoad.TabIndex = 11;
            this.btnLoad.Text = "Import...";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImage = global::gView.Win.DataSources.TileCache.UI.Properties.Resources.save_16;
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnSave.Location = new System.Drawing.Point(352, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(112, 23);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "Save...";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // FormTileCacheConnection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 425);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormTileCacheConnection";
            this.Text = "Tile Cache Connection";
            this.gbOrigin.ResumeLayout(false);
            this.panelExtent.ResumeLayout(false);
            this.panelExtent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRight)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.gpTileSize.ResumeLayout(false);
            this.gpTileSize.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTileHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTileWidth)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbOrigin;
        private System.Windows.Forms.ComboBox cmbOrigin;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Panel panelExtent;
        private System.Windows.Forms.NumericUpDown numTop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numBottom;
        private System.Windows.Forms.NumericUpDown numLeft;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numRight;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSpatialReference;
        private System.Windows.Forms.TextBox txtSpatialReference;
        private System.Windows.Forms.NumericUpDown numScale;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ListBox lstScales;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtTileUrl;
        private System.Windows.Forms.GroupBox gpTileSize;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numTileHeight;
        private System.Windows.Forms.NumericUpDown numTileWidth;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox txtCopyright;
    }
}
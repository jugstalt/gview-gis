namespace gView.MapServer.Lib.UI
{
    partial class FormPreRenderTiles
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
            this.lstScales = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtServer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtService = new System.Windows.Forms.TextBox();
            this.btnPreRender = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.cmbOrigin = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cmbEpsg = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbImageFormat = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numMaxParallelRequest = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panelExtent = new System.Windows.Forms.Panel();
            this.numRight = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.numLeft = new System.Windows.Forms.NumericUpDown();
            this.numBottom = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.btnImport = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.numTop = new System.Windows.Forms.NumericUpDown();
            this.chkUseExtent = new System.Windows.Forms.CheckBox();
            this.lblEpsg = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.cmbCacheFormat = new System.Windows.Forms.ComboBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxParallelRequest)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.panelExtent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTop)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstScales
            // 
            this.lstScales.CheckBoxes = true;
            this.lstScales.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.lstScales.FullRowSelect = true;
            this.lstScales.GridLines = true;
            this.lstScales.Location = new System.Drawing.Point(10, 49);
            this.lstScales.Name = "lstScales";
            this.lstScales.Size = new System.Drawing.Size(454, 226);
            this.lstScales.TabIndex = 0;
            this.lstScales.UseCompatibleStateImageBehavior = false;
            this.lstScales.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Scale";
            this.columnHeader1.Width = 173;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Rows";
            this.columnHeader2.Width = 72;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Columns";
            this.columnHeader3.Width = 69;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Tiles";
            this.columnHeader4.Width = 111;
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(99, 17);
            this.txtServer.Name = "txtServer";
            this.txtServer.ReadOnly = true;
            this.txtServer.Size = new System.Drawing.Size(112, 20);
            this.txtServer.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Server-Instance:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(223, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Service:";
            // 
            // txtService
            // 
            this.txtService.Location = new System.Drawing.Point(270, 17);
            this.txtService.Name = "txtService";
            this.txtService.ReadOnly = true;
            this.txtService.Size = new System.Drawing.Size(114, 20);
            this.txtService.TabIndex = 3;
            // 
            // btnPreRender
            // 
            this.btnPreRender.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPreRender.Location = new System.Drawing.Point(374, 357);
            this.btnPreRender.Name = "btnPreRender";
            this.btnPreRender.Size = new System.Drawing.Size(116, 23);
            this.btnPreRender.TabIndex = 5;
            this.btnPreRender.Text = "Pre-Render Now";
            this.btnPreRender.UseVisualStyleBackColor = true;
            this.btnPreRender.Click += new System.EventHandler(this.btnPreRender_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(12, 357);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(478, 339);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cmbOrigin);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.cmbEpsg);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.cmbImageFormat);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.txtServer);
            this.tabPage1.Controls.Add(this.lstScales);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.txtService);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(470, 313);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // cmbOrigin
            // 
            this.cmbOrigin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOrigin.FormattingEnabled = true;
            this.cmbOrigin.Location = new System.Drawing.Point(350, 281);
            this.cmbOrigin.Name = "cmbOrigin";
            this.cmbOrigin.Size = new System.Drawing.Size(114, 21);
            this.cmbOrigin.TabIndex = 10;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(310, 285);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(37, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "Origin:";
            // 
            // cmbEpsg
            // 
            this.cmbEpsg.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEpsg.FormattingEnabled = true;
            this.cmbEpsg.Location = new System.Drawing.Point(54, 281);
            this.cmbEpsg.Name = "cmbEpsg";
            this.cmbEpsg.Size = new System.Drawing.Size(100, 21);
            this.cmbEpsg.TabIndex = 8;
            this.cmbEpsg.SelectedIndexChanged += new System.EventHandler(this.cmbEpsg_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 285);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "EPSG:";
            // 
            // cmbImageFormat
            // 
            this.cmbImageFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbImageFormat.FormattingEnabled = true;
            this.cmbImageFormat.Location = new System.Drawing.Point(203, 281);
            this.cmbImageFormat.Name = "cmbImageFormat";
            this.cmbImageFormat.Size = new System.Drawing.Size(99, 21);
            this.cmbImageFormat.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(160, 285);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Format:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numMaxParallelRequest);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(9, 183);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 57);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Performance";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(165, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Maximum parallel service request:";
            // 
            // numMaxParallelRequest
            // 
            this.numMaxParallelRequest.Location = new System.Drawing.Point(185, 26);
            this.numMaxParallelRequest.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxParallelRequest.Name = "numMaxParallelRequest";
            this.numMaxParallelRequest.Size = new System.Drawing.Size(61, 20);
            this.numMaxParallelRequest.TabIndex = 1;
            this.numMaxParallelRequest.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblEpsg);
            this.groupBox2.Controls.Add(this.chkUseExtent);
            this.groupBox2.Controls.Add(this.panelExtent);
            this.groupBox2.Location = new System.Drawing.Point(6, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(425, 171);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Extent";
            // 
            // panelExtent
            // 
            this.panelExtent.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panelExtent.Controls.Add(this.numTop);
            this.panelExtent.Controls.Add(this.label6);
            this.panelExtent.Controls.Add(this.btnImport);
            this.panelExtent.Controls.Add(this.label7);
            this.panelExtent.Controls.Add(this.numBottom);
            this.panelExtent.Controls.Add(this.numLeft);
            this.panelExtent.Controls.Add(this.label8);
            this.panelExtent.Controls.Add(this.label9);
            this.panelExtent.Controls.Add(this.numRight);
            this.panelExtent.Location = new System.Drawing.Point(6, 42);
            this.panelExtent.Name = "panelExtent";
            this.panelExtent.Size = new System.Drawing.Size(409, 120);
            this.panelExtent.TabIndex = 9;
            // 
            // numRight
            // 
            this.numRight.DecimalPlaces = 6;
            this.numRight.Location = new System.Drawing.Point(235, 36);
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
            this.numRight.Size = new System.Drawing.Size(160, 20);
            this.numRight.TabIndex = 5;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label9.Location = new System.Drawing.Point(202, 39);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(35, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "Right:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8.Location = new System.Drawing.Point(92, 67);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(43, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Bottom:";
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
            this.numLeft.Size = new System.Drawing.Size(160, 20);
            this.numLeft.TabIndex = 3;
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
            this.numBottom.Size = new System.Drawing.Size(160, 20);
            this.numBottom.TabIndex = 7;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Location = new System.Drawing.Point(6, 39);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(28, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Left:";
            // 
            // btnImport
            // 
            this.btnImport.BackColor = System.Drawing.SystemColors.Control;
            this.btnImport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnImport.Location = new System.Drawing.Point(263, 90);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(135, 23);
            this.btnImport.TabIndex = 8;
            this.btnImport.Text = "Import Extent...";
            this.btnImport.UseVisualStyleBackColor = false;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(101, 12);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Top:";
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
            this.numTop.Size = new System.Drawing.Size(160, 20);
            this.numTop.TabIndex = 1;
            // 
            // chkUseExtent
            // 
            this.chkUseExtent.AutoSize = true;
            this.chkUseExtent.Location = new System.Drawing.Point(6, 19);
            this.chkUseExtent.Name = "chkUseExtent";
            this.chkUseExtent.Size = new System.Drawing.Size(168, 17);
            this.chkUseExtent.TabIndex = 10;
            this.chkUseExtent.Text = "Pre-Render Tiles in this Extent";
            this.chkUseExtent.UseVisualStyleBackColor = true;
            // 
            // lblEpsg
            // 
            this.lblEpsg.AutoSize = true;
            this.lblEpsg.Location = new System.Drawing.Point(179, 21);
            this.lblEpsg.Name = "lblEpsg";
            this.lblEpsg.Size = new System.Drawing.Size(51, 13);
            this.lblEpsg.TabIndex = 11;
            this.lblEpsg.Text = "(EPSG:0)";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cmbCacheFormat);
            this.groupBox3.Location = new System.Drawing.Point(283, 190);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(148, 50);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Tile Cache Format";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox3);
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(470, 313);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Advanced";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // cmbCacheFormat
            // 
            this.cmbCacheFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCacheFormat.FormattingEnabled = true;
            this.cmbCacheFormat.Items.AddRange(new object[] {
            "Normal",
            "Compact"});
            this.cmbCacheFormat.Location = new System.Drawing.Point(6, 19);
            this.cmbCacheFormat.Name = "cmbCacheFormat";
            this.cmbCacheFormat.Size = new System.Drawing.Size(132, 21);
            this.cmbCacheFormat.TabIndex = 0;
            // 
            // FormPreRenderTiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 392);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnPreRender);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormPreRenderTiles";
            this.Text = "Pre-Render Tiles";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxParallelRequest)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panelExtent.ResumeLayout(false);
            this.panelExtent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTop)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lstScales;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtService;
        private System.Windows.Forms.Button btnPreRender;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ComboBox cmbImageFormat;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbEpsg;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbOrigin;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cmbCacheFormat;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblEpsg;
        private System.Windows.Forms.CheckBox chkUseExtent;
        private System.Windows.Forms.Panel panelExtent;
        private System.Windows.Forms.NumericUpDown numTop;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numBottom;
        private System.Windows.Forms.NumericUpDown numLeft;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numRight;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown numMaxParallelRequest;
        private System.Windows.Forms.Label label4;
    }
}
namespace gView.Plugins.MapTools.Dialogs
{
    partial class FormChartWizard
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.tabWizard = new System.Windows.Forms.TabControl();
            this.tabChartType = new System.Windows.Forms.TabPage();
            this.panelDisplayMode = new System.Windows.Forms.Panel();
            this.cmbDisplayMode = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chartPreview = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lstChartTypes = new System.Windows.Forms.ListBox();
            this.tabData = new System.Windows.Forms.TabPage();
            this.cmbDataFields = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.seriesListView = new gView.Framework.UI.Controls.SymbolsListView();
            this.lstSeries = new System.Windows.Forms.ListBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbFeatures = new System.Windows.Forms.ComboBox();
            this.tabTitle = new System.Windows.Forms.TabPage();
            this.txtChartTitle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnBack = new System.Windows.Forms.Button();
            this.tabWizard.SuspendLayout();
            this.tabChartType.SuspendLayout();
            this.panelDisplayMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartPreview)).BeginInit();
            this.tabData.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabTitle.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(522, 339);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(12, 339);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNext.Location = new System.Drawing.Point(434, 339);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(82, 23);
            this.btnNext.TabIndex = 2;
            this.btnNext.Text = "Next >>";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // tabWizard
            // 
            this.tabWizard.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.tabWizard.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabWizard.Controls.Add(this.tabChartType);
            this.tabWizard.Controls.Add(this.tabData);
            this.tabWizard.Controls.Add(this.tabTitle);
            this.tabWizard.Location = new System.Drawing.Point(-21, 1);
            this.tabWizard.Multiline = true;
            this.tabWizard.Name = "tabWizard";
            this.tabWizard.SelectedIndex = 0;
            this.tabWizard.Size = new System.Drawing.Size(629, 332);
            this.tabWizard.TabIndex = 3;
            // 
            // tabChartType
            // 
            this.tabChartType.Controls.Add(this.panelDisplayMode);
            this.tabChartType.Controls.Add(this.chartPreview);
            this.tabChartType.Controls.Add(this.lstChartTypes);
            this.tabChartType.Location = new System.Drawing.Point(23, 4);
            this.tabChartType.Name = "tabChartType";
            this.tabChartType.Padding = new System.Windows.Forms.Padding(3);
            this.tabChartType.Size = new System.Drawing.Size(569, 324);
            this.tabChartType.TabIndex = 0;
            this.tabChartType.Text = "Chart Type";
            this.tabChartType.UseVisualStyleBackColor = true;
            // 
            // panelDisplayMode
            // 
            this.panelDisplayMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelDisplayMode.Controls.Add(this.cmbDisplayMode);
            this.panelDisplayMode.Controls.Add(this.label3);
            this.panelDisplayMode.Location = new System.Drawing.Point(154, 290);
            this.panelDisplayMode.Name = "panelDisplayMode";
            this.panelDisplayMode.Size = new System.Drawing.Size(335, 28);
            this.panelDisplayMode.TabIndex = 4;
            // 
            // cmbDisplayMode
            // 
            this.cmbDisplayMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbDisplayMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDisplayMode.FormattingEnabled = true;
            this.cmbDisplayMode.Items.AddRange(new object[] {
            "Every serie in the same chart area",
            "Every serie in extra chart area",
            "Mixed"});
            this.cmbDisplayMode.Location = new System.Drawing.Point(55, 4);
            this.cmbDisplayMode.Name = "cmbDisplayMode";
            this.cmbDisplayMode.Size = new System.Drawing.Size(268, 21);
            this.cmbDisplayMode.TabIndex = 2;
            this.cmbDisplayMode.SelectedIndexChanged += new System.EventHandler(this.cmbDisplayMode_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Display:";
            // 
            // chartPreview
            // 
            this.chartPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.chartPreview.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chartPreview.Legends.Add(legend1);
            this.chartPreview.Location = new System.Drawing.Point(154, 3);
            this.chartPreview.Name = "chartPreview";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartPreview.Series.Add(series1);
            this.chartPreview.Size = new System.Drawing.Size(349, 287);
            this.chartPreview.TabIndex = 1;
            this.chartPreview.Text = "chart1";
            // 
            // lstChartTypes
            // 
            this.lstChartTypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lstChartTypes.FormattingEnabled = true;
            this.lstChartTypes.IntegralHeight = false;
            this.lstChartTypes.Location = new System.Drawing.Point(3, 3);
            this.lstChartTypes.Name = "lstChartTypes";
            this.lstChartTypes.Size = new System.Drawing.Size(148, 315);
            this.lstChartTypes.TabIndex = 0;
            this.lstChartTypes.SelectedIndexChanged += new System.EventHandler(this.lstChartTypes_SelectedIndexChanged);
            // 
            // tabData
            // 
            this.tabData.Controls.Add(this.cmbDataFields);
            this.tabData.Controls.Add(this.groupBox1);
            this.tabData.Controls.Add(this.label2);
            this.tabData.Controls.Add(this.label1);
            this.tabData.Controls.Add(this.cmbFeatures);
            this.tabData.Location = new System.Drawing.Point(23, 4);
            this.tabData.Name = "tabData";
            this.tabData.Padding = new System.Windows.Forms.Padding(3);
            this.tabData.Size = new System.Drawing.Size(602, 324);
            this.tabData.TabIndex = 1;
            this.tabData.Text = "Data";
            this.tabData.UseVisualStyleBackColor = true;
            // 
            // cmbDataFields
            // 
            this.cmbDataFields.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDataFields.FormattingEnabled = true;
            this.cmbDataFields.Location = new System.Drawing.Point(196, 38);
            this.cmbDataFields.Name = "cmbDataFields";
            this.cmbDataFields.Size = new System.Drawing.Size(220, 21);
            this.cmbDataFields.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.seriesListView);
            this.groupBox1.Controls.Add(this.lstSeries);
            this.groupBox1.Controls.Add(this.btnApply);
            this.groupBox1.Controls.Add(this.btnRemove);
            this.groupBox1.Location = new System.Drawing.Point(7, 65);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(560, 253);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Series (Y-Axis)";
            // 
            // seriesListView
            // 
            this.seriesListView.AllowDrop = true;
            this.seriesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.seriesListView.LegendText = "";
            this.seriesListView.Location = new System.Drawing.Point(192, 19);
            this.seriesListView.Name = "seriesListView";
            this.seriesListView.Size = new System.Drawing.Size(362, 228);
            this.seriesListView.TabIndex = 11;
            this.seriesListView.ValueText = "";
            this.seriesListView.OnLabelChanged += new gView.Framework.UI.Controls.SymbolsListView.LabelChanged(this.seriesListView_OnLabelChanged);
            this.seriesListView.SelectedIndexChanged += new System.EventHandler(this.seriesListView_SelectedIndexChanged);
            // 
            // lstSeries
            // 
            this.lstSeries.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lstSeries.FormattingEnabled = true;
            this.lstSeries.IntegralHeight = false;
            this.lstSeries.Location = new System.Drawing.Point(11, 19);
            this.lstSeries.Name = "lstSeries";
            this.lstSeries.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstSeries.Size = new System.Drawing.Size(139, 228);
            this.lstSeries.TabIndex = 7;
            this.lstSeries.SelectedIndexChanged += new System.EventHandler(this.lstSeries_SelectedIndexChanged);
            // 
            // btnApply
            // 
            this.btnApply.Enabled = false;
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApply.Location = new System.Drawing.Point(156, 19);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(30, 23);
            this.btnApply.TabIndex = 9;
            this.btnApply.Text = ">";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Enabled = false;
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemove.Location = new System.Drawing.Point(156, 48);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(30, 23);
            this.btnRemove.TabIndex = 10;
            this.btnRemove.Text = "<";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(94, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Data Field (X-Axis):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(139, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Features:";
            // 
            // cmbFeatures
            // 
            this.cmbFeatures.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFeatures.FormattingEnabled = true;
            this.cmbFeatures.Location = new System.Drawing.Point(196, 11);
            this.cmbFeatures.Name = "cmbFeatures";
            this.cmbFeatures.Size = new System.Drawing.Size(220, 21);
            this.cmbFeatures.TabIndex = 0;
            // 
            // tabTitle
            // 
            this.tabTitle.Controls.Add(this.txtChartTitle);
            this.tabTitle.Controls.Add(this.label4);
            this.tabTitle.Location = new System.Drawing.Point(23, 4);
            this.tabTitle.Name = "tabTitle";
            this.tabTitle.Size = new System.Drawing.Size(602, 324);
            this.tabTitle.TabIndex = 2;
            this.tabTitle.Text = "Title";
            this.tabTitle.UseVisualStyleBackColor = true;
            // 
            // txtChartTitle
            // 
            this.txtChartTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtChartTitle.Location = new System.Drawing.Point(81, 26);
            this.txtChartTitle.Name = "txtChartTitle";
            this.txtChartTitle.Size = new System.Drawing.Size(502, 20);
            this.txtChartTitle.TabIndex = 1;
            this.txtChartTitle.Text = "Chart 1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Chart Title:";
            // 
            // btnBack
            // 
            this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBack.Enabled = false;
            this.btnBack.Location = new System.Drawing.Point(346, 339);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(82, 23);
            this.btnBack.TabIndex = 4;
            this.btnBack.Text = "<< Back";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // FormChartWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 374);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.tabWizard);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormChartWizard";
            this.Text = "Chart Wizard";
            this.Load += new System.EventHandler(this.FormChartWizard_Load);
            this.tabWizard.ResumeLayout(false);
            this.tabChartType.ResumeLayout(false);
            this.panelDisplayMode.ResumeLayout(false);
            this.panelDisplayMode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartPreview)).EndInit();
            this.tabData.ResumeLayout(false);
            this.tabData.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.tabTitle.ResumeLayout(false);
            this.tabTitle.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.TabControl tabWizard;
        private System.Windows.Forms.TabPage tabChartType;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPreview;
        private System.Windows.Forms.ListBox lstChartTypes;
        private System.Windows.Forms.TabPage tabData;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbDataFields;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbFeatures;
        private System.Windows.Forms.GroupBox groupBox1;
        private Framework.UI.Controls.SymbolsListView seriesListView;
        private System.Windows.Forms.ListBox lstSeries;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbDisplayMode;
        private System.Windows.Forms.Panel panelDisplayMode;
        private System.Windows.Forms.TabPage tabTitle;
        private System.Windows.Forms.TextBox txtChartTitle;
        private System.Windows.Forms.Label label4;
    }
}
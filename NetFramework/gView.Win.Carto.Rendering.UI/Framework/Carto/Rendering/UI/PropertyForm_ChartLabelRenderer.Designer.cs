namespace gView.Framework.Carto.Rendering.UI
{
    partial class PropertyForm_ChartLabelRenderer
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnValueOfEquatesToSize = new System.Windows.Forms.RadioButton();
            this.btnConstantSize = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.numSize = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.panelMaxValue = new System.Windows.Forms.Panel();
            this.numMaxValue = new System.Windows.Forms.NumericUpDown();
            this.btnGetMaxValue = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnOutlineSymbol = new System.Windows.Forms.Button();
            this.lstNumberFields = new System.Windows.Forms.ListBox();
            this.symbolsListView1 = new gView.Framework.UI.Controls.SymbolsListView();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbChartType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbLabelPriority = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSize)).BeginInit();
            this.panelMaxValue.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxValue)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmbLabelPriority);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.btnOutlineSymbol);
            this.panel1.Controls.Add(this.lstNumberFields);
            this.panel1.Controls.Add(this.symbolsListView1);
            this.panel1.Controls.Add(this.btnRemove);
            this.panel1.Controls.Add(this.btnApply);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.cmbChartType);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(559, 368);
            this.panel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnValueOfEquatesToSize);
            this.groupBox1.Controls.Add(this.btnConstantSize);
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Controls.Add(this.panelMaxValue);
            this.groupBox1.Location = new System.Drawing.Point(83, 241);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(461, 89);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            // 
            // btnValueOfEquatesToSize
            // 
            this.btnValueOfEquatesToSize.AutoSize = true;
            this.btnValueOfEquatesToSize.Location = new System.Drawing.Point(24, 57);
            this.btnValueOfEquatesToSize.Name = "btnValueOfEquatesToSize";
            this.btnValueOfEquatesToSize.Size = new System.Drawing.Size(73, 17);
            this.btnValueOfEquatesToSize.TabIndex = 11;
            this.btnValueOfEquatesToSize.Text = "A value of";
            this.btnValueOfEquatesToSize.UseVisualStyleBackColor = true;
            this.btnValueOfEquatesToSize.CheckedChanged += new System.EventHandler(this.btnValueOfEquatesToSize_CheckedChanged);
            // 
            // btnConstantSize
            // 
            this.btnConstantSize.AutoSize = true;
            this.btnConstantSize.Checked = true;
            this.btnConstantSize.Location = new System.Drawing.Point(24, 25);
            this.btnConstantSize.Name = "btnConstantSize";
            this.btnConstantSize.Size = new System.Drawing.Size(217, 17);
            this.btnConstantSize.TabIndex = 10;
            this.btnConstantSize.TabStop = true;
            this.btnConstantSize.Text = "Constant Size (show relative relatoinship)";
            this.btnConstantSize.UseVisualStyleBackColor = true;
            this.btnConstantSize.CheckedChanged += new System.EventHandler(this.btnConstantSize_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.numSize);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Location = new System.Drawing.Point(298, 49);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(107, 32);
            this.panel2.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(84, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(18, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "px";
            // 
            // numSize
            // 
            this.numSize.Location = new System.Drawing.Point(34, 6);
            this.numSize.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSize.Name = "numSize";
            this.numSize.Size = new System.Drawing.Size(47, 20);
            this.numSize.TabIndex = 2;
            this.numSize.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numSize.ValueChanged += new System.EventHandler(this.numSize_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Size:";
            // 
            // panelMaxValue
            // 
            this.panelMaxValue.Controls.Add(this.numMaxValue);
            this.panelMaxValue.Controls.Add(this.btnGetMaxValue);
            this.panelMaxValue.Controls.Add(this.label6);
            this.panelMaxValue.Location = new System.Drawing.Point(103, 49);
            this.panelMaxValue.Name = "panelMaxValue";
            this.panelMaxValue.Size = new System.Drawing.Size(199, 32);
            this.panelMaxValue.TabIndex = 9;
            // 
            // numMaxValue
            // 
            this.numMaxValue.DecimalPlaces = 2;
            this.numMaxValue.Location = new System.Drawing.Point(3, 6);
            this.numMaxValue.Maximum = new decimal(new int[] {
            1410065408,
            2,
            0,
            0});
            this.numMaxValue.Name = "numMaxValue";
            this.numMaxValue.Size = new System.Drawing.Size(97, 20);
            this.numMaxValue.TabIndex = 3;
            this.numMaxValue.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numMaxValue.ValueChanged += new System.EventHandler(this.numMaxValue_ValueChanged);
            // 
            // btnGetMaxValue
            // 
            this.btnGetMaxValue.Location = new System.Drawing.Point(106, 6);
            this.btnGetMaxValue.Name = "btnGetMaxValue";
            this.btnGetMaxValue.Size = new System.Drawing.Size(31, 20);
            this.btnGetMaxValue.TabIndex = 10;
            this.btnGetMaxValue.Text = "<<";
            this.btnGetMaxValue.UseVisualStyleBackColor = true;
            this.btnGetMaxValue.Click += new System.EventHandler(this.btnGetMaxValue_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(139, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "equates to";
            // 
            // btnOutlineSymbol
            // 
            this.btnOutlineSymbol.Location = new System.Drawing.Point(262, 200);
            this.btnOutlineSymbol.Name = "btnOutlineSymbol";
            this.btnOutlineSymbol.Size = new System.Drawing.Size(282, 35);
            this.btnOutlineSymbol.TabIndex = 7;
            this.btnOutlineSymbol.Text = "Outline Symbol";
            this.btnOutlineSymbol.UseVisualStyleBackColor = true;
            this.btnOutlineSymbol.Click += new System.EventHandler(this.btnOutlineSymbol_Click);
            // 
            // lstNumberFields
            // 
            this.lstNumberFields.FormattingEnabled = true;
            this.lstNumberFields.Location = new System.Drawing.Point(81, 58);
            this.lstNumberFields.Name = "lstNumberFields";
            this.lstNumberFields.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstNumberFields.Size = new System.Drawing.Size(139, 173);
            this.lstNumberFields.TabIndex = 2;
            this.lstNumberFields.SelectedIndexChanged += new System.EventHandler(this.lstNumberFields_SelectedIndexChanged);
            // 
            // symbolsListView1
            // 
            this.symbolsListView1.AllowDrop = true;
            this.symbolsListView1.LegendText = "";
            this.symbolsListView1.Location = new System.Drawing.Point(262, 58);
            this.symbolsListView1.Name = "symbolsListView1";
            this.symbolsListView1.Size = new System.Drawing.Size(282, 134);
            this.symbolsListView1.TabIndex = 6;
            this.symbolsListView1.ValueText = "";
            this.symbolsListView1.OnSymbolChanged += new gView.Framework.UI.Controls.SymbolsListView.SymbolChanged(this.symbolsListView1_OnSymbolChanged);
            this.symbolsListView1.OnLabelChanged += new gView.Framework.UI.Controls.SymbolsListView.LabelChanged(this.symbolsListView1_OnLabelChanged);
            this.symbolsListView1.SelectedIndexChanged += new System.EventHandler(this.symbolsListView1_SelectedIndexChanged);
            // 
            // btnRemove
            // 
            this.btnRemove.Enabled = false;
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemove.Location = new System.Drawing.Point(226, 87);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(30, 23);
            this.btnRemove.TabIndex = 5;
            this.btnRemove.Text = "<";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnApply
            // 
            this.btnApply.Enabled = false;
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApply.Location = new System.Drawing.Point(226, 58);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(30, 23);
            this.btnApply.TabIndex = 4;
            this.btnApply.Text = ">";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(80, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Number Fields:";
            // 
            // cmbChartType
            // 
            this.cmbChartType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbChartType.FormattingEnabled = true;
            this.cmbChartType.Items.AddRange(new object[] {
            "Pie",
            "Bars",
            "Stack"});
            this.cmbChartType.Location = new System.Drawing.Point(80, 10);
            this.cmbChartType.Name = "cmbChartType";
            this.cmbChartType.Size = new System.Drawing.Size(139, 21);
            this.cmbChartType.TabIndex = 1;
            this.cmbChartType.SelectedIndexChanged += new System.EventHandler(this.cmbChartType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Chart Type:";
            // 
            // cmbLabelPriority
            // 
            this.cmbLabelPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLabelPriority.FormattingEnabled = true;
            this.cmbLabelPriority.Items.AddRange(new object[] {
            "Always (no overlay check)",
            "High (overlay check)",
            "Normal (overlay check)",
            "Low (overlay check)"});
            this.cmbLabelPriority.Location = new System.Drawing.Point(303, 10);
            this.cmbLabelPriority.Name = "cmbLabelPriority";
            this.cmbLabelPriority.Size = new System.Drawing.Size(241, 21);
            this.cmbLabelPriority.TabIndex = 12;
            this.cmbLabelPriority.SelectedIndexChanged += new System.EventHandler(this.cmbLabelPriority_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(229, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Label Priority:";
            // 
            // PropertyForm_ChartLabelRenderer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 368);
            this.Controls.Add(this.panel1);
            this.Name = "PropertyForm_ChartLabelRenderer";
            this.Text = "PropertyForm_ChartLabelRenderer";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSize)).EndInit();
            this.panelMaxValue.ResumeLayout(false);
            this.panelMaxValue.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxValue)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cmbChartType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lstNumberFields;
        private Framework.UI.Controls.SymbolsListView symbolsListView1;
        private System.Windows.Forms.Button btnOutlineSymbol;
        private System.Windows.Forms.Panel panelMaxValue;
        private System.Windows.Forms.NumericUpDown numMaxValue;
        private System.Windows.Forms.Button btnGetMaxValue;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton btnValueOfEquatesToSize;
        private System.Windows.Forms.RadioButton btnConstantSize;
        private System.Windows.Forms.ComboBox cmbLabelPriority;
        private System.Windows.Forms.Label label5;
    }
}
namespace gView.Plugins.DbTools.Relates
{
    partial class AddTableRelationDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddTableRelationDialog));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbLeftTableField = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbLeftTable = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cmbRightTableField = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbRightTable = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbLogicalOperator = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(12, 309);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Enabled = false;
            this.btnOk.Location = new System.Drawing.Point(576, 309);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 12;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.txtName);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(12, 96);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(639, 56);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "New Join";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(65, 24);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(552, 20);
            this.txtName.TabIndex = 20;
            this.txtName.Text = "Relation1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Name:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbLeftTable);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 159);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(250, 75);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Left Relation Table";
            // 
            // cmbLeftTableField
            // 
            this.cmbLeftTableField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLeftTableField.FormattingEnabled = true;
            this.cmbLeftTableField.Location = new System.Drawing.Point(29, 263);
            this.cmbLeftTableField.Name = "cmbLeftTableField";
            this.cmbLeftTableField.Size = new System.Drawing.Size(217, 21);
            this.cmbLeftTableField.TabIndex = 3;
            this.cmbLeftTableField.SelectedIndexChanged += new System.EventHandler(this.cmbLeftTableField_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 246);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Relation Field:";
            // 
            // cmbLeftTable
            // 
            this.cmbLeftTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLeftTable.FormattingEnabled = true;
            this.cmbLeftTable.Location = new System.Drawing.Point(16, 47);
            this.cmbLeftTable.Name = "cmbLeftTable";
            this.cmbLeftTable.Size = new System.Drawing.Size(217, 21);
            this.cmbLeftTable.TabIndex = 1;
            this.cmbLeftTable.SelectedIndexChanged += new System.EventHandler(this.cmbLeftTable_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Table:";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.cmbRightTable);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(382, 159);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(269, 75);
            this.groupBox3.TabIndex = 21;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Right Relation Table";
            // 
            // cmbRightTableField
            // 
            this.cmbRightTableField.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbRightTableField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRightTableField.FormattingEnabled = true;
            this.cmbRightTableField.Location = new System.Drawing.Point(398, 263);
            this.cmbRightTableField.Name = "cmbRightTableField";
            this.cmbRightTableField.Size = new System.Drawing.Size(231, 21);
            this.cmbRightTableField.TabIndex = 7;
            this.cmbRightTableField.SelectedIndexChanged += new System.EventHandler(this.cmbRightTableField_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(395, 246);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Relation Field:";
            // 
            // cmbRightTable
            // 
            this.cmbRightTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbRightTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRightTable.FormattingEnabled = true;
            this.cmbRightTable.Location = new System.Drawing.Point(16, 47);
            this.cmbRightTable.Name = "cmbRightTable";
            this.cmbRightTable.Size = new System.Drawing.Size(220, 21);
            this.cmbRightTable.TabIndex = 5;
            this.cmbRightTable.SelectedIndexChanged += new System.EventHandler(this.cmbRightTable_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Table:";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(13, 13);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(522, 70);
            this.label6.TabIndex = 22;
            this.label6.Text = resources.GetString("label6.Text");
            // 
            // cmbLogicalOperator
            // 
            this.cmbLogicalOperator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLogicalOperator.FormattingEnabled = true;
            this.cmbLogicalOperator.Items.AddRange(new object[] {
            "=",
            "<",
            ">",
            "<=",
            ">=",
            "<>"});
            this.cmbLogicalOperator.Location = new System.Drawing.Point(252, 263);
            this.cmbLogicalOperator.Name = "cmbLogicalOperator";
            this.cmbLogicalOperator.Size = new System.Drawing.Size(140, 21);
            this.cmbLogicalOperator.TabIndex = 23;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(249, 246);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "Logical Operator:";
            // 
            // AddTableRelationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(663, 344);
            this.Controls.Add(this.cmbLogicalOperator);
            this.Controls.Add(this.cmbLeftTableField);
            this.Controls.Add(this.cmbRightTableField);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "AddTableRelationDialog";
            this.Text = "Add Table Relation Dialog";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbLeftTableField;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbLeftTable;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cmbRightTableField;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbRightTable;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbLogicalOperator;
        private System.Windows.Forms.Label label7;
    }
}
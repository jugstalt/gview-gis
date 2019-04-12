namespace gView.Framework.Data.Joins.UI
{
    partial class FeatureLayerDatabaseJoinControl
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
            this.gbJoinedDatabaseTable = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lstJoinTableFields = new System.Windows.Forms.ListBox();
            this.cmbJoinTableField = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbTable = new System.Windows.Forms.ComboBox();
            this.getConnectionString = new System.Windows.Forms.Button();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.gbJoinedDatabaseTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbJoinedDatabaseTable
            // 
            this.gbJoinedDatabaseTable.Controls.Add(this.label5);
            this.gbJoinedDatabaseTable.Controls.Add(this.lstJoinTableFields);
            this.gbJoinedDatabaseTable.Controls.Add(this.cmbJoinTableField);
            this.gbJoinedDatabaseTable.Controls.Add(this.label6);
            this.gbJoinedDatabaseTable.Controls.Add(this.cmbTable);
            this.gbJoinedDatabaseTable.Controls.Add(this.getConnectionString);
            this.gbJoinedDatabaseTable.Controls.Add(this.txtConnectionString);
            this.gbJoinedDatabaseTable.Controls.Add(this.label1);
            this.gbJoinedDatabaseTable.Controls.Add(this.label2);
            this.gbJoinedDatabaseTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbJoinedDatabaseTable.Location = new System.Drawing.Point(0, 0);
            this.gbJoinedDatabaseTable.Name = "gbJoinedDatabaseTable";
            this.gbJoinedDatabaseTable.Size = new System.Drawing.Size(426, 224);
            this.gbJoinedDatabaseTable.TabIndex = 13;
            this.gbJoinedDatabaseTable.TabStop = false;
            this.gbJoinedDatabaseTable.Text = "Joined Database Table";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(66, 113);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Join Table Fields:";
            // 
            // lstJoinTableFields
            // 
            this.lstJoinTableFields.FormattingEnabled = true;
            this.lstJoinTableFields.Location = new System.Drawing.Point(159, 113);
            this.lstJoinTableFields.Name = "lstJoinTableFields";
            this.lstJoinTableFields.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lstJoinTableFields.Size = new System.Drawing.Size(228, 82);
            this.lstJoinTableFields.TabIndex = 18;
            // 
            // cmbJoinTableField
            // 
            this.cmbJoinTableField.FormattingEnabled = true;
            this.cmbJoinTableField.Location = new System.Drawing.Point(159, 86);
            this.cmbJoinTableField.Name = "cmbJoinTableField";
            this.cmbJoinTableField.Size = new System.Drawing.Size(229, 21);
            this.cmbJoinTableField.TabIndex = 17;
            this.cmbJoinTableField.DropDown += new System.EventHandler(this.cmbJoinTableField_DropDown);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(48, 90);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(106, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Join Table Join-Field:";
            // 
            // cmbTable
            // 
            this.cmbTable.FormattingEnabled = true;
            this.cmbTable.Location = new System.Drawing.Point(159, 59);
            this.cmbTable.Name = "cmbTable";
            this.cmbTable.Size = new System.Drawing.Size(229, 21);
            this.cmbTable.TabIndex = 9;
            this.cmbTable.DropDown += new System.EventHandler(this.cmbTable_DropDown);
            this.cmbTable.SelectedIndexChanged += new System.EventHandler(this.cmbTable_SelectedIndexChanged);
            // 
            // getConnectionString
            // 
            this.getConnectionString.Location = new System.Drawing.Point(347, 21);
            this.getConnectionString.Name = "getConnectionString";
            this.getConnectionString.Size = new System.Drawing.Size(41, 23);
            this.getConnectionString.TabIndex = 5;
            this.getConnectionString.Text = "...";
            this.getConnectionString.UseVisualStyleBackColor = true;
            this.getConnectionString.Click += new System.EventHandler(this.getConnectionString_Click);
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Location = new System.Drawing.Point(159, 23);
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.ReadOnly = true;
            this.txtConnectionString.Size = new System.Drawing.Size(182, 20);
            this.txtConnectionString.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Database Connection String:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(67, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Database Table:";
            // 
            // FeatureLayerDatabaseJoinControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbJoinedDatabaseTable);
            this.Name = "FeatureLayerDatabaseJoinControl";
            this.Size = new System.Drawing.Size(426, 224);
            this.gbJoinedDatabaseTable.ResumeLayout(false);
            this.gbJoinedDatabaseTable.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbJoinedDatabaseTable;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox lstJoinTableFields;
        private System.Windows.Forms.ComboBox cmbJoinTableField;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbTable;
        private System.Windows.Forms.Button getConnectionString;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

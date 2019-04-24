namespace gView.Plugins.DbTools.Joins
{
    partial class AddJoinDialog
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
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.cmbFeatureLayerField = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnInnerJoin = new System.Windows.Forms.RadioButton();
            this.btnOuterJoin = new System.Windows.Forms.RadioButton();
            this.panelJoinTypeUI = new System.Windows.Forms.Panel();
            this.cmbJoinClasses = new System.Windows.Forms.ComboBox();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(340, 514);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 10;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(12, 514);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtName);
            this.groupBox2.Controls.Add(this.cmbFeatureLayerField);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(14, 11);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(400, 82);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "New Join";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(65, 24);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(320, 20);
            this.txtName.TabIndex = 20;
            this.txtName.Text = "Join";
            // 
            // cmbFeatureLayerField
            // 
            this.cmbFeatureLayerField.FormattingEnabled = true;
            this.cmbFeatureLayerField.Location = new System.Drawing.Point(157, 52);
            this.cmbFeatureLayerField.Name = "cmbFeatureLayerField";
            this.cmbFeatureLayerField.Size = new System.Drawing.Size(229, 21);
            this.cmbFeatureLayerField.TabIndex = 19;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(51, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Feature Layer Field:";
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
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.btnInnerJoin);
            this.groupBox3.Controls.Add(this.btnOuterJoin);
            this.groupBox3.Location = new System.Drawing.Point(12, 356);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(402, 149);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Join Type";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(31, 104);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(346, 30);
            this.label8.TabIndex = 3;
            this.label8.Text = "Keep only matching records in the target table.";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(31, 47);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(346, 30);
            this.label7.TabIndex = 2;
            this.label7.Text = "Keep all records in the target table even if there is no record in the joined tab" +
                "le.";
            // 
            // btnInnerJoin
            // 
            this.btnInnerJoin.AutoSize = true;
            this.btnInnerJoin.Location = new System.Drawing.Point(13, 85);
            this.btnInnerJoin.Name = "btnInnerJoin";
            this.btnInnerJoin.Size = new System.Drawing.Size(93, 17);
            this.btnInnerJoin.TabIndex = 1;
            this.btnInnerJoin.Text = "Use Inner Join";
            this.btnInnerJoin.UseVisualStyleBackColor = true;
            // 
            // btnOuterJoin
            // 
            this.btnOuterJoin.AutoSize = true;
            this.btnOuterJoin.Checked = true;
            this.btnOuterJoin.Location = new System.Drawing.Point(13, 28);
            this.btnOuterJoin.Name = "btnOuterJoin";
            this.btnOuterJoin.Size = new System.Drawing.Size(95, 17);
            this.btnOuterJoin.TabIndex = 0;
            this.btnOuterJoin.TabStop = true;
            this.btnOuterJoin.Text = "Use Outer Join";
            this.btnOuterJoin.UseVisualStyleBackColor = true;
            // 
            // panelJoinTypeUI
            // 
            this.panelJoinTypeUI.Location = new System.Drawing.Point(12, 128);
            this.panelJoinTypeUI.Name = "panelJoinTypeUI";
            this.panelJoinTypeUI.Size = new System.Drawing.Size(403, 213);
            this.panelJoinTypeUI.TabIndex = 15;
            // 
            // cmbJoinClasses
            // 
            this.cmbJoinClasses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbJoinClasses.FormattingEnabled = true;
            this.cmbJoinClasses.Location = new System.Drawing.Point(14, 100);
            this.cmbJoinClasses.Name = "cmbJoinClasses";
            this.cmbJoinClasses.Size = new System.Drawing.Size(400, 21);
            this.cmbJoinClasses.TabIndex = 16;
            this.cmbJoinClasses.SelectedIndexChanged += new System.EventHandler(this.cmbJoinMode_SelectedIndexChanged);
            // 
            // AddJoinDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 549);
            this.Controls.Add(this.cmbJoinClasses);
            this.Controls.Add(this.panelJoinTypeUI);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "AddJoinDialog";
            this.Text = "Add Join";
            this.Load += new System.EventHandler(this.AddJoinDialog_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.ComboBox cmbFeatureLayerField;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RadioButton btnInnerJoin;
        private System.Windows.Forms.RadioButton btnOuterJoin;
        private System.Windows.Forms.Panel panelJoinTypeUI;
        private System.Windows.Forms.ComboBox cmbJoinClasses;
    }
}
namespace gView.DataSources.EventTable.UI
{
    partial class FormEventTableConnection
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
            this.getConnectionString = new System.Windows.Forms.Button();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbTable = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbY = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSR = new System.Windows.Forms.TextBox();
            this.btnGetSR = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbX = new System.Windows.Forms.ComboBox();
            this.cmbID = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // getConnectionString
            // 
            this.getConnectionString.Location = new System.Drawing.Point(350, 8);
            this.getConnectionString.Name = "getConnectionString";
            this.getConnectionString.Size = new System.Drawing.Size(41, 23);
            this.getConnectionString.TabIndex = 0;
            this.getConnectionString.Text = "...";
            this.getConnectionString.UseVisualStyleBackColor = true;
            this.getConnectionString.Click += new System.EventHandler(this.getConnectionString_Click);
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Location = new System.Drawing.Point(162, 10);
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.ReadOnly = true;
            this.txtConnectionString.Size = new System.Drawing.Size(182, 20);
            this.txtConnectionString.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Database Connection String:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(70, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Database Table:";
            // 
            // cmbTable
            // 
            this.cmbTable.FormattingEnabled = true;
            this.cmbTable.Location = new System.Drawing.Point(162, 46);
            this.cmbTable.Name = "cmbTable";
            this.cmbTable.Size = new System.Drawing.Size(229, 21);
            this.cmbTable.TabIndex = 4;
            this.cmbTable.DropDown += new System.EventHandler(this.cmbTable_DropDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(84, 143);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Y Value Field:";
            // 
            // cmbY
            // 
            this.cmbY.FormattingEnabled = true;
            this.cmbY.Location = new System.Drawing.Point(162, 138);
            this.cmbY.Name = "cmbY";
            this.cmbY.Size = new System.Drawing.Size(229, 21);
            this.cmbY.TabIndex = 8;
            this.cmbY.DropDown += new System.EventHandler(this.cmbY_DropDown);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(303, 219);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(97, 36);
            this.btnOK.TabIndex = 9;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(12, 219);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(97, 36);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(61, 183);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Spatial Reference:";
            // 
            // txtSR
            // 
            this.txtSR.Location = new System.Drawing.Point(162, 180);
            this.txtSR.Name = "txtSR";
            this.txtSR.ReadOnly = true;
            this.txtSR.Size = new System.Drawing.Size(182, 20);
            this.txtSR.TabIndex = 12;
            // 
            // btnGetSR
            // 
            this.btnGetSR.Location = new System.Drawing.Point(350, 178);
            this.btnGetSR.Name = "btnGetSR";
            this.btnGetSR.Size = new System.Drawing.Size(41, 23);
            this.btnGetSR.TabIndex = 11;
            this.btnGetSR.Text = "...";
            this.btnGetSR.UseVisualStyleBackColor = true;
            this.btnGetSR.Click += new System.EventHandler(this.btnGetSR_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(84, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "X Value Field:";
            // 
            // cmbX
            // 
            this.cmbX.FormattingEnabled = true;
            this.cmbX.Location = new System.Drawing.Point(162, 113);
            this.cmbX.Name = "cmbX";
            this.cmbX.Size = new System.Drawing.Size(229, 21);
            this.cmbX.TabIndex = 7;
            this.cmbX.DropDown += new System.EventHandler(this.cmbX_DropDown);
            // 
            // cmbID
            // 
            this.cmbID.FormattingEnabled = true;
            this.cmbID.Location = new System.Drawing.Point(162, 87);
            this.cmbID.Name = "cmbID";
            this.cmbID.Size = new System.Drawing.Size(229, 21);
            this.cmbID.TabIndex = 15;
            this.cmbID.DropDown += new System.EventHandler(this.cmbID_DropDown);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(84, 92);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(46, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "ID Field:";
            // 
            // FormEventTableConnection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 265);
            this.Controls.Add(this.cmbID);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtSR);
            this.Controls.Add(this.btnGetSR);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cmbY);
            this.Controls.Add(this.cmbX);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbTable);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtConnectionString);
            this.Controls.Add(this.getConnectionString);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormEventTableConnection";
            this.Text = "Event Table Connection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button getConnectionString;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbTable;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbY;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSR;
        private System.Windows.Forms.Button btnGetSR;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbX;
        private System.Windows.Forms.ComboBox cmbID;
        private System.Windows.Forms.Label label6;
    }
}
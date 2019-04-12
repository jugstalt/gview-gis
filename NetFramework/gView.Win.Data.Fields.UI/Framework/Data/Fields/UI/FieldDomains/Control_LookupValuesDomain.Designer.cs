namespace gView.Framework.Data.Fields.UI.FieldDomains
{
    partial class Control_LookupValuesDomain
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtConnection = new System.Windows.Forms.TextBox();
            this.btnGetConnectionString = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSQL = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Database Connection:";
            // 
            // txtConnection
            // 
            this.txtConnection.Location = new System.Drawing.Point(135, 10);
            this.txtConnection.Name = "txtConnection";
            this.txtConnection.ReadOnly = true;
            this.txtConnection.Size = new System.Drawing.Size(224, 20);
            this.txtConnection.TabIndex = 1;
            // 
            // btnGetConnectionString
            // 
            this.btnGetConnectionString.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnGetConnectionString.Location = new System.Drawing.Point(365, 8);
            this.btnGetConnectionString.Name = "btnGetConnectionString";
            this.btnGetConnectionString.Size = new System.Drawing.Size(40, 23);
            this.btnGetConnectionString.TabIndex = 2;
            this.btnGetConnectionString.Text = "...";
            this.btnGetConnectionString.UseVisualStyleBackColor = true;
            this.btnGetConnectionString.Click += new System.EventHandler(this.btnGetConnectionString_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "SQL Statement:";
            // 
            // txtSQL
            // 
            this.txtSQL.Location = new System.Drawing.Point(19, 63);
            this.txtSQL.Multiline = true;
            this.txtSQL.Name = "txtSQL";
            this.txtSQL.Size = new System.Drawing.Size(386, 89);
            this.txtSQL.TabIndex = 4;
            this.txtSQL.TextChanged += new System.EventHandler(this.txtSQL_TextChanged);
            // 
            // Control_LookupValuesDomain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtSQL);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnGetConnectionString);
            this.Controls.Add(this.txtConnection);
            this.Controls.Add(this.label1);
            this.Name = "Control_LookupValuesDomain";
            this.Size = new System.Drawing.Size(437, 213);
            this.Load += new System.EventHandler(this.Control_LookupValuesDomain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtConnection;
        private System.Windows.Forms.Button btnGetConnectionString;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSQL;
    }
}

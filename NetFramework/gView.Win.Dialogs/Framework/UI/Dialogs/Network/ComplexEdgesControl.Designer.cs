namespace gView.Framework.UI.Dialogs.Network
{
    partial class ComplexEdgesControl
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
            this.chkCreateComplexEdges = new System.Windows.Forms.CheckBox();
            this.lstEdges = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkCreateComplexEdges
            // 
            this.chkCreateComplexEdges.AutoSize = true;
            this.chkCreateComplexEdges.Location = new System.Drawing.Point(17, 81);
            this.chkCreateComplexEdges.Name = "chkCreateComplexEdges";
            this.chkCreateComplexEdges.Size = new System.Drawing.Size(133, 17);
            this.chkCreateComplexEdges.TabIndex = 0;
            this.chkCreateComplexEdges.Text = "Create Complex Edges";
            this.chkCreateComplexEdges.UseVisualStyleBackColor = true;
            this.chkCreateComplexEdges.CheckedChanged += new System.EventHandler(this.chkCreateComplexEdges_CheckedChanged);
            // 
            // lstEdges
            // 
            this.lstEdges.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstEdges.CheckBoxes = true;
            this.lstEdges.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lstEdges.FullRowSelect = true;
            this.lstEdges.GridLines = true;
            this.lstEdges.Location = new System.Drawing.Point(6, 19);
            this.lstEdges.MultiSelect = false;
            this.lstEdges.Name = "lstEdges";
            this.lstEdges.Size = new System.Drawing.Size(521, 134);
            this.lstEdges.TabIndex = 0;
            this.lstEdges.UseCompatibleStateImageBehavior = false;
            this.lstEdges.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Line Featureclasses";
            this.columnHeader1.Width = 477;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lstEdges);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(17, 113);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(533, 159);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Create Complex Edges for the following Featureclasses";
            // 
            // ComplexEdgesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.chkCreateComplexEdges);
            this.Name = "ComplexEdgesControl";
            this.Size = new System.Drawing.Size(564, 333);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkCreateComplexEdges;
        private System.Windows.Forms.ListView lstEdges;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

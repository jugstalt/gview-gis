namespace gView.Framework.Data.Fields.UI.FieldDomains
{
    partial class Control_RangeDomain
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
            this.label2 = new System.Windows.Forms.Label();
            this.numMinValue = new System.Windows.Forms.NumericUpDown();
            this.numMaxValue = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numMinValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxValue)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Minimum Value:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Maximum Value:";
            // 
            // numMinValue
            // 
            this.numMinValue.Location = new System.Drawing.Point(115, 29);
            this.numMinValue.Name = "numMinValue";
            this.numMinValue.Size = new System.Drawing.Size(152, 20);
            this.numMinValue.TabIndex = 2;
            this.numMinValue.ValueChanged += new System.EventHandler(this.numMinValue_ValueChanged);
            // 
            // numMaxValue
            // 
            this.numMaxValue.Location = new System.Drawing.Point(115, 62);
            this.numMaxValue.Name = "numMaxValue";
            this.numMaxValue.Size = new System.Drawing.Size(152, 20);
            this.numMaxValue.TabIndex = 3;
            this.numMaxValue.ValueChanged += new System.EventHandler(this.numMaxValue_ValueChanged);
            // 
            // Control_RangeDomain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.numMaxValue);
            this.Controls.Add(this.numMinValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Control_RangeDomain";
            this.Size = new System.Drawing.Size(345, 150);
            this.Load += new System.EventHandler(this.Control_RangeDomain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numMinValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxValue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numMinValue;
        private System.Windows.Forms.NumericUpDown numMaxValue;
    }
}

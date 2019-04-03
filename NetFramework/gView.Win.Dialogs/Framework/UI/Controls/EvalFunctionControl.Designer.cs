namespace gView.Framework.UI.Controls
{
    partial class EvalFunctionControl
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
            this.txtFunction = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCheckSyntax = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtFunction
            // 
            this.txtFunction.Location = new System.Drawing.Point(49, 19);
            this.txtFunction.Name = "txtFunction";
            this.txtFunction.Size = new System.Drawing.Size(246, 20);
            this.txtFunction.TabIndex = 0;
            this.txtFunction.TextChanged += new System.EventHandler(this.txtFunction_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "f(x) =";
            // 
            // btnCheckSyntax
            // 
            this.btnCheckSyntax.Location = new System.Drawing.Point(175, 55);
            this.btnCheckSyntax.Name = "btnCheckSyntax";
            this.btnCheckSyntax.Size = new System.Drawing.Size(120, 23);
            this.btnCheckSyntax.TabIndex = 2;
            this.btnCheckSyntax.Text = "Check Syntax...";
            this.btnCheckSyntax.UseVisualStyleBackColor = true;
            this.btnCheckSyntax.Click += new System.EventHandler(this.btnCheckSyntax_Click);
            // 
            // EvalFunctionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCheckSyntax);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtFunction);
            this.Name = "EvalFunctionControl";
            this.Size = new System.Drawing.Size(311, 100);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFunction;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCheckSyntax;
    }
}

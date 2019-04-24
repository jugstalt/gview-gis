namespace gView.Framework.Carto.Rendering.UI
{
    partial class NetworkRendererControl
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
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnChooseEdgeSymbol = new System.Windows.Forms.Button();
            this.btnChooseSimpeSwitchSymbol = new System.Windows.Forms.Button();
            this.btnChooseSwitchOnSymbol = new System.Windows.Forms.Button();
            this.btnChooseSwitchOffSymbol = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(58, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Edges:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Simple Nodes:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 123);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Switches (On):";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 167);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Switches (Off):";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnChooseEdgeSymbol
            // 
            this.btnChooseEdgeSymbol.Location = new System.Drawing.Point(104, 24);
            this.btnChooseEdgeSymbol.Name = "btnChooseEdgeSymbol";
            this.btnChooseEdgeSymbol.Size = new System.Drawing.Size(172, 38);
            this.btnChooseEdgeSymbol.TabIndex = 5;
            this.btnChooseEdgeSymbol.UseVisualStyleBackColor = true;
            this.btnChooseEdgeSymbol.Paint += new System.Windows.Forms.PaintEventHandler(this.btnChooseEdgeSymbol_Paint);
            this.btnChooseEdgeSymbol.Click += new System.EventHandler(this.btnChooseEdgeSymbol_Click);
            // 
            // btnChooseSimpeSwitchSymbol
            // 
            this.btnChooseSimpeSwitchSymbol.Location = new System.Drawing.Point(104, 66);
            this.btnChooseSimpeSwitchSymbol.Name = "btnChooseSimpeSwitchSymbol";
            this.btnChooseSimpeSwitchSymbol.Size = new System.Drawing.Size(172, 38);
            this.btnChooseSimpeSwitchSymbol.TabIndex = 6;
            this.btnChooseSimpeSwitchSymbol.UseVisualStyleBackColor = true;
            this.btnChooseSimpeSwitchSymbol.Paint += new System.Windows.Forms.PaintEventHandler(this.btnChooseSimpeSwitchSymbol_Paint);
            this.btnChooseSimpeSwitchSymbol.Click += new System.EventHandler(this.btnChooseSimpeSwitchSymbol_Click);
            // 
            // btnChooseSwitchOnSymbol
            // 
            this.btnChooseSwitchOnSymbol.Location = new System.Drawing.Point(104, 110);
            this.btnChooseSwitchOnSymbol.Name = "btnChooseSwitchOnSymbol";
            this.btnChooseSwitchOnSymbol.Size = new System.Drawing.Size(172, 38);
            this.btnChooseSwitchOnSymbol.TabIndex = 7;
            this.btnChooseSwitchOnSymbol.UseVisualStyleBackColor = true;
            this.btnChooseSwitchOnSymbol.Paint += new System.Windows.Forms.PaintEventHandler(this.btnChooseSwitchOnSymbol_Paint);
            this.btnChooseSwitchOnSymbol.Click += new System.EventHandler(this.btnChooseSwitchOnSymbol_Click);
            // 
            // btnChooseSwitchOffSymbol
            // 
            this.btnChooseSwitchOffSymbol.Location = new System.Drawing.Point(104, 154);
            this.btnChooseSwitchOffSymbol.Name = "btnChooseSwitchOffSymbol";
            this.btnChooseSwitchOffSymbol.Size = new System.Drawing.Size(172, 38);
            this.btnChooseSwitchOffSymbol.TabIndex = 8;
            this.btnChooseSwitchOffSymbol.UseVisualStyleBackColor = true;
            this.btnChooseSwitchOffSymbol.Paint += new System.Windows.Forms.PaintEventHandler(this.btnChooseSwitchOffSymbol_Paint);
            this.btnChooseSwitchOffSymbol.Click += new System.EventHandler(this.btnChooseSwitchOffSymbol_Click);
            // 
            // NetworkRendererControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnChooseSwitchOffSymbol);
            this.Controls.Add(this.btnChooseSwitchOnSymbol);
            this.Controls.Add(this.btnChooseSimpeSwitchSymbol);
            this.Controls.Add(this.btnChooseEdgeSymbol);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "NetworkRendererControl";
            this.Size = new System.Drawing.Size(298, 329);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnChooseEdgeSymbol;
        private System.Windows.Forms.Button btnChooseSimpeSwitchSymbol;
        private System.Windows.Forms.Button btnChooseSwitchOnSymbol;
        private System.Windows.Forms.Button btnChooseSwitchOffSymbol;
    }
}

namespace gView.Framework.Carto.Rendering.UI
{
    partial class PropertyForm_DimensionRenderer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyForm_DimensionRenderer));
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtFormat = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbLineCap = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnTextSymbol = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnLineSymbol = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AccessibleDescription = null;
            this.panel1.AccessibleName = null;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackgroundImage = null;
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Font = null;
            this.panel1.Name = "panel1";
            // 
            // groupBox2
            // 
            this.groupBox2.AccessibleDescription = null;
            this.groupBox2.AccessibleName = null;
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.BackgroundImage = null;
            this.groupBox2.Controls.Add(this.txtFormat);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.cmbLineCap);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Font = null;
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // txtFormat
            // 
            this.txtFormat.AccessibleDescription = null;
            this.txtFormat.AccessibleName = null;
            resources.ApplyResources(this.txtFormat, "txtFormat");
            this.txtFormat.BackgroundImage = null;
            this.txtFormat.Font = null;
            this.txtFormat.Name = "txtFormat";
            this.txtFormat.TextChanged += new System.EventHandler(this.txtFormat_TextChanged);
            // 
            // label4
            // 
            this.label4.AccessibleDescription = null;
            this.label4.AccessibleName = null;
            resources.ApplyResources(this.label4, "label4");
            this.label4.Font = null;
            this.label4.Name = "label4";
            // 
            // cmbLineCap
            // 
            this.cmbLineCap.AccessibleDescription = null;
            this.cmbLineCap.AccessibleName = null;
            resources.ApplyResources(this.cmbLineCap, "cmbLineCap");
            this.cmbLineCap.BackgroundImage = null;
            this.cmbLineCap.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbLineCap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLineCap.Font = null;
            this.cmbLineCap.FormattingEnabled = true;
            this.cmbLineCap.Name = "cmbLineCap";
            this.cmbLineCap.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cmbLineCap_DrawItem);
            this.cmbLineCap.SelectedIndexChanged += new System.EventHandler(this.cmbLineCap_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Font = null;
            this.label3.Name = "label3";
            // 
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = null;
            this.groupBox1.AccessibleName = null;
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.BackgroundImage = null;
            this.groupBox1.Controls.Add(this.btnTextSymbol);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnLineSymbol);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = null;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // btnTextSymbol
            // 
            this.btnTextSymbol.AccessibleDescription = null;
            this.btnTextSymbol.AccessibleName = null;
            resources.ApplyResources(this.btnTextSymbol, "btnTextSymbol");
            this.btnTextSymbol.BackgroundImage = null;
            this.btnTextSymbol.Font = null;
            this.btnTextSymbol.Name = "btnTextSymbol";
            this.btnTextSymbol.UseVisualStyleBackColor = true;
            this.btnTextSymbol.Paint += new System.Windows.Forms.PaintEventHandler(this.btnTextSymbol_Paint);
            this.btnTextSymbol.Click += new System.EventHandler(this.btnTextSymbol_Click);
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // btnLineSymbol
            // 
            this.btnLineSymbol.AccessibleDescription = null;
            this.btnLineSymbol.AccessibleName = null;
            resources.ApplyResources(this.btnLineSymbol, "btnLineSymbol");
            this.btnLineSymbol.BackgroundImage = null;
            this.btnLineSymbol.Font = null;
            this.btnLineSymbol.Name = "btnLineSymbol";
            this.btnLineSymbol.UseVisualStyleBackColor = true;
            this.btnLineSymbol.Paint += new System.Windows.Forms.PaintEventHandler(this.btnLineSymbol_Paint);
            this.btnLineSymbol.Click += new System.EventHandler(this.btnLineSymbol_Click);
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // PropertyForm_DimensionRenderer
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.panel1);
            this.Font = null;
            this.Icon = null;
            this.Name = "PropertyForm_DimensionRenderer";
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnLineSymbol;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnTextSymbol;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbLineCap;
        private System.Windows.Forms.TextBox txtFormat;
        private System.Windows.Forms.Label label4;
    }
}
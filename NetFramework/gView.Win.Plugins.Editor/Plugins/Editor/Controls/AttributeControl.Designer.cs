namespace gView.Plugins.Editor.Controls
{
    partial class AttributeControl
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
            this.panelFieldNames = new System.Windows.Forms.Panel();
            this.panelValues = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelFieldNames
            // 
            this.panelFieldNames.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelFieldNames.Location = new System.Drawing.Point(0, 0);
            this.panelFieldNames.Name = "panelFieldNames";
            this.panelFieldNames.Size = new System.Drawing.Size(68, 111);
            this.panelFieldNames.TabIndex = 0;
            // 
            // panelValues
            // 
            this.panelValues.BackColor = System.Drawing.Color.White;
            this.panelValues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelValues.Location = new System.Drawing.Point(68, 0);
            this.panelValues.Name = "panelValues";
            this.panelValues.Size = new System.Drawing.Size(182, 111);
            this.panelValues.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.splitter1);
            this.panel1.Controls.Add(this.panelValues);
            this.panel1.Controls.Add(this.panelFieldNames);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(250, 111);
            this.panel1.TabIndex = 3;
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.splitter1.Location = new System.Drawing.Point(68, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1, 111);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 111);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(250, 1);
            this.panel2.TabIndex = 4;
            // 
            // AttributeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "AttributeControl";
            this.Size = new System.Drawing.Size(250, 439);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelFieldNames;
        private System.Windows.Forms.Panel panelValues;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panel2;
    }
}

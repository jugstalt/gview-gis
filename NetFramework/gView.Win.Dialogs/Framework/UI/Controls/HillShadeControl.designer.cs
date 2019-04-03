namespace gView.Framework.UI.Controls
{
    partial class HillShadeControl
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
            this.panelCone = new System.Windows.Forms.Panel();
            this.btnLight = new System.Windows.Forms.Button();
            this.btnShaddow = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panelElevation = new System.Windows.Forms.Panel();
            this.panelElevationSlider = new System.Windows.Forms.Panel();
            this.numDx = new System.Windows.Forms.NumericUpDown();
            this.numDy = new System.Windows.Forms.NumericUpDown();
            this.numDz = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numDx)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDz)).BeginInit();
            this.SuspendLayout();
            // 
            // panelCone
            // 
            this.panelCone.Location = new System.Drawing.Point(16, 26);
            this.panelCone.Name = "panelCone";
            this.panelCone.Size = new System.Drawing.Size(135, 122);
            this.panelCone.TabIndex = 0;
            this.panelCone.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelCone_MouseDown);
            this.panelCone.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelCone_MouseMove);
            this.panelCone.Paint += new System.Windows.Forms.PaintEventHandler(this.panelCone_Paint);
            this.panelCone.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelCone_MouseUp);
            // 
            // btnLight
            // 
            this.btnLight.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLight.Location = new System.Drawing.Point(283, 112);
            this.btnLight.Name = "btnLight";
            this.btnLight.Size = new System.Drawing.Size(89, 23);
            this.btnLight.TabIndex = 1;
            this.btnLight.Text = "...";
            this.btnLight.UseVisualStyleBackColor = true;
            this.btnLight.Visible = false;
            this.btnLight.Click += new System.EventHandler(this.btnLight_Click);
            // 
            // btnShaddow
            // 
            this.btnShaddow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnShaddow.Location = new System.Drawing.Point(283, 141);
            this.btnShaddow.Name = "btnShaddow";
            this.btnShaddow.Size = new System.Drawing.Size(89, 23);
            this.btnShaddow.TabIndex = 2;
            this.btnShaddow.Text = "...";
            this.btnShaddow.UseVisualStyleBackColor = true;
            this.btnShaddow.Visible = false;
            this.btnShaddow.Click += new System.EventHandler(this.btnShaddow_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(242, 117);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Light:";
            this.label1.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(222, 146);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Shaddow:";
            this.label2.Visible = false;
            // 
            // panelElevation
            // 
            this.panelElevation.Location = new System.Drawing.Point(183, 26);
            this.panelElevation.Name = "panelElevation";
            this.panelElevation.Size = new System.Drawing.Size(18, 122);
            this.panelElevation.TabIndex = 5;
            this.panelElevation.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelElevation_MouseDown);
            this.panelElevation.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelElevation_MouseMove);
            this.panelElevation.Paint += new System.Windows.Forms.PaintEventHandler(this.panelElevation_Paint);
            this.panelElevation.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelElevation_MouseUp);
            // 
            // panelElevationSlider
            // 
            this.panelElevationSlider.Location = new System.Drawing.Point(173, 21);
            this.panelElevationSlider.Name = "panelElevationSlider";
            this.panelElevationSlider.Size = new System.Drawing.Size(11, 132);
            this.panelElevationSlider.TabIndex = 6;
            this.panelElevationSlider.Paint += new System.Windows.Forms.PaintEventHandler(this.panelElevationSlider_Paint);
            // 
            // numDx
            // 
            this.numDx.DecimalPlaces = 5;
            this.numDx.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numDx.Location = new System.Drawing.Point(283, 25);
            this.numDx.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numDx.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numDx.Name = "numDx";
            this.numDx.Size = new System.Drawing.Size(89, 20);
            this.numDx.TabIndex = 7;
            this.numDx.ValueChanged += new System.EventHandler(this.numDx_ValueChanged);
            // 
            // numDy
            // 
            this.numDy.DecimalPlaces = 5;
            this.numDy.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numDy.Location = new System.Drawing.Point(283, 51);
            this.numDy.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numDy.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numDy.Name = "numDy";
            this.numDy.Size = new System.Drawing.Size(89, 20);
            this.numDy.TabIndex = 8;
            this.numDy.ValueChanged += new System.EventHandler(this.numDy_ValueChanged);
            // 
            // numDz
            // 
            this.numDz.DecimalPlaces = 5;
            this.numDz.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numDz.Location = new System.Drawing.Point(283, 77);
            this.numDz.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numDz.Name = "numDz";
            this.numDz.Size = new System.Drawing.Size(89, 20);
            this.numDz.TabIndex = 9;
            this.numDz.ValueChanged += new System.EventHandler(this.numDz_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(257, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Dx:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(257, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Dy:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(257, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Dz:";
            // 
            // HillShadeControl
            // 
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numDz);
            this.Controls.Add(this.numDy);
            this.Controls.Add(this.numDx);
            this.Controls.Add(this.panelElevationSlider);
            this.Controls.Add(this.panelElevation);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnShaddow);
            this.Controls.Add(this.btnLight);
            this.Controls.Add(this.panelCone);
            this.Name = "HillShadeControl";
            this.Size = new System.Drawing.Size(393, 182);
            this.Load += new System.EventHandler(this.HillShadeControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numDx)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDz)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelCone;
        private System.Windows.Forms.Button btnLight;
        private System.Windows.Forms.Button btnShaddow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelElevation;
        private System.Windows.Forms.Panel panelElevationSlider;
        private System.Windows.Forms.NumericUpDown numDx;
        private System.Windows.Forms.NumericUpDown numDy;
        private System.Windows.Forms.NumericUpDown numDz;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}

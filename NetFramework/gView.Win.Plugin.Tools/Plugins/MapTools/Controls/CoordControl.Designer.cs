namespace gView.Plugins.MapTools.Controls
{
    partial class CoordControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CoordControl));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panelXY = new System.Windows.Forms.Panel();
            this.chkThousandsSeperator = new System.Windows.Forms.CheckBox();
            this.cmbDigits = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numY = new System.Windows.Forms.NumericUpDown();
            this.numX = new System.Windows.Forms.NumericUpDown();
            this.panelLonLat = new System.Windows.Forms.Panel();
            this.numLatSec = new System.Windows.Forms.NumericUpDown();
            this.numLonSec = new System.Windows.Forms.NumericUpDown();
            this.numLatMin = new System.Windows.Forms.NumericUpDown();
            this.numLonMin = new System.Windows.Forms.NumericUpDown();
            this.cmbDMS = new System.Windows.Forms.ComboBox();
            this.cmbLat = new System.Windows.Forms.ComboBox();
            this.cmbLon = new System.Windows.Forms.ComboBox();
            this.cmbDigits2 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numLat = new System.Windows.Forms.NumericUpDown();
            this.numLon = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panelXY.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numX)).BeginInit();
            this.panelLonLat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLatSec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLonSec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLatMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLonMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLat)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLon)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // panelXY
            // 
            this.panelXY.AccessibleDescription = null;
            this.panelXY.AccessibleName = null;
            resources.ApplyResources(this.panelXY, "panelXY");
            this.panelXY.BackgroundImage = null;
            this.panelXY.Controls.Add(this.chkThousandsSeperator);
            this.panelXY.Controls.Add(this.cmbDigits);
            this.panelXY.Controls.Add(this.label3);
            this.panelXY.Controls.Add(this.numY);
            this.panelXY.Controls.Add(this.numX);
            this.panelXY.Controls.Add(this.label2);
            this.panelXY.Controls.Add(this.label1);
            this.panelXY.Font = null;
            this.panelXY.Name = "panelXY";
            // 
            // chkThousandsSeperator
            // 
            this.chkThousandsSeperator.AccessibleDescription = null;
            this.chkThousandsSeperator.AccessibleName = null;
            resources.ApplyResources(this.chkThousandsSeperator, "chkThousandsSeperator");
            this.chkThousandsSeperator.BackgroundImage = null;
            this.chkThousandsSeperator.Font = null;
            this.chkThousandsSeperator.Name = "chkThousandsSeperator";
            this.chkThousandsSeperator.UseVisualStyleBackColor = true;
            this.chkThousandsSeperator.CheckedChanged += new System.EventHandler(this.chkThousandsSeperator_CheckedChanged);
            // 
            // cmbDigits
            // 
            this.cmbDigits.AccessibleDescription = null;
            this.cmbDigits.AccessibleName = null;
            resources.ApplyResources(this.cmbDigits, "cmbDigits");
            this.cmbDigits.BackgroundImage = null;
            this.cmbDigits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDigits.Font = null;
            this.cmbDigits.FormattingEnabled = true;
            this.cmbDigits.Name = "cmbDigits";
            this.cmbDigits.SelectedIndexChanged += new System.EventHandler(this.cmbDigits_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Font = null;
            this.label3.Name = "label3";
            // 
            // numY
            // 
            this.numY.AccessibleDescription = null;
            this.numY.AccessibleName = null;
            resources.ApplyResources(this.numY, "numY");
            this.numY.DecimalPlaces = 3;
            this.numY.Font = null;
            this.numY.Maximum = new decimal(new int[] {
            -1304428545,
            434162106,
            542,
            0});
            this.numY.Minimum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            -2147483648});
            this.numY.Name = "numY";
            // 
            // numX
            // 
            this.numX.AccessibleDescription = null;
            this.numX.AccessibleName = null;
            resources.ApplyResources(this.numX, "numX");
            this.numX.DecimalPlaces = 3;
            this.numX.Font = null;
            this.numX.Maximum = new decimal(new int[] {
            -1304428545,
            434162106,
            542,
            0});
            this.numX.Minimum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            -2147483648});
            this.numX.Name = "numX";
            // 
            // panelLonLat
            // 
            this.panelLonLat.AccessibleDescription = null;
            this.panelLonLat.AccessibleName = null;
            resources.ApplyResources(this.panelLonLat, "panelLonLat");
            this.panelLonLat.BackgroundImage = null;
            this.panelLonLat.Controls.Add(this.numLatSec);
            this.panelLonLat.Controls.Add(this.numLonSec);
            this.panelLonLat.Controls.Add(this.numLatMin);
            this.panelLonLat.Controls.Add(this.numLonMin);
            this.panelLonLat.Controls.Add(this.cmbDMS);
            this.panelLonLat.Controls.Add(this.cmbLat);
            this.panelLonLat.Controls.Add(this.cmbLon);
            this.panelLonLat.Controls.Add(this.cmbDigits2);
            this.panelLonLat.Controls.Add(this.label4);
            this.panelLonLat.Controls.Add(this.numLat);
            this.panelLonLat.Controls.Add(this.numLon);
            this.panelLonLat.Controls.Add(this.label5);
            this.panelLonLat.Controls.Add(this.label6);
            this.panelLonLat.Font = null;
            this.panelLonLat.Name = "panelLonLat";
            // 
            // numLatSec
            // 
            this.numLatSec.AccessibleDescription = null;
            this.numLatSec.AccessibleName = null;
            resources.ApplyResources(this.numLatSec, "numLatSec");
            this.numLatSec.DecimalPlaces = 3;
            this.numLatSec.Font = null;
            this.numLatSec.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numLatSec.Name = "numLatSec";
            this.numLatSec.ValueChanged += new System.EventHandler(this.numLonLat_ValueChanged);
            // 
            // numLonSec
            // 
            this.numLonSec.AccessibleDescription = null;
            this.numLonSec.AccessibleName = null;
            resources.ApplyResources(this.numLonSec, "numLonSec");
            this.numLonSec.DecimalPlaces = 3;
            this.numLonSec.Font = null;
            this.numLonSec.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numLonSec.Name = "numLonSec";
            this.numLonSec.ValueChanged += new System.EventHandler(this.numLonLat_ValueChanged);
            // 
            // numLatMin
            // 
            this.numLatMin.AccessibleDescription = null;
            this.numLatMin.AccessibleName = null;
            resources.ApplyResources(this.numLatMin, "numLatMin");
            this.numLatMin.DecimalPlaces = 3;
            this.numLatMin.Font = null;
            this.numLatMin.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numLatMin.Name = "numLatMin";
            this.numLatMin.ValueChanged += new System.EventHandler(this.numLonLat_ValueChanged);
            // 
            // numLonMin
            // 
            this.numLonMin.AccessibleDescription = null;
            this.numLonMin.AccessibleName = null;
            resources.ApplyResources(this.numLonMin, "numLonMin");
            this.numLonMin.DecimalPlaces = 3;
            this.numLonMin.Font = null;
            this.numLonMin.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numLonMin.Name = "numLonMin";
            this.numLonMin.ValueChanged += new System.EventHandler(this.numLonLat_ValueChanged);
            // 
            // cmbDMS
            // 
            this.cmbDMS.AccessibleDescription = null;
            this.cmbDMS.AccessibleName = null;
            resources.ApplyResources(this.cmbDMS, "cmbDMS");
            this.cmbDMS.BackgroundImage = null;
            this.cmbDMS.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDMS.Font = null;
            this.cmbDMS.FormattingEnabled = true;
            this.cmbDMS.Items.AddRange(new object[] {
            resources.GetString("cmbDMS.Items"),
            resources.GetString("cmbDMS.Items1"),
            resources.GetString("cmbDMS.Items2")});
            this.cmbDMS.Name = "cmbDMS";
            this.cmbDMS.TabStop = false;
            this.cmbDMS.SelectedIndexChanged += new System.EventHandler(this.cmbDMS_SelectedIndexChanged);
            // 
            // cmbLat
            // 
            this.cmbLat.AccessibleDescription = null;
            this.cmbLat.AccessibleName = null;
            resources.ApplyResources(this.cmbLat, "cmbLat");
            this.cmbLat.BackgroundImage = null;
            this.cmbLat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLat.Font = null;
            this.cmbLat.FormattingEnabled = true;
            this.cmbLat.Items.AddRange(new object[] {
            resources.GetString("cmbLat.Items"),
            resources.GetString("cmbLat.Items1")});
            this.cmbLat.Name = "cmbLat";
            // 
            // cmbLon
            // 
            this.cmbLon.AccessibleDescription = null;
            this.cmbLon.AccessibleName = null;
            resources.ApplyResources(this.cmbLon, "cmbLon");
            this.cmbLon.BackgroundImage = null;
            this.cmbLon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLon.Font = null;
            this.cmbLon.FormattingEnabled = true;
            this.cmbLon.Items.AddRange(new object[] {
            resources.GetString("cmbLon.Items"),
            resources.GetString("cmbLon.Items1")});
            this.cmbLon.Name = "cmbLon";
            // 
            // cmbDigits2
            // 
            this.cmbDigits2.AccessibleDescription = null;
            this.cmbDigits2.AccessibleName = null;
            resources.ApplyResources(this.cmbDigits2, "cmbDigits2");
            this.cmbDigits2.BackgroundImage = null;
            this.cmbDigits2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDigits2.Font = null;
            this.cmbDigits2.FormattingEnabled = true;
            this.cmbDigits2.Name = "cmbDigits2";
            this.cmbDigits2.TabStop = false;
            this.cmbDigits2.SelectedIndexChanged += new System.EventHandler(this.cmbDigits2_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AccessibleDescription = null;
            this.label4.AccessibleName = null;
            resources.ApplyResources(this.label4, "label4");
            this.label4.Font = null;
            this.label4.Name = "label4";
            // 
            // numLat
            // 
            this.numLat.AccessibleDescription = null;
            this.numLat.AccessibleName = null;
            resources.ApplyResources(this.numLat, "numLat");
            this.numLat.DecimalPlaces = 3;
            this.numLat.Font = null;
            this.numLat.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numLat.Name = "numLat";
            this.numLat.ValueChanged += new System.EventHandler(this.numLonLat_ValueChanged);
            // 
            // numLon
            // 
            this.numLon.AccessibleDescription = null;
            this.numLon.AccessibleName = null;
            resources.ApplyResources(this.numLon, "numLon");
            this.numLon.DecimalPlaces = 3;
            this.numLon.Font = null;
            this.numLon.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.numLon.Name = "numLon";
            this.numLon.ValueChanged += new System.EventHandler(this.numLonLat_ValueChanged);
            // 
            // label5
            // 
            this.label5.AccessibleDescription = null;
            this.label5.AccessibleName = null;
            resources.ApplyResources(this.label5, "label5");
            this.label5.Font = null;
            this.label5.Name = "label5";
            // 
            // label6
            // 
            this.label6.AccessibleDescription = null;
            this.label6.AccessibleName = null;
            resources.ApplyResources(this.label6, "label6");
            this.label6.Font = null;
            this.label6.Name = "label6";
            // 
            // CoordControl
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.panelLonLat);
            this.Controls.Add(this.panelXY);
            this.Font = null;
            this.Name = "CoordControl";
            this.panelXY.ResumeLayout(false);
            this.panelXY.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numX)).EndInit();
            this.panelLonLat.ResumeLayout(false);
            this.panelLonLat.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLatSec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLonSec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLatMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLonMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLat)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelXY;
        private System.Windows.Forms.NumericUpDown numY;
        private System.Windows.Forms.NumericUpDown numX;
        private System.Windows.Forms.CheckBox chkThousandsSeperator;
        private System.Windows.Forms.ComboBox cmbDigits;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panelLonLat;
        private System.Windows.Forms.ComboBox cmbLat;
        private System.Windows.Forms.ComboBox cmbLon;
        private System.Windows.Forms.ComboBox cmbDigits2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numLat;
        private System.Windows.Forms.NumericUpDown numLon;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbDMS;
        private System.Windows.Forms.NumericUpDown numLatSec;
        private System.Windows.Forms.NumericUpDown numLonSec;
        private System.Windows.Forms.NumericUpDown numLatMin;
        private System.Windows.Forms.NumericUpDown numLonMin;
    }
}

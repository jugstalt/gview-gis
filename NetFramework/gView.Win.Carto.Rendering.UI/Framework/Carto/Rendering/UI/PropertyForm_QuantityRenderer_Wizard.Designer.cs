namespace gView.Framework.Carto.Rendering.UI
{
    partial class PropertyForm_QuantityRenderer_Wizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyForm_QuantityRenderer_Wizard));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnMinSymbol = new System.Windows.Forms.Button();
            this.btnMaxSymbol = new System.Windows.Forms.Button();
            this.btnFixStepWidth = new System.Windows.Forms.RadioButton();
            this.btnFixStepCount = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.numStepCount = new gView.Framework.UI.Controls.IntegerTextBox();
            this.numStepWidth = new gView.Framework.UI.Controls.NumericTextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.numMin = new gView.Framework.UI.Controls.NumericTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numMax = new gView.Framework.UI.Controls.NumericTextBox();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
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
            // btnMinSymbol
            // 
            this.btnMinSymbol.AccessibleDescription = null;
            this.btnMinSymbol.AccessibleName = null;
            resources.ApplyResources(this.btnMinSymbol, "btnMinSymbol");
            this.btnMinSymbol.BackgroundImage = null;
            this.btnMinSymbol.Font = null;
            this.btnMinSymbol.Name = "btnMinSymbol";
            this.btnMinSymbol.UseVisualStyleBackColor = true;
            this.btnMinSymbol.Paint += new System.Windows.Forms.PaintEventHandler(this.btnMinSymbol_Paint);
            this.btnMinSymbol.Click += new System.EventHandler(this.btnMinSymbol_Click);
            // 
            // btnMaxSymbol
            // 
            this.btnMaxSymbol.AccessibleDescription = null;
            this.btnMaxSymbol.AccessibleName = null;
            resources.ApplyResources(this.btnMaxSymbol, "btnMaxSymbol");
            this.btnMaxSymbol.BackgroundImage = null;
            this.btnMaxSymbol.Font = null;
            this.btnMaxSymbol.Name = "btnMaxSymbol";
            this.btnMaxSymbol.UseVisualStyleBackColor = true;
            this.btnMaxSymbol.Paint += new System.Windows.Forms.PaintEventHandler(this.btnMaxSymbol_Paint);
            this.btnMaxSymbol.Click += new System.EventHandler(this.btnMaxSymbol_Click);
            // 
            // btnFixStepWidth
            // 
            this.btnFixStepWidth.AccessibleDescription = null;
            this.btnFixStepWidth.AccessibleName = null;
            resources.ApplyResources(this.btnFixStepWidth, "btnFixStepWidth");
            this.btnFixStepWidth.BackgroundImage = null;
            this.btnFixStepWidth.Font = null;
            this.btnFixStepWidth.Name = "btnFixStepWidth";
            this.btnFixStepWidth.TabStop = true;
            this.btnFixStepWidth.UseVisualStyleBackColor = true;
            // 
            // btnFixStepCount
            // 
            this.btnFixStepCount.AccessibleDescription = null;
            this.btnFixStepCount.AccessibleName = null;
            resources.ApplyResources(this.btnFixStepCount, "btnFixStepCount");
            this.btnFixStepCount.BackgroundImage = null;
            this.btnFixStepCount.Font = null;
            this.btnFixStepCount.Name = "btnFixStepCount";
            this.btnFixStepCount.TabStop = true;
            this.btnFixStepCount.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.AccessibleDescription = null;
            this.groupBox2.AccessibleName = null;
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.BackgroundImage = null;
            this.groupBox2.Controls.Add(this.numStepCount);
            this.groupBox2.Controls.Add(this.numStepWidth);
            this.groupBox2.Controls.Add(this.btnFixStepCount);
            this.groupBox2.Controls.Add(this.btnFixStepWidth);
            this.groupBox2.Font = null;
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // numStepCount
            // 
            this.numStepCount.AccessibleDescription = null;
            this.numStepCount.AccessibleName = null;
            this.numStepCount.AllowNegative = true;
            resources.ApplyResources(this.numStepCount, "numStepCount");
            this.numStepCount.BackgroundImage = null;
            this.numStepCount.DataType = gView.Framework.UI.Controls.NumericTextBox.NumericDataType.intType;
            this.numStepCount.DigitsInGroup = 0;
            this.numStepCount.Flags = 0;
            this.numStepCount.Font = null;
            this.numStepCount.MaxDecimalPlaces = 0;
            this.numStepCount.MaxWholeDigits = 10;
            this.numStepCount.Name = "numStepCount";
            this.numStepCount.Prefix = "";
            this.numStepCount.RangeMax = 2147483647;
            this.numStepCount.RangeMin = -2147483648;
            // 
            // numStepWidth
            // 
            this.numStepWidth.AccessibleDescription = null;
            this.numStepWidth.AccessibleName = null;
            this.numStepWidth.AllowNegative = true;
            resources.ApplyResources(this.numStepWidth, "numStepWidth");
            this.numStepWidth.BackgroundImage = null;
            this.numStepWidth.DataType = gView.Framework.UI.Controls.NumericTextBox.NumericDataType.doubleType;
            this.numStepWidth.DigitsInGroup = 0;
            this.numStepWidth.Flags = 0;
            this.numStepWidth.Font = null;
            this.numStepWidth.MaxDecimalPlaces = 4;
            this.numStepWidth.MaxWholeDigits = 9;
            this.numStepWidth.Name = "numStepWidth";
            this.numStepWidth.Prefix = "";
            this.numStepWidth.RangeMax = 1.7976931348623157E+308;
            this.numStepWidth.RangeMin = -1.7976931348623157E+308;
            // 
            // btnOK
            // 
            this.btnOK.AccessibleDescription = null;
            this.btnOK.AccessibleName = null;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.BackgroundImage = null;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Font = null;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleDescription = null;
            this.btnCancel.AccessibleName = null;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.BackgroundImage = null;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = null;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.AccessibleDescription = null;
            this.groupBox3.AccessibleName = null;
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.BackgroundImage = null;
            this.groupBox3.Controls.Add(this.numMin);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.btnMinSymbol);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Font = null;
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // numMin
            // 
            this.numMin.AccessibleDescription = null;
            this.numMin.AccessibleName = null;
            this.numMin.AllowNegative = true;
            resources.ApplyResources(this.numMin, "numMin");
            this.numMin.BackgroundImage = null;
            this.numMin.DataType = gView.Framework.UI.Controls.NumericTextBox.NumericDataType.floatType;
            this.numMin.DigitsInGroup = 0;
            this.numMin.Flags = 0;
            this.numMin.Font = null;
            this.numMin.MaxDecimalPlaces = 5;
            this.numMin.MaxWholeDigits = 9;
            this.numMin.Name = "numMin";
            this.numMin.Prefix = "";
            this.numMin.RangeMax = 3.4028234663852886E+38;
            this.numMin.RangeMin = -3.4028234663852886E+38;
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Font = null;
            this.label3.Name = "label3";
            // 
            // groupBox4
            // 
            this.groupBox4.AccessibleDescription = null;
            this.groupBox4.AccessibleName = null;
            resources.ApplyResources(this.groupBox4, "groupBox4");
            this.groupBox4.BackgroundImage = null;
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.numMax);
            this.groupBox4.Controls.Add(this.btnMaxSymbol);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Font = null;
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.TabStop = false;
            // 
            // label4
            // 
            this.label4.AccessibleDescription = null;
            this.label4.AccessibleName = null;
            resources.ApplyResources(this.label4, "label4");
            this.label4.Font = null;
            this.label4.Name = "label4";
            // 
            // numMax
            // 
            this.numMax.AccessibleDescription = null;
            this.numMax.AccessibleName = null;
            this.numMax.AllowNegative = true;
            resources.ApplyResources(this.numMax, "numMax");
            this.numMax.BackgroundImage = null;
            this.numMax.DataType = gView.Framework.UI.Controls.NumericTextBox.NumericDataType.floatType;
            this.numMax.DigitsInGroup = 0;
            this.numMax.Flags = 0;
            this.numMax.Font = null;
            this.numMax.MaxDecimalPlaces = 5;
            this.numMax.MaxWholeDigits = 9;
            this.numMax.Name = "numMax";
            this.numMax.Prefix = "";
            this.numMax.RangeMax = 3.4028234663852886E+38;
            this.numMax.RangeMin = -3.4028234663852886E+38;
            // 
            // PropertyForm_QuantityRenderer_Wizard
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox2);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "PropertyForm_QuantityRenderer_Wizard";
            this.Load += new System.EventHandler(this.PropertyForm_QuantityRenderer_Wizard_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnMinSymbol;
        private System.Windows.Forms.Button btnMaxSymbol;
        private System.Windows.Forms.RadioButton btnFixStepWidth;
        private System.Windows.Forms.RadioButton btnFixStepCount;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label3;
        private gView.Framework.UI.Controls.NumericTextBox numMin;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label4;
        private gView.Framework.UI.Controls.NumericTextBox numMax;
        private gView.Framework.UI.Controls.IntegerTextBox numStepCount;
        private gView.Framework.UI.Controls.NumericTextBox numStepWidth;
    }
}
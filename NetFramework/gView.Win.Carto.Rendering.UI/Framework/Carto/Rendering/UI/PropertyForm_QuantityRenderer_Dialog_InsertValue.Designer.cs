namespace gView.Framework.Carto.Rendering.UI
{
    partial class PropertyForm_QuantityRenderer_Dialog_InsertValue
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyForm_QuantityRenderer_Dialog_InsertValue));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLabel = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.numTo = new gView.Framework.UI.Controls.NumericTextBox();
            this.numFrom = new gView.Framework.UI.Controls.NumericTextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // txtLabel
            // 
            this.txtLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtLabel, "txtLabel");
            this.txtLabel.Name = "txtLabel";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // numTo
            // 
            this.numTo.AllowNegative = true;
            this.numTo.DataType = gView.Framework.UI.Controls.NumericTextBox.NumericDataType.doubleType;
            this.numTo.DigitsInGroup = 0;
            this.numTo.Flags = 0;
            resources.ApplyResources(this.numTo, "numTo");
            this.numTo.MaxDecimalPlaces = 4;
            this.numTo.MaxWholeDigits = 9;
            this.numTo.Name = "numTo";
            this.numTo.Prefix = "";
            this.numTo.RangeMax = 1.7976931348623157E+308;
            this.numTo.RangeMin = -1.7976931348623157E+308;
            // 
            // numFrom
            // 
            this.numFrom.AllowNegative = true;
            this.numFrom.DataType = gView.Framework.UI.Controls.NumericTextBox.NumericDataType.doubleType;
            this.numFrom.DigitsInGroup = 0;
            this.numFrom.Flags = 0;
            resources.ApplyResources(this.numFrom, "numFrom");
            this.numFrom.MaxDecimalPlaces = 4;
            this.numFrom.MaxWholeDigits = 9;
            this.numFrom.Name = "numFrom";
            this.numFrom.Prefix = "";
            this.numFrom.RangeMax = 1.7976931348623157E+308;
            this.numFrom.RangeMin = -1.7976931348623157E+308;
            // 
            // PropertyForm_QuantityRenderer_Dialog_InsertValue
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.numTo);
            this.Controls.Add(this.numFrom);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PropertyForm_QuantityRenderer_Dialog_InsertValue";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLabel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label3;
        private gView.Framework.UI.Controls.NumericTextBox numTo;
        private gView.Framework.UI.Controls.NumericTextBox numFrom;
    }
}
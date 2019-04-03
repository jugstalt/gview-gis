namespace gView.Framework.UI.Dialogs
{
    partial class FormGridColorClass
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGridColorClass));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.numMinValue = new System.Windows.Forms.NumericUpDown();
            this.numMaxValue = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLabel = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numMinValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxValue)).BeginInit();
            this.SuspendLayout();
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
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // numMinValue
            // 
            this.numMinValue.AccessibleDescription = null;
            this.numMinValue.AccessibleName = null;
            resources.ApplyResources(this.numMinValue, "numMinValue");
            this.numMinValue.Font = null;
            this.numMinValue.Maximum = new decimal(new int[] {
            268435455,
            1042612833,
            542101086,
            0});
            this.numMinValue.Minimum = new decimal(new int[] {
            268435455,
            1042612833,
            542101086,
            -2147483648});
            this.numMinValue.Name = "numMinValue";
            // 
            // numMaxValue
            // 
            this.numMaxValue.AccessibleDescription = null;
            this.numMaxValue.AccessibleName = null;
            resources.ApplyResources(this.numMaxValue, "numMaxValue");
            this.numMaxValue.Font = null;
            this.numMaxValue.Maximum = new decimal(new int[] {
            268435455,
            1042612833,
            542101086,
            0});
            this.numMaxValue.Minimum = new decimal(new int[] {
            1241513983,
            370409800,
            542101,
            -2147483648});
            this.numMaxValue.Name = "numMaxValue";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Font = null;
            this.label3.Name = "label3";
            // 
            // txtLabel
            // 
            this.txtLabel.AccessibleDescription = null;
            this.txtLabel.AccessibleName = null;
            resources.ApplyResources(this.txtLabel, "txtLabel");
            this.txtLabel.BackgroundImage = null;
            this.txtLabel.Font = null;
            this.txtLabel.Name = "txtLabel";
            // 
            // FormGridColorClass
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.txtLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numMaxValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numMinValue);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormGridColorClass";
            ((System.ComponentModel.ISupportInitialize)(this.numMinValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxValue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numMinValue;
        private System.Windows.Forms.NumericUpDown numMaxValue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtLabel;
    }
}
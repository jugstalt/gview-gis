namespace gView.Framework.UI.Dialogs
{
    partial class FormGridWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGridWizard));
            this.label1 = new System.Windows.Forms.Label();
            this.numMinValue = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.chkUseAverage = new System.Windows.Forms.CheckBox();
            this.btnColMin = new System.Windows.Forms.Button();
            this.btnColAverage = new System.Windows.Forms.Button();
            this.btnColMax = new System.Windows.Forms.Button();
            this.numAverageValue = new System.Windows.Forms.NumericUpDown();
            this.numMaxValue = new System.Windows.Forms.NumericUpDown();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.numLevelEvery = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numLabelEvery = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numMinValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAverageValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLevelEvery)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLabelEvery)).BeginInit();
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
            // numMinValue
            // 
            this.numMinValue.AccessibleDescription = null;
            this.numMinValue.AccessibleName = null;
            resources.ApplyResources(this.numMinValue, "numMinValue");
            this.numMinValue.DecimalPlaces = 5;
            this.numMinValue.Font = null;
            this.numMinValue.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numMinValue.Maximum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            0});
            this.numMinValue.Minimum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            -2147483648});
            this.numMinValue.Name = "numMinValue";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // chkUseAverage
            // 
            this.chkUseAverage.AccessibleDescription = null;
            this.chkUseAverage.AccessibleName = null;
            resources.ApplyResources(this.chkUseAverage, "chkUseAverage");
            this.chkUseAverage.BackgroundImage = null;
            this.chkUseAverage.Checked = true;
            this.chkUseAverage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseAverage.Font = null;
            this.chkUseAverage.Name = "chkUseAverage";
            this.chkUseAverage.UseVisualStyleBackColor = true;
            // 
            // btnColMin
            // 
            this.btnColMin.AccessibleDescription = null;
            this.btnColMin.AccessibleName = null;
            resources.ApplyResources(this.btnColMin, "btnColMin");
            this.btnColMin.BackgroundImage = null;
            this.btnColMin.Font = null;
            this.btnColMin.Name = "btnColMin";
            this.btnColMin.UseVisualStyleBackColor = true;
            this.btnColMin.Click += new System.EventHandler(this.btnCol_Click);
            // 
            // btnColAverage
            // 
            this.btnColAverage.AccessibleDescription = null;
            this.btnColAverage.AccessibleName = null;
            resources.ApplyResources(this.btnColAverage, "btnColAverage");
            this.btnColAverage.BackgroundImage = null;
            this.btnColAverage.Font = null;
            this.btnColAverage.Name = "btnColAverage";
            this.btnColAverage.UseVisualStyleBackColor = true;
            this.btnColAverage.Click += new System.EventHandler(this.btnCol_Click);
            // 
            // btnColMax
            // 
            this.btnColMax.AccessibleDescription = null;
            this.btnColMax.AccessibleName = null;
            resources.ApplyResources(this.btnColMax, "btnColMax");
            this.btnColMax.BackgroundImage = null;
            this.btnColMax.Font = null;
            this.btnColMax.Name = "btnColMax";
            this.btnColMax.UseVisualStyleBackColor = true;
            this.btnColMax.Click += new System.EventHandler(this.btnCol_Click);
            // 
            // numAverageValue
            // 
            this.numAverageValue.AccessibleDescription = null;
            this.numAverageValue.AccessibleName = null;
            resources.ApplyResources(this.numAverageValue, "numAverageValue");
            this.numAverageValue.DecimalPlaces = 5;
            this.numAverageValue.Font = null;
            this.numAverageValue.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numAverageValue.Maximum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            0});
            this.numAverageValue.Minimum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            -2147483648});
            this.numAverageValue.Name = "numAverageValue";
            // 
            // numMaxValue
            // 
            this.numMaxValue.AccessibleDescription = null;
            this.numMaxValue.AccessibleName = null;
            resources.ApplyResources(this.numMaxValue, "numMaxValue");
            this.numMaxValue.DecimalPlaces = 5;
            this.numMaxValue.Font = null;
            this.numMaxValue.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numMaxValue.Maximum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            0});
            this.numMaxValue.Minimum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            -2147483648});
            this.numMaxValue.Name = "numMaxValue";
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
            // numLevelEvery
            // 
            this.numLevelEvery.AccessibleDescription = null;
            this.numLevelEvery.AccessibleName = null;
            resources.ApplyResources(this.numLevelEvery, "numLevelEvery");
            this.numLevelEvery.DecimalPlaces = 5;
            this.numLevelEvery.Font = null;
            this.numLevelEvery.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numLevelEvery.Maximum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            0});
            this.numLevelEvery.Minimum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            -2147483648});
            this.numLevelEvery.Name = "numLevelEvery";
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Font = null;
            this.label3.Name = "label3";
            // 
            // numLabelEvery
            // 
            this.numLabelEvery.AccessibleDescription = null;
            this.numLabelEvery.AccessibleName = null;
            resources.ApplyResources(this.numLabelEvery, "numLabelEvery");
            this.numLabelEvery.DecimalPlaces = 5;
            this.numLabelEvery.Font = null;
            this.numLabelEvery.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numLabelEvery.Maximum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            0});
            this.numLabelEvery.Minimum = new decimal(new int[] {
            -469762049,
            -590869294,
            5421010,
            -2147483648});
            this.numLabelEvery.Name = "numLabelEvery";
            // 
            // label4
            // 
            this.label4.AccessibleDescription = null;
            this.label4.AccessibleName = null;
            resources.ApplyResources(this.label4, "label4");
            this.label4.Font = null;
            this.label4.Name = "label4";
            // 
            // FormGridWizard
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.numLabelEvery);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numLevelEvery);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.numMaxValue);
            this.Controls.Add(this.numAverageValue);
            this.Controls.Add(this.btnColMax);
            this.Controls.Add(this.btnColAverage);
            this.Controls.Add(this.btnColMin);
            this.Controls.Add(this.chkUseAverage);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numMinValue);
            this.Controls.Add(this.label1);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = null;
            this.Name = "FormGridWizard";
            ((System.ComponentModel.ISupportInitialize)(this.numMinValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAverageValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLevelEvery)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLabelEvery)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numMinValue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkUseAverage;
        private System.Windows.Forms.Button btnColMin;
        private System.Windows.Forms.Button btnColAverage;
        private System.Windows.Forms.Button btnColMax;
        private System.Windows.Forms.NumericUpDown numAverageValue;
        private System.Windows.Forms.NumericUpDown numMaxValue;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.NumericUpDown numLevelEvery;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numLabelEvery;
        private System.Windows.Forms.Label label4;
    }
}
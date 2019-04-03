namespace gView.Plugins.MapTools.Dialogs
{
    partial class FormPrintPreview
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPrintPreview));
            this.printPreviewControl1 = new System.Windows.Forms.PrintPreviewControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.numResolution = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnPrinter = new System.Windows.Forms.Button();
            this.btnPageSetup = new System.Windows.Forms.Button();
            this.pageSetupDialog1 = new System.Windows.Forms.PageSetupDialog();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numResolution)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // printPreviewControl1
            // 
            this.printPreviewControl1.AccessibleDescription = null;
            this.printPreviewControl1.AccessibleName = null;
            resources.ApplyResources(this.printPreviewControl1, "printPreviewControl1");
            this.printPreviewControl1.BackgroundImage = null;
            this.printPreviewControl1.Font = null;
            this.printPreviewControl1.Name = "printPreviewControl1";
            // 
            // panel1
            // 
            this.panel1.AccessibleDescription = null;
            this.panel1.AccessibleName = null;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackgroundImage = null;
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.numResolution);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.comboBox1);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btnPrinter);
            this.panel1.Controls.Add(this.btnPageSetup);
            this.panel1.Font = null;
            this.panel1.Name = "panel1";
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Font = null;
            this.label3.Name = "label3";
            // 
            // numResolution
            // 
            this.numResolution.AccessibleDescription = null;
            this.numResolution.AccessibleName = null;
            resources.ApplyResources(this.numResolution, "numResolution");
            this.numResolution.Font = null;
            this.numResolution.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numResolution.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numResolution.Name = "numResolution";
            this.numResolution.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numResolution.ValueChanged += new System.EventHandler(this.numResolution_ValueChanged);
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // comboBox1
            // 
            this.comboBox1.AccessibleDescription = null;
            this.comboBox1.AccessibleName = null;
            resources.ApplyResources(this.comboBox1, "comboBox1");
            this.comboBox1.BackgroundImage = null;
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Font = null;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            resources.GetString("comboBox1.Items"),
            resources.GetString("comboBox1.Items1"),
            resources.GetString("comboBox1.Items2"),
            resources.GetString("comboBox1.Items3"),
            resources.GetString("comboBox1.Items4"),
            resources.GetString("comboBox1.Items5"),
            resources.GetString("comboBox1.Items6"),
            resources.GetString("comboBox1.Items7"),
            resources.GetString("comboBox1.Items8"),
            resources.GetString("comboBox1.Items9"),
            resources.GetString("comboBox1.Items10"),
            resources.GetString("comboBox1.Items11")});
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // panel2
            // 
            this.panel2.AccessibleDescription = null;
            this.panel2.AccessibleName = null;
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.BackgroundImage = null;
            this.panel2.Controls.Add(this.btnPrint);
            this.panel2.Font = null;
            this.panel2.Name = "panel2";
            // 
            // btnPrint
            // 
            this.btnPrint.AccessibleDescription = null;
            this.btnPrint.AccessibleName = null;
            resources.ApplyResources(this.btnPrint, "btnPrint");
            this.btnPrint.BackgroundImage = null;
            this.btnPrint.Font = null;
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click_1);
            // 
            // btnPrinter
            // 
            this.btnPrinter.AccessibleDescription = null;
            this.btnPrinter.AccessibleName = null;
            resources.ApplyResources(this.btnPrinter, "btnPrinter");
            this.btnPrinter.BackgroundImage = null;
            this.btnPrinter.Font = null;
            this.btnPrinter.Name = "btnPrinter";
            this.btnPrinter.UseVisualStyleBackColor = true;
            this.btnPrinter.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btnPageSetup
            // 
            this.btnPageSetup.AccessibleDescription = null;
            this.btnPageSetup.AccessibleName = null;
            resources.ApplyResources(this.btnPageSetup, "btnPageSetup");
            this.btnPageSetup.BackgroundImage = null;
            this.btnPageSetup.Font = null;
            this.btnPageSetup.Name = "btnPageSetup";
            this.btnPageSetup.UseVisualStyleBackColor = true;
            this.btnPageSetup.Click += new System.EventHandler(this.btnPageSetup_Click);
            // 
            // printDialog1
            // 
            this.printDialog1.UseEXDialog = true;
            // 
            // FormPrintPreview
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.printPreviewControl1);
            this.Controls.Add(this.panel1);
            this.Font = null;
            this.Name = "FormPrintPreview";
            this.Load += new System.EventHandler(this.FormPrintPreview_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numResolution)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PrintPreviewControl printPreviewControl1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnPageSetup;
        private System.Windows.Forms.PageSetupDialog pageSetupDialog1;
        private System.Windows.Forms.Button btnPrinter;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numResolution;
        private System.Windows.Forms.Label label2;
    }
}
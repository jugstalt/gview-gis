namespace gView.Framework.UI.Dialogs
{
    partial class FormMapProperties
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMapProperties));
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbMapUnits = new System.Windows.Forms.ComboBox();
            this.cmbDisplayUnits = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabMap = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnNoAntialiasFeatures = new System.Windows.Forms.Button();
            this.btnAntialiasFeatures = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnNoAntialiasLabels = new System.Windows.Forms.Button();
            this.btnAntialiasLabels = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnTransparent = new System.Windows.Forms.Button();
            this.btnBackgroundColor = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.numRefScale = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabSR = new System.Windows.Forms.TabPage();
            this.tabDefLayerSR = new System.Windows.Forms.TabPage();
            this.panelDefaultLayerSR = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.btnDefaultLayerSRFromSpatialReference = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabMap.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRefScale)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tabDefLayerSR.SuspendLayout();
            this.panel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // txtName
            // 
            resources.ApplyResources(this.txtName, "txtName");
            this.txtName.Name = "txtName";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // cmbMapUnits
            // 
            resources.ApplyResources(this.cmbMapUnits, "cmbMapUnits");
            this.cmbMapUnits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMapUnits.FormattingEnabled = true;
            this.cmbMapUnits.Name = "cmbMapUnits";
            // 
            // cmbDisplayUnits
            // 
            resources.ApplyResources(this.cmbDisplayUnits, "cmbDisplayUnits");
            this.cmbDisplayUnits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDisplayUnits.FormattingEnabled = true;
            this.cmbDisplayUnits.Name = "cmbDisplayUnits";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Name = "panel1";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.btnOK);
            this.panel2.Name = "panel2";
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabMap);
            this.tabControl1.Controls.Add(this.tabSR);
            this.tabControl1.Controls.Add(this.tabDefLayerSR);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabMap
            // 
            resources.ApplyResources(this.tabMap, "tabMap");
            this.tabMap.Controls.Add(this.groupBox3);
            this.tabMap.Controls.Add(this.groupBox2);
            this.tabMap.Controls.Add(this.groupBox1);
            this.tabMap.Name = "tabMap";
            this.tabMap.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Controls.Add(this.panel4);
            this.groupBox3.Controls.Add(this.panel3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // panel4
            // 
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.BackColor = System.Drawing.Color.Gainsboro;
            this.panel4.Controls.Add(this.btnNoAntialiasFeatures);
            this.panel4.Controls.Add(this.btnAntialiasFeatures);
            this.panel4.Controls.Add(this.label7);
            this.panel4.Name = "panel4";
            // 
            // btnNoAntialiasFeatures
            // 
            resources.ApplyResources(this.btnNoAntialiasFeatures, "btnNoAntialiasFeatures");
            this.btnNoAntialiasFeatures.Name = "btnNoAntialiasFeatures";
            this.btnNoAntialiasFeatures.UseVisualStyleBackColor = true;
            this.btnNoAntialiasFeatures.Click += new System.EventHandler(this.btnNoAntialiasFeatures_Click);
            // 
            // btnAntialiasFeatures
            // 
            resources.ApplyResources(this.btnAntialiasFeatures, "btnAntialiasFeatures");
            this.btnAntialiasFeatures.Name = "btnAntialiasFeatures";
            this.btnAntialiasFeatures.UseVisualStyleBackColor = true;
            this.btnAntialiasFeatures.Click += new System.EventHandler(this.btnAntialiasFeatures_Click);
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.BackColor = System.Drawing.Color.Gainsboro;
            this.panel3.Controls.Add(this.btnNoAntialiasLabels);
            this.panel3.Controls.Add(this.btnAntialiasLabels);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Name = "panel3";
            // 
            // btnNoAntialiasLabels
            // 
            resources.ApplyResources(this.btnNoAntialiasLabels, "btnNoAntialiasLabels");
            this.btnNoAntialiasLabels.Name = "btnNoAntialiasLabels";
            this.btnNoAntialiasLabels.UseVisualStyleBackColor = true;
            this.btnNoAntialiasLabels.Click += new System.EventHandler(this.btnNoAntialiasLabels_Click);
            // 
            // btnAntialiasLabels
            // 
            resources.ApplyResources(this.btnAntialiasLabels, "btnAntialiasLabels");
            this.btnAntialiasLabels.Name = "btnAntialiasLabels";
            this.btnAntialiasLabels.UseVisualStyleBackColor = true;
            this.btnAntialiasLabels.Click += new System.EventHandler(this.btnAntialiasLabels_Click);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.btnTransparent);
            this.groupBox2.Controls.Add(this.btnBackgroundColor);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.numRefScale);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.cmbDisplayUnits);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.cmbMapUnits);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // btnTransparent
            // 
            resources.ApplyResources(this.btnTransparent, "btnTransparent");
            this.btnTransparent.Name = "btnTransparent";
            this.btnTransparent.UseVisualStyleBackColor = true;
            this.btnTransparent.Click += new System.EventHandler(this.btnTransparent_Click);
            // 
            // btnBackgroundColor
            // 
            resources.ApplyResources(this.btnBackgroundColor, "btnBackgroundColor");
            this.btnBackgroundColor.Name = "btnBackgroundColor";
            this.btnBackgroundColor.UseVisualStyleBackColor = true;
            this.btnBackgroundColor.Click += new System.EventHandler(this.btnBackgroundColor_Click);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // numRefScale
            // 
            resources.ApplyResources(this.numRefScale, "numRefScale");
            this.numRefScale.Maximum = new decimal(new int[] {
            -1981284353,
            -1966660860,
            0,
            0});
            this.numRefScale.Name = "numRefScale";
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.txtName);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // tabSR
            // 
            resources.ApplyResources(this.tabSR, "tabSR");
            this.tabSR.Name = "tabSR";
            this.tabSR.UseVisualStyleBackColor = true;
            // 
            // tabDefLayerSR
            // 
            resources.ApplyResources(this.tabDefLayerSR, "tabDefLayerSR");
            this.tabDefLayerSR.Controls.Add(this.panelDefaultLayerSR);
            this.tabDefLayerSR.Controls.Add(this.panel5);
            this.tabDefLayerSR.Name = "tabDefLayerSR";
            this.tabDefLayerSR.UseVisualStyleBackColor = true;
            // 
            // panelDefaultLayerSR
            // 
            resources.ApplyResources(this.panelDefaultLayerSR, "panelDefaultLayerSR");
            this.panelDefaultLayerSR.Name = "panelDefaultLayerSR";
            // 
            // panel5
            // 
            resources.ApplyResources(this.panel5, "panel5");
            this.panel5.Controls.Add(this.btnDefaultLayerSRFromSpatialReference);
            this.panel5.Controls.Add(this.label8);
            this.panel5.Name = "panel5";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // btnDefaultLayerSRFromSpatialReference
            // 
            resources.ApplyResources(this.btnDefaultLayerSRFromSpatialReference, "btnDefaultLayerSRFromSpatialReference");
            this.btnDefaultLayerSRFromSpatialReference.Name = "btnDefaultLayerSRFromSpatialReference";
            this.btnDefaultLayerSRFromSpatialReference.UseVisualStyleBackColor = true;
            this.btnDefaultLayerSRFromSpatialReference.Click += new System.EventHandler(this.btnDefaultLayerSRFromSpatialReference_Click);
            // 
            // FormMapProperties
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormMapProperties";
            this.Load += new System.EventHandler(this.FormMapProperties_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabMap.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRefScale)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabDefLayerSR.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbMapUnits;
        private System.Windows.Forms.ComboBox cmbDisplayUnits;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabMap;
        private System.Windows.Forms.NumericUpDown numRefScale;
        private System.Windows.Forms.TabPage tabSR;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnBackgroundColor;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnTransparent;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnAntialiasLabels;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnAntialiasFeatures;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnNoAntialiasLabels;
        private System.Windows.Forms.Button btnNoAntialiasFeatures;
        private System.Windows.Forms.TabPage tabDefLayerSR;
        private System.Windows.Forms.Panel panelDefaultLayerSR;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnDefaultLayerSRFromSpatialReference;
    }
}
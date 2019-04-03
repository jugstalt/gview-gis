namespace gView.Plugins.MapTools.Dialogs
{
    partial class OptionPageIdentify
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionPageIdentify));
            this.OptionPanel = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numTolerance = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnMethodRubberband = new System.Windows.Forms.RadioButton();
            this.btnMethodClick = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnQueryThemeEditor = new System.Windows.Forms.Button();
            this.btnCustom = new System.Windows.Forms.RadioButton();
            this.btnDefault = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.OptionPanel.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTolerance)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // OptionPanel
            // 
            this.OptionPanel.Controls.Add(this.groupBox2);
            this.OptionPanel.Controls.Add(this.groupBox1);
            this.OptionPanel.Controls.Add(this.panel1);
            this.OptionPanel.Controls.Add(this.groupBox3);
            this.OptionPanel.Controls.Add(this.panel2);
            resources.ApplyResources(this.OptionPanel, "OptionPanel");
            this.OptionPanel.Name = "OptionPanel";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.numTolerance);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // numTolerance
            // 
            this.numTolerance.DecimalPlaces = 2;
            resources.ApplyResources(this.numTolerance, "numTolerance");
            this.numTolerance.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numTolerance.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numTolerance.Name = "numTolerance";
            this.numTolerance.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnMethodRubberband);
            this.groupBox1.Controls.Add(this.btnMethodClick);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // btnMethodRubberband
            // 
            resources.ApplyResources(this.btnMethodRubberband, "btnMethodRubberband");
            this.btnMethodRubberband.Name = "btnMethodRubberband";
            this.btnMethodRubberband.TabStop = true;
            this.btnMethodRubberband.UseVisualStyleBackColor = true;
            // 
            // btnMethodClick
            // 
            resources.ApplyResources(this.btnMethodClick, "btnMethodClick");
            this.btnMethodClick.Name = "btnMethodClick";
            this.btnMethodClick.TabStop = true;
            this.btnMethodClick.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnQueryThemeEditor);
            this.groupBox3.Controls.Add(this.btnCustom);
            this.groupBox3.Controls.Add(this.btnDefault);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // btnQueryThemeEditor
            // 
            resources.ApplyResources(this.btnQueryThemeEditor, "btnQueryThemeEditor");
            this.btnQueryThemeEditor.Name = "btnQueryThemeEditor";
            this.btnQueryThemeEditor.UseVisualStyleBackColor = true;
            this.btnQueryThemeEditor.Click += new System.EventHandler(this.btnQueryThemeEditor_Click);
            // 
            // btnCustom
            // 
            resources.ApplyResources(this.btnCustom, "btnCustom");
            this.btnCustom.Name = "btnCustom";
            this.btnCustom.TabStop = true;
            this.btnCustom.UseVisualStyleBackColor = true;
            // 
            // btnDefault
            // 
            resources.ApplyResources(this.btnDefault, "btnDefault");
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.TabStop = true;
            this.btnDefault.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "identify.png");
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // OptionPageIdentify
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.OptionPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "OptionPageIdentify";
            this.OptionPanel.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTolerance)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numTolerance;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton btnMethodRubberband;
        private System.Windows.Forms.RadioButton btnMethodClick;
        internal System.Windows.Forms.Panel OptionPanel;
        internal System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton btnCustom;
        private System.Windows.Forms.RadioButton btnDefault;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnQueryThemeEditor;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
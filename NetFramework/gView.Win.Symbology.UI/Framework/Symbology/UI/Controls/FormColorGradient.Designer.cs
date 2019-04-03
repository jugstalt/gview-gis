namespace gView.Framework.Symbology.UI.Controls
{
    partial class FormColorGradient
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormColorGradient));
            this.btnColor1 = new System.Windows.Forms.Button();
            this.btnColor2 = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panelPreview = new System.Windows.Forms.Panel();
            this.shadeAngleSetter1 = new gView.Framework.Symbology.UI.Controls.ShadeAngleSetter();
            this.btnTrans1 = new System.Windows.Forms.Button();
            this.btnTrans2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnColor1
            // 
            this.btnColor1.AccessibleDescription = null;
            this.btnColor1.AccessibleName = null;
            resources.ApplyResources(this.btnColor1, "btnColor1");
            this.btnColor1.BackgroundImage = null;
            this.btnColor1.Font = null;
            this.btnColor1.Name = "btnColor1";
            this.btnColor1.UseVisualStyleBackColor = true;
            this.btnColor1.Click += new System.EventHandler(this.btnColor1_Click);
            // 
            // btnColor2
            // 
            this.btnColor2.AccessibleDescription = null;
            this.btnColor2.AccessibleName = null;
            resources.ApplyResources(this.btnColor2, "btnColor2");
            this.btnColor2.BackgroundImage = null;
            this.btnColor2.Font = null;
            this.btnColor2.Name = "btnColor2";
            this.btnColor2.UseVisualStyleBackColor = true;
            this.btnColor2.Click += new System.EventHandler(this.btnColor2_Click);
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
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = null;
            this.groupBox1.AccessibleName = null;
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.BackgroundImage = null;
            this.groupBox1.Controls.Add(this.panelPreview);
            this.groupBox1.Font = null;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // panelPreview
            // 
            this.panelPreview.AccessibleDescription = null;
            this.panelPreview.AccessibleName = null;
            resources.ApplyResources(this.panelPreview, "panelPreview");
            this.panelPreview.BackgroundImage = null;
            this.panelPreview.Font = null;
            this.panelPreview.Name = "panelPreview";
            this.panelPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.panelPreview_Paint);
            // 
            // shadeAngleSetter1
            // 
            this.shadeAngleSetter1.AccessibleDescription = null;
            this.shadeAngleSetter1.AccessibleName = null;
            resources.ApplyResources(this.shadeAngleSetter1, "shadeAngleSetter1");
            this.shadeAngleSetter1.Angle2D = 0;
            this.shadeAngleSetter1.BackgroundImage = null;
            this.shadeAngleSetter1.Dx = 1;
            this.shadeAngleSetter1.Dy = 0;
            this.shadeAngleSetter1.Dz = 1;
            this.shadeAngleSetter1.Font = null;
            this.shadeAngleSetter1.Name = "shadeAngleSetter1";
            this.shadeAngleSetter1.ValueChanged += new System.EventHandler(this.shadeAngleSetter1_ValueChanged);
            // 
            // btnTrans1
            // 
            this.btnTrans1.AccessibleDescription = null;
            this.btnTrans1.AccessibleName = null;
            resources.ApplyResources(this.btnTrans1, "btnTrans1");
            this.btnTrans1.BackgroundImage = null;
            this.btnTrans1.Font = null;
            this.btnTrans1.Name = "btnTrans1";
            this.btnTrans1.UseVisualStyleBackColor = true;
            this.btnTrans1.Click += new System.EventHandler(this.btnTrans1_Click);
            // 
            // btnTrans2
            // 
            this.btnTrans2.AccessibleDescription = null;
            this.btnTrans2.AccessibleName = null;
            resources.ApplyResources(this.btnTrans2, "btnTrans2");
            this.btnTrans2.BackgroundImage = null;
            this.btnTrans2.Font = null;
            this.btnTrans2.Name = "btnTrans2";
            this.btnTrans2.UseVisualStyleBackColor = true;
            this.btnTrans2.Click += new System.EventHandler(this.btnTrans2_Click);
            // 
            // FormColorGradient
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.btnTrans2);
            this.Controls.Add(this.btnTrans1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnColor2);
            this.Controls.Add(this.btnColor1);
            this.Controls.Add(this.shadeAngleSetter1);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormColorGradient";
            this.Load += new System.EventHandler(this.FormColorGradient_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ShadeAngleSetter shadeAngleSetter1;
        private System.Windows.Forms.Button btnColor1;
        private System.Windows.Forms.Button btnColor2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panelPreview;
        private System.Windows.Forms.Button btnTrans1;
        private System.Windows.Forms.Button btnTrans2;
    }
}
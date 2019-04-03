using gView.Plugins.MapTools.Controls;
namespace gView.Plugins.MapTools.Dialogs
{
    partial class FormXY
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormXY));
            gView.Framework.Geometry.Point point1 = new gView.Framework.Geometry.Point();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbSRef = new System.Windows.Forms.ComboBox();
            this.btnGetSRef = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.coordControl1 = new CoordControl();
            this.groupBox1.SuspendLayout();
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
            // cmbSRef
            // 
            this.cmbSRef.AccessibleDescription = null;
            this.cmbSRef.AccessibleName = null;
            resources.ApplyResources(this.cmbSRef, "cmbSRef");
            this.cmbSRef.BackgroundImage = null;
            this.cmbSRef.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSRef.Font = null;
            this.cmbSRef.FormattingEnabled = true;
            this.cmbSRef.Name = "cmbSRef";
            this.cmbSRef.SelectedIndexChanged += new System.EventHandler(this.cmbSRef_SelectedIndexChanged);
            // 
            // btnGetSRef
            // 
            this.btnGetSRef.AccessibleDescription = null;
            this.btnGetSRef.AccessibleName = null;
            resources.ApplyResources(this.btnGetSRef, "btnGetSRef");
            this.btnGetSRef.BackgroundImage = null;
            this.btnGetSRef.Font = null;
            this.btnGetSRef.Name = "btnGetSRef";
            this.btnGetSRef.UseVisualStyleBackColor = true;
            this.btnGetSRef.Click += new System.EventHandler(this.btnGetSRef_Click);
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
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = null;
            this.groupBox1.AccessibleName = null;
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.BackgroundImage = null;
            this.groupBox1.Controls.Add(this.coordControl1);
            this.groupBox1.Font = null;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // coordControl1
            // 
            this.coordControl1.AccessibleDescription = null;
            this.coordControl1.AccessibleName = null;
            resources.ApplyResources(this.coordControl1, "coordControl1");
            this.coordControl1.BackgroundImage = null;
            this.coordControl1.Font = null;
            this.coordControl1.Name = "coordControl1";
            point1.M = 0;
            point1.X = 0;
            point1.Y = 0;
            point1.Z = 0;
            this.coordControl1.Point = point1;
            this.coordControl1.SpatialReference = null;
            // 
            // FormXY
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnGetSRef);
            this.Controls.Add(this.cmbSRef);
            this.Controls.Add(this.label1);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormXY";
            this.Load += new System.EventHandler(this.FormXY_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbSRef;
        private System.Windows.Forms.Button btnGetSRef;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private CoordControl coordControl1;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}
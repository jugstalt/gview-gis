namespace gView.Framework.UI.Dialogs
{
    partial class FormFeatureclassCopy
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFeatureclassCopy));
            this.lstFeatureclasses = new System.Windows.Forms.ListView();
            this.colSourceFeatureclass = new System.Windows.Forms.ColumnHeader();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtTargetFeatureclass = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnScript = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.gvFields = new System.Windows.Forms.DataGridView();
            this.panelFormat = new System.Windows.Forms.Panel();
            this.cmbDestFormat = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvFields)).BeginInit();
            this.panelFormat.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstFeatureclasses
            // 
            this.lstFeatureclasses.CheckBoxes = true;
            this.lstFeatureclasses.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSourceFeatureclass});
            this.lstFeatureclasses.Dock = System.Windows.Forms.DockStyle.Left;
            this.lstFeatureclasses.HideSelection = false;
            this.lstFeatureclasses.Location = new System.Drawing.Point(0, 53);
            this.lstFeatureclasses.MultiSelect = false;
            this.lstFeatureclasses.Name = "lstFeatureclasses";
            this.lstFeatureclasses.Size = new System.Drawing.Size(188, 356);
            this.lstFeatureclasses.SmallImageList = this.imageList1;
            this.lstFeatureclasses.TabIndex = 2;
            this.lstFeatureclasses.UseCompatibleStateImageBehavior = false;
            this.lstFeatureclasses.View = System.Windows.Forms.View.Details;
            this.lstFeatureclasses.SelectedIndexChanged += new System.EventHandler(this.lstFeatureclasses_SelectedIndexChanged);
            // 
            // colSourceFeatureclass
            // 
            this.colSourceFeatureclass.Tag = "SourceFeatureclass";
            this.colSourceFeatureclass.Text = "Source Featureclass";
            this.colSourceFeatureclass.Width = 180;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.White;
            this.imageList1.Images.SetKeyName(0, "pointlayer.png");
            this.imageList1.Images.SetKeyName(1, "polylinelayer.png");
            this.imageList1.Images.SetKeyName(2, "polygonlayer.png");
            this.imageList1.Images.SetKeyName(3, "rasterlayer.png");
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(188, 53);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 356);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtTargetFeatureclass);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(191, 53);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(550, 33);
            this.panel1.TabIndex = 4;
            // 
            // txtTargetFeatureclass
            // 
            this.txtTargetFeatureclass.Location = new System.Drawing.Point(116, 6);
            this.txtTargetFeatureclass.Name = "txtTargetFeatureclass";
            this.txtTargetFeatureclass.Size = new System.Drawing.Size(297, 20);
            this.txtTargetFeatureclass.TabIndex = 1;
            this.txtTargetFeatureclass.TextChanged += new System.EventHandler(this.txtTargetFeatureclass_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Target Featureclass:";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 409);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(741, 51);
            this.panel2.TabIndex = 5;
            this.panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.panel2_Paint);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(12, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnScript);
            this.panel3.Controls.Add(this.btnOK);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(538, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(203, 51);
            this.panel3.TabIndex = 1;
            // 
            // btnScript
            // 
            this.btnScript.Location = new System.Drawing.Point(20, 12);
            this.btnScript.Name = "btnScript";
            this.btnScript.Size = new System.Drawing.Size(75, 23);
            this.btnScript.TabIndex = 1;
            this.btnScript.Text = "Script...";
            this.btnScript.UseVisualStyleBackColor = true;
            this.btnScript.Click += new System.EventHandler(this.btnScript_Click);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(116, 12);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // gvFields
            // 
            this.gvFields.AllowUserToAddRows = false;
            this.gvFields.AllowUserToDeleteRows = false;
            this.gvFields.AllowUserToResizeRows = false;
            this.gvFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvFields.Location = new System.Drawing.Point(191, 86);
            this.gvFields.Name = "gvFields";
            this.gvFields.ShowEditingIcon = false;
            this.gvFields.Size = new System.Drawing.Size(550, 323);
            this.gvFields.TabIndex = 6;
            // 
            // panelFormat
            // 
            this.panelFormat.Controls.Add(this.cmbDestFormat);
            this.panelFormat.Controls.Add(this.label2);
            this.panelFormat.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFormat.Location = new System.Drawing.Point(0, 0);
            this.panelFormat.Name = "panelFormat";
            this.panelFormat.Size = new System.Drawing.Size(741, 53);
            this.panelFormat.TabIndex = 7;
            this.panelFormat.Visible = false;
            // 
            // cmbDestFormat
            // 
            this.cmbDestFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDestFormat.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbDestFormat.FormattingEnabled = true;
            this.cmbDestFormat.Location = new System.Drawing.Point(102, 15);
            this.cmbDestFormat.Name = "cmbDestFormat";
            this.cmbDestFormat.Size = new System.Drawing.Size(502, 23);
            this.cmbDestFormat.Sorted = true;
            this.cmbDestFormat.TabIndex = 1;
            this.cmbDestFormat.SelectedIndexChanged += new System.EventHandler(this.cmbDestFormat_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(8, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Output Format:";
            // 
            // FormFeatureclassCopy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(741, 460);
            this.Controls.Add(this.gvFields);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.lstFeatureclasses);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panelFormat);
            this.Name = "FormFeatureclassCopy";
            this.Text = "Copy Featureclass(es)";
            this.Load += new System.EventHandler(this.FormFeatureclassCopy_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvFields)).EndInit();
            this.panelFormat.ResumeLayout(false);
            this.panelFormat.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lstFeatureclasses;
        private System.Windows.Forms.Splitter splitter1;
        public System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridView gvFields;
        private System.Windows.Forms.Button btnScript;
        private System.Windows.Forms.ColumnHeader colSourceFeatureclass;
        private System.Windows.Forms.TextBox txtTargetFeatureclass;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelFormat;
        private System.Windows.Forms.ComboBox cmbDestFormat;
        private System.Windows.Forms.Label label2;

    }
}
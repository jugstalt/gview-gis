namespace gView.Framework.UI.Dialogs
{
    partial class FormDatasetProperties
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDatasetProperties));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.dgLayers = new System.Windows.Forms.DataGridView();
            this.Column = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnLayer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnVisible = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnProperties = new System.Windows.Forms.DataGridViewButtonColumn();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgLayers)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AccessibleDescription = null;
            this.panel1.AccessibleName = null;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackgroundImage = null;
            this.panel1.Font = null;
            this.panel1.Name = "panel1";
            // 
            // panel2
            // 
            this.panel2.AccessibleDescription = null;
            this.panel2.AccessibleName = null;
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.BackgroundImage = null;
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Controls.Add(this.btnOK);
            this.panel2.Font = null;
            this.panel2.Name = "panel2";
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
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
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
            // dgLayers
            // 
            this.dgLayers.AccessibleDescription = null;
            this.dgLayers.AccessibleName = null;
            this.dgLayers.AllowUserToAddRows = false;
            this.dgLayers.AllowUserToDeleteRows = false;
            this.dgLayers.AllowUserToResizeRows = false;
            resources.ApplyResources(this.dgLayers, "dgLayers");
            this.dgLayers.BackgroundImage = null;
            this.dgLayers.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.dgLayers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgLayers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column,
            this.ColumnLayer,
            this.ColumnVisible,
            this.ColumnProperties});
            this.dgLayers.Font = null;
            this.dgLayers.Name = "dgLayers";
            this.dgLayers.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgLayers_CellClick);
            this.dgLayers.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgLayers_CellValueChanged);
            // 
            // Column
            // 
            resources.ApplyResources(this.Column, "Column");
            this.Column.Name = "Column";
            this.Column.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // ColumnLayer
            // 
            resources.ApplyResources(this.ColumnLayer, "ColumnLayer");
            this.ColumnLayer.Name = "ColumnLayer";
            this.ColumnLayer.ReadOnly = true;
            // 
            // ColumnVisible
            // 
            resources.ApplyResources(this.ColumnVisible, "ColumnVisible");
            this.ColumnVisible.Name = "ColumnVisible";
            // 
            // ColumnProperties
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            this.ColumnProperties.DefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(this.ColumnProperties, "ColumnProperties");
            this.ColumnProperties.Name = "ColumnProperties";
            this.ColumnProperties.Text = "...";
            this.ColumnProperties.UseColumnTextForButtonValue = true;
            // 
            // FormDatasetProperties
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.dgLayers);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormDatasetProperties";
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgLayers)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridView dgLayers;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLayer;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnVisible;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnProperties;
    }
}
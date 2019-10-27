namespace gView.MapServer.Lib.UI
{
    partial class FormAddServices
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAddServices));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.dgMaps = new System.Windows.Forms.DataGridView();
            this.ColumnAdd = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnMap = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnMapProps = new System.Windows.Forms.DataGridViewButtonColumn();
            this.ColumnService = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgMaps)).BeginInit();
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
            // dgMaps
            // 
            this.dgMaps.AccessibleDescription = null;
            this.dgMaps.AccessibleName = null;
            this.dgMaps.AllowUserToAddRows = false;
            this.dgMaps.AllowUserToDeleteRows = false;
            resources.ApplyResources(this.dgMaps, "dgMaps");
            this.dgMaps.BackgroundImage = null;
            this.dgMaps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgMaps.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnAdd,
            this.ColumnMap,
            this.ColumnMapProps,
            this.ColumnService});
            this.dgMaps.Font = null;
            this.dgMaps.Name = "dgMaps";
            this.dgMaps.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgMaps_CellClick);
            // 
            // ColumnAdd
            // 
            resources.ApplyResources(this.ColumnAdd, "ColumnAdd");
            this.ColumnAdd.Name = "ColumnAdd";
            // 
            // ColumnMap
            // 
            resources.ApplyResources(this.ColumnMap, "ColumnMap");
            this.ColumnMap.Name = "ColumnMap";
            this.ColumnMap.ReadOnly = true;
            // 
            // ColumnMapProps
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            this.ColumnMapProps.DefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(this.ColumnMapProps, "ColumnMapProps");
            this.ColumnMapProps.Name = "ColumnMapProps";
            this.ColumnMapProps.Text = "...";
            this.ColumnMapProps.UseColumnTextForButtonValue = true;
            // 
            // ColumnService
            // 
            resources.ApplyResources(this.ColumnService, "ColumnService");
            this.ColumnService.Name = "ColumnService";
            // 
            // FormAddServices
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.dgMaps);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormAddServices";
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgMaps)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridView dgMaps;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnAdd;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnMap;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnMapProps;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnService;
    }
}
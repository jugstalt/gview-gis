namespace gView.MapServer.Lib.UI
{
    partial class FormSelectService
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSelectService));
            this.txtPwd = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lstServices = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkRouteThroughRole = new System.Windows.Forms.CheckBox();
            this.chkRouteThroughUser = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtPwd
            // 
            this.txtPwd.AccessibleDescription = null;
            this.txtPwd.AccessibleName = null;
            resources.ApplyResources(this.txtPwd, "txtPwd");
            this.txtPwd.BackgroundImage = null;
            this.txtPwd.Font = null;
            this.txtPwd.Name = "txtPwd";
            // 
            // label4
            // 
            this.label4.AccessibleDescription = null;
            this.label4.AccessibleName = null;
            resources.ApplyResources(this.label4, "label4");
            this.label4.Font = null;
            this.label4.Name = "label4";
            // 
            // txtUser
            // 
            this.txtUser.AccessibleDescription = null;
            this.txtUser.AccessibleName = null;
            resources.ApplyResources(this.txtUser, "txtUser");
            this.txtUser.BackgroundImage = null;
            this.txtUser.Font = null;
            this.txtUser.Name = "txtUser";
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Font = null;
            this.label3.Name = "label3";
            // 
            // txtServer
            // 
            this.txtServer.AccessibleDescription = null;
            this.txtServer.AccessibleName = null;
            resources.ApplyResources(this.txtServer, "txtServer");
            this.txtServer.BackgroundImage = null;
            this.txtServer.Font = null;
            this.txtServer.Name = "txtServer";
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // btnConnect
            // 
            this.btnConnect.AccessibleDescription = null;
            this.btnConnect.AccessibleName = null;
            resources.ApplyResources(this.btnConnect, "btnConnect");
            this.btnConnect.BackgroundImage = null;
            this.btnConnect.Font = null;
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lstServices
            // 
            this.lstServices.AccessibleDescription = null;
            this.lstServices.AccessibleName = null;
            resources.ApplyResources(this.lstServices, "lstServices");
            this.lstServices.BackgroundImage = null;
            this.lstServices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lstServices.Font = null;
            this.lstServices.HideSelection = false;
            this.lstServices.MultiSelect = false;
            this.lstServices.Name = "lstServices";
            this.lstServices.SmallImageList = this.imageList1;
            this.lstServices.UseCompatibleStateImageBehavior = false;
            this.lstServices.View = System.Windows.Forms.View.Details;
            this.lstServices.SelectedIndexChanged += new System.EventHandler(this.lstServices_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "i_connection.png");
            this.imageList1.Images.SetKeyName(1, "i_connection_server.png");
            this.imageList1.Images.SetKeyName(2, "i_connection_collection.png");
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
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // numPort
            // 
            this.numPort.AccessibleDescription = null;
            this.numPort.AccessibleName = null;
            resources.ApplyResources(this.numPort, "numPort");
            this.numPort.Font = null;
            this.numPort.Maximum = new decimal(new int[] {
            -1981284353,
            -1966660860,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Value = new decimal(new int[] {
            8001,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = null;
            this.groupBox1.AccessibleName = null;
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.BackgroundImage = null;
            this.groupBox1.Controls.Add(this.chkRouteThroughRole);
            this.groupBox1.Controls.Add(this.chkRouteThroughUser);
            this.groupBox1.Controls.Add(this.txtUser);
            this.groupBox1.Controls.Add(this.txtPwd);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Font = null;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // chkRouteThroughRole
            // 
            this.chkRouteThroughRole.AccessibleDescription = null;
            this.chkRouteThroughRole.AccessibleName = null;
            resources.ApplyResources(this.chkRouteThroughRole, "chkRouteThroughRole");
            this.chkRouteThroughRole.BackgroundImage = null;
            this.chkRouteThroughRole.Font = null;
            this.chkRouteThroughRole.Name = "chkRouteThroughRole";
            this.chkRouteThroughRole.UseVisualStyleBackColor = true;
            this.chkRouteThroughRole.CheckedChanged += new System.EventHandler(this.chkRouteThroughRole_CheckedChanged);
            // 
            // chkRouteThroughUser
            // 
            this.chkRouteThroughUser.AccessibleDescription = null;
            this.chkRouteThroughUser.AccessibleName = null;
            resources.ApplyResources(this.chkRouteThroughUser, "chkRouteThroughUser");
            this.chkRouteThroughUser.BackgroundImage = null;
            this.chkRouteThroughUser.Font = null;
            this.chkRouteThroughUser.Name = "chkRouteThroughUser";
            this.chkRouteThroughUser.UseVisualStyleBackColor = true;
            this.chkRouteThroughUser.CheckedChanged += new System.EventHandler(this.chkRouteThrough_CheckedChanged);
            // 
            // FormSelectService
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.numPort);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lstServices);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.txtServer);
            this.Controls.Add(this.label1);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormSelectService";
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtPwd;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.ListView lstServices;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkRouteThroughUser;
        private System.Windows.Forms.CheckBox chkRouteThroughRole;
        private System.Windows.Forms.ImageList imageList1;
    }
}
namespace gView.DataSources.Fdb.UI.MSSql
{
    partial class FormCreateSQLFeatureDatabase
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCreateSQLFeatureDatabase));
            this.label1 = new System.Windows.Forms.Label();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.lUser = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.lPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.chkCreateConnection = new System.Windows.Forms.CheckBox();
            this.txtObject = new System.Windows.Forms.TextBox();
            this.cmbServer = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkUseFile = new System.Windows.Forms.CheckBox();
            this.txtFilename = new System.Windows.Forms.TextBox();
            this.lFilename = new System.Windows.Forms.Label();
            this.btnGetMDF = new System.Windows.Forms.Button();
            this.openMdfFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnAdvanced = new System.Windows.Forms.Button();
            this.chkCreateReplicationDatamodel = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnOnlyRepository = new System.Windows.Forms.RadioButton();
            this.btnCreateDatabase = new System.Windows.Forms.RadioButton();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cmbType
            // 
            this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            resources.GetString("cmbType.Items"),
            resources.GetString("cmbType.Items1")});
            resources.ApplyResources(this.cmbType, "cmbType");
            this.cmbType.Name = "cmbType";
            this.cmbType.SelectedIndexChanged += new System.EventHandler(this.cmbType_SelectedIndexChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // txtDatabase
            // 
            resources.ApplyResources(this.txtDatabase, "txtDatabase");
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.TextChanged += new System.EventHandler(this.txtDatabase_TextChanged);
            // 
            // lUser
            // 
            resources.ApplyResources(this.lUser, "lUser");
            this.lUser.Name = "lUser";
            // 
            // txtUser
            // 
            resources.ApplyResources(this.txtUser, "txtUser");
            this.txtUser.Name = "txtUser";
            // 
            // lPassword
            // 
            resources.ApplyResources(this.lPassword, "lPassword");
            this.lPassword.Name = "lPassword";
            // 
            // txtPassword
            // 
            resources.ApplyResources(this.txtPassword, "txtPassword");
            this.txtPassword.Name = "txtPassword";
            // 
            // chkCreateConnection
            // 
            resources.ApplyResources(this.chkCreateConnection, "chkCreateConnection");
            this.chkCreateConnection.Checked = true;
            this.chkCreateConnection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCreateConnection.Name = "chkCreateConnection";
            this.chkCreateConnection.TabStop = false;
            this.chkCreateConnection.UseVisualStyleBackColor = true;
            this.chkCreateConnection.CheckedChanged += new System.EventHandler(this.chkCreateConnection_CheckedChanged);
            // 
            // txtObject
            // 
            resources.ApplyResources(this.txtObject, "txtObject");
            this.txtObject.Name = "txtObject";
            // 
            // cmbServer
            // 
            this.cmbServer.FormattingEnabled = true;
            resources.ApplyResources(this.cmbServer, "cmbServer");
            this.cmbServer.Name = "cmbServer";
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // chkUseFile
            // 
            resources.ApplyResources(this.chkUseFile, "chkUseFile");
            this.chkUseFile.Name = "chkUseFile";
            this.chkUseFile.TabStop = false;
            this.chkUseFile.UseVisualStyleBackColor = true;
            this.chkUseFile.CheckedChanged += new System.EventHandler(this.chkUseFile_CheckedChanged);
            // 
            // txtFilename
            // 
            resources.ApplyResources(this.txtFilename, "txtFilename");
            this.txtFilename.Name = "txtFilename";
            this.txtFilename.TextChanged += new System.EventHandler(this.txtFilename_TextChanged);
            // 
            // lFilename
            // 
            resources.ApplyResources(this.lFilename, "lFilename");
            this.lFilename.Name = "lFilename";
            // 
            // btnGetMDF
            // 
            resources.ApplyResources(this.btnGetMDF, "btnGetMDF");
            this.btnGetMDF.Name = "btnGetMDF";
            this.btnGetMDF.UseVisualStyleBackColor = true;
            this.btnGetMDF.Click += new System.EventHandler(this.btnGetMDF_Click);
            // 
            // openMdfFileDialog
            // 
            resources.ApplyResources(this.openMdfFileDialog, "openMdfFileDialog");
            // 
            // btnAdvanced
            // 
            resources.ApplyResources(this.btnAdvanced, "btnAdvanced");
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.UseVisualStyleBackColor = true;
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
            // 
            // chkCreateReplicationDatamodel
            // 
            resources.ApplyResources(this.chkCreateReplicationDatamodel, "chkCreateReplicationDatamodel");
            this.chkCreateReplicationDatamodel.Checked = true;
            this.chkCreateReplicationDatamodel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCreateReplicationDatamodel.Name = "chkCreateReplicationDatamodel";
            this.chkCreateReplicationDatamodel.TabStop = false;
            this.chkCreateReplicationDatamodel.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.pictureBox1);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::gView.DataSources.Fdb.UI.Properties.Resources.db_connect;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // btnOnlyRepository
            // 
            resources.ApplyResources(this.btnOnlyRepository, "btnOnlyRepository");
            this.btnOnlyRepository.Name = "btnOnlyRepository";
            this.btnOnlyRepository.UseVisualStyleBackColor = true;
            // 
            // btnCreateDatabase
            // 
            resources.ApplyResources(this.btnCreateDatabase, "btnCreateDatabase");
            this.btnCreateDatabase.Checked = true;
            this.btnCreateDatabase.Name = "btnCreateDatabase";
            this.btnCreateDatabase.TabStop = true;
            this.btnCreateDatabase.UseVisualStyleBackColor = true;
            // 
            // FormCreateSQLFeatureDatabase
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnOnlyRepository);
            this.Controls.Add(this.btnCreateDatabase);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.chkCreateReplicationDatamodel);
            this.Controls.Add(this.btnAdvanced);
            this.Controls.Add(this.btnGetMDF);
            this.Controls.Add(this.lFilename);
            this.Controls.Add(this.txtFilename);
            this.Controls.Add(this.chkUseFile);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cmbServer);
            this.Controls.Add(this.txtObject);
            this.Controls.Add(this.chkCreateConnection);
            this.Controls.Add(this.lPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lUser);
            this.Controls.Add(this.txtUser);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtDatabase);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbType);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormCreateSQLFeatureDatabase";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.Label lUser;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label lPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.CheckBox chkCreateConnection;
        private System.Windows.Forms.TextBox txtObject;
        private System.Windows.Forms.ComboBox cmbServer;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkUseFile;
        private System.Windows.Forms.TextBox txtFilename;
        private System.Windows.Forms.Label lFilename;
        private System.Windows.Forms.Button btnGetMDF;
        private System.Windows.Forms.OpenFileDialog openMdfFileDialog;
        private System.Windows.Forms.Button btnAdvanced;
        private System.Windows.Forms.CheckBox chkCreateReplicationDatamodel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.RadioButton btnOnlyRepository;
        private System.Windows.Forms.RadioButton btnCreateDatabase;
    }
}
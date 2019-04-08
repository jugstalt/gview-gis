namespace gView.Interoperability.OGC.UI.Dataset.WMS
{
    partial class FormNewConnection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNewConnection));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtWMSUrl = new System.Windows.Forms.TextBox();
            this.txtServiceName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnTestWMS = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbSERVICE = new System.Windows.Forms.ComboBox();
            this.txtWFSUrl = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnTestWFS = new System.Windows.Forms.Button();
            this.xmlViewTree1 = new gView.Framework.UI.Dialogs.XmlViewTree();
            this.txtPwd = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.panel2);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnOK);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // txtWMSUrl
            // 
            resources.ApplyResources(this.txtWMSUrl, "txtWMSUrl");
            this.txtWMSUrl.Name = "txtWMSUrl";
            // 
            // txtServiceName
            // 
            resources.ApplyResources(this.txtServiceName, "txtServiceName");
            this.txtServiceName.Name = "txtServiceName";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // btnTestWMS
            // 
            resources.ApplyResources(this.btnTestWMS, "btnTestWMS");
            this.btnTestWMS.Name = "btnTestWMS";
            this.btnTestWMS.UseVisualStyleBackColor = true;
            this.btnTestWMS.Click += new System.EventHandler(this.btnTestWMS_Click);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // cmbSERVICE
            // 
            this.cmbSERVICE.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSERVICE.FormattingEnabled = true;
            this.cmbSERVICE.Items.AddRange(new object[] {
            resources.GetString("cmbSERVICE.Items"),
            resources.GetString("cmbSERVICE.Items1"),
            resources.GetString("cmbSERVICE.Items2")});
            resources.ApplyResources(this.cmbSERVICE, "cmbSERVICE");
            this.cmbSERVICE.Name = "cmbSERVICE";
            this.cmbSERVICE.SelectedIndexChanged += new System.EventHandler(this.cmbSERVICE_SelectedIndexChanged);
            // 
            // txtWFSUrl
            // 
            resources.ApplyResources(this.txtWFSUrl, "txtWFSUrl");
            this.txtWFSUrl.Name = "txtWFSUrl";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // btnTestWFS
            // 
            resources.ApplyResources(this.btnTestWFS, "btnTestWFS");
            this.btnTestWFS.Name = "btnTestWFS";
            this.btnTestWFS.UseVisualStyleBackColor = true;
            this.btnTestWFS.Click += new System.EventHandler(this.btnTestWFS_Click);
            // 
            // xmlViewTree1
            // 
            resources.ApplyResources(this.xmlViewTree1, "xmlViewTree1");
            this.xmlViewTree1.Name = "xmlViewTree1";
            this.xmlViewTree1.ShowAttributes = true;
            this.xmlViewTree1.XmlDocument = null;
            // 
            // txtPwd
            // 
            resources.ApplyResources(this.txtPwd, "txtPwd");
            this.txtPwd.Name = "txtPwd";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // txtUser
            // 
            resources.ApplyResources(this.txtUser, "txtUser");
            this.txtUser.Name = "txtUser";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // FormNewConnection
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtPwd);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtUser);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.xmlViewTree1);
            this.Controls.Add(this.btnTestWFS);
            this.Controls.Add(this.txtWFSUrl);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbSERVICE);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnTestWMS);
            this.Controls.Add(this.txtServiceName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtWMSUrl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormNewConnection";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtWMSUrl;
        private System.Windows.Forms.TextBox txtServiceName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnTestWMS;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbSERVICE;
        private System.Windows.Forms.TextBox txtWFSUrl;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnTestWFS;
        private gView.Framework.UI.Dialogs.XmlViewTree xmlViewTree1;
        private System.Windows.Forms.TextBox txtPwd;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label label6;
    }
}
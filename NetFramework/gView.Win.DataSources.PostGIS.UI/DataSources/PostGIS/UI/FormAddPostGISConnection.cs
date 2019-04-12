using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using gView.Framework.IO;
using System.Threading.Tasks;

namespace gView.DataSources.PostGIS.UI
{
	/// <summary>
	/// Zusammenfassung für FormAddSqlFDBDataset.
	/// </summary>
	internal class FormAddPostGISConnection : System.Windows.Forms.Form
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
        private TextBox txtPort;
        private Label label5;
		private string _connStr="";

        public FormAddPostGISConnection()
		{
			//
			// Erforderlich für die Windows Form-Designerunterstützung
			//
			InitializeComponent();
		}

		/// <summary>
		/// Die verwendeten Ressourcen bereinigen.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Vom Windows Form-Designer generierter Code
		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAddPostGISConnection));
            this.label1 = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.txtDataset = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPwd = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
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
            // txtServer
            // 
            this.txtServer.AccessibleDescription = null;
            this.txtServer.AccessibleName = null;
            resources.ApplyResources(this.txtServer, "txtServer");
            this.txtServer.BackgroundImage = null;
            this.txtServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer.Font = null;
            this.txtServer.Name = "txtServer";
            // 
            // txtDataset
            // 
            this.txtDataset.AccessibleDescription = null;
            this.txtDataset.AccessibleName = null;
            resources.ApplyResources(this.txtDataset, "txtDataset");
            this.txtDataset.BackgroundImage = null;
            this.txtDataset.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDataset.Font = null;
            this.txtDataset.Name = "txtDataset";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // txtUser
            // 
            this.txtUser.AccessibleDescription = null;
            this.txtUser.AccessibleName = null;
            resources.ApplyResources(this.txtUser, "txtUser");
            this.txtUser.BackgroundImage = null;
            this.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
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
            // txtPwd
            // 
            this.txtPwd.AccessibleDescription = null;
            this.txtPwd.AccessibleName = null;
            resources.ApplyResources(this.txtPwd, "txtPwd");
            this.txtPwd.BackgroundImage = null;
            this.txtPwd.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
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
            // button1
            // 
            this.button1.AccessibleDescription = null;
            this.button1.AccessibleName = null;
            resources.ApplyResources(this.button1, "button1");
            this.button1.BackgroundImage = null;
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Font = null;
            this.button1.Name = "button1";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.AccessibleDescription = null;
            this.button2.AccessibleName = null;
            resources.ApplyResources(this.button2, "button2");
            this.button2.BackgroundImage = null;
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Font = null;
            this.button2.Name = "button2";
            // 
            // txtPort
            // 
            this.txtPort.AccessibleDescription = null;
            this.txtPort.AccessibleName = null;
            resources.ApplyResources(this.txtPort, "txtPort");
            this.txtPort.BackgroundImage = null;
            this.txtPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPort.Font = null;
            this.txtPort.Name = "txtPort";
            // 
            // label5
            // 
            this.label5.AccessibleDescription = null;
            this.label5.AccessibleName = null;
            resources.ApplyResources(this.label5, "label5");
            this.label5.Font = null;
            this.label5.Name = "label5";
            // 
            // FormAddPostGISConnection
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.BackgroundImage = null;
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtPwd);
            this.Controls.Add(this.txtUser);
            this.Controls.Add(this.txtDataset);
            this.Controls.Add(this.txtServer);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormAddPostGISConnection";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtServer;
		private System.Windows.Forms.TextBox txtDataset;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtUser;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtPwd;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;

		#region IDatasetEnum Member

		int _pos=0;
		public void Reset()
		{
			int _pos=0;
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			_connStr="Server="+txtServer.Text+";Port="+txtPort.Text+";User Id="+txtUser.Text+";Password="+txtPwd.Text+";Database="+txtDataset.Text;
		}


		private gView.Framework.Data.IDataset _dataset=null;
        async public Task<gView.Framework.Data.IDataset> Next()
        {
            if (_pos == 0)
            {
                _pos++;
                gView.DataSources.PostGIS.PostGISDataset dataset = new gView.DataSources.PostGIS.PostGISDataset();
                await dataset.SetConnectionString("Server=" + txtServer.Text + ";Port=" + txtPort.Text + ";User Id=" + txtUser.Text + ";Password=" + txtPwd.Text + ";Database=" + txtDataset.Text);
                await dataset.Open();

                // Dataset in _dataset zwischenspeichern, damit referenz bestehen bleibt
                // Dispose ist danach vom Benutzer auszuführen wenn das Objekt nicht mehr verwendet wird.
                // Ansonsten kann es vorkommen, dass der Garbagecolletor das Dispose ausführt, wenn
                // ein Anwendung ein dataset abholt, Geoprocessing mit den Layern durchführt und danach
                // das Dataset Objekt nicht mehr abholt.
                // Beim Dispose werden dann näml. auch die Datenbankverbingen (wärend eines Geoprocessings)
                // abgeschnitten !!!
                return _dataset = dataset;
            }
            return null;
        }

		#endregion

		public string ConnectionString 
		{
			get 
			{
				return _connStr;
			}
			set 
			{
				_connStr=value;
				txtServer.Text =ConfigTextStream.ExtractValue(value,"server");
				txtDataset.Text=ConfigTextStream.ExtractValue(value,"database");
				txtUser.Text   =ConfigTextStream.ExtractValue(value,"uid");
				txtPwd.Text    =ConfigTextStream.ExtractValue(value,"pwd");
			}
		}

	}
}

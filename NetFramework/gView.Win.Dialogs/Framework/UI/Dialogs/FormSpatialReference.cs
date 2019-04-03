using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using gView.Framework;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.UI.Controls.Filter;
using System.IO;
using gView.Framework.UI.Dialogs.Properties;
using gView.Framework.Proj;

namespace gView.Framework.UI.Dialogs
{
	/// <summary>
	/// Zusammenfassung für FormSpatialReference.
	/// </summary>
	public class FormSpatialReference : System.Windows.Forms.Form
    {
		private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnOK;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
        private System.ComponentModel.Container components = null;
        private GroupBox GroupBox1;
        private Button button1;
        private Label label1;
        private TextBox txtDescription;
        private Label label2;
        private TextBox txtID;
        private Button btnGetSys;
        private GroupBox GroupBox2;
        private Button button2;
        private Label label3;
        private TextBox txtDatumID;
        private Button btnGetDatum;
        public Panel panelReferenceSystem;
        private Button btnImport;
        private Button btnExport;
        private SaveFileDialog exportDialog;
		private ISpatialReference _sRef=null;

		public FormSpatialReference(ISpatialReference sRef)
		{
			//
			// Erforderlich für die Windows Form-Designerunterstützung
			//
			InitializeComponent();

			_sRef=sRef;
			MakeGUI();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSpatialReference));
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtID = new System.Windows.Forms.TextBox();
            this.btnGetSys = new System.Windows.Forms.Button();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDatumID = new System.Windows.Forms.TextBox();
            this.btnGetDatum = new System.Windows.Forms.Button();
            this.panelReferenceSystem = new System.Windows.Forms.Panel();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.exportDialog = new System.Windows.Forms.SaveFileDialog();
            this.panel2.SuspendLayout();
            this.GroupBox1.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.panelReferenceSystem.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.AccessibleDescription = null;
            this.panel2.AccessibleName = null;
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.BackgroundImage = null;
            this.panel2.Controls.Add(this.btnOK);
            this.panel2.Font = null;
            this.panel2.Name = "panel2";
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
            // 
            // GroupBox1
            // 
            this.GroupBox1.AccessibleDescription = null;
            this.GroupBox1.AccessibleName = null;
            resources.ApplyResources(this.GroupBox1, "GroupBox1");
            this.GroupBox1.BackgroundImage = null;
            this.GroupBox1.Controls.Add(this.button1);
            this.GroupBox1.Controls.Add(this.label1);
            this.GroupBox1.Controls.Add(this.txtDescription);
            this.GroupBox1.Controls.Add(this.label2);
            this.GroupBox1.Controls.Add(this.txtID);
            this.GroupBox1.Controls.Add(this.btnGetSys);
            this.GroupBox1.Font = null;
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.AccessibleDescription = null;
            this.button1.AccessibleName = null;
            resources.ApplyResources(this.button1, "button1");
            this.button1.BackgroundImage = null;
            this.button1.Font = null;
            this.button1.Name = "button1";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // txtDescription
            // 
            this.txtDescription.AccessibleDescription = null;
            this.txtDescription.AccessibleName = null;
            resources.ApplyResources(this.txtDescription, "txtDescription");
            this.txtDescription.BackgroundImage = null;
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescription.Font = null;
            this.txtDescription.Name = "txtDescription";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // txtID
            // 
            this.txtID.AccessibleDescription = null;
            this.txtID.AccessibleName = null;
            resources.ApplyResources(this.txtID, "txtID");
            this.txtID.BackgroundImage = null;
            this.txtID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtID.Font = null;
            this.txtID.Name = "txtID";
            // 
            // btnGetSys
            // 
            this.btnGetSys.AccessibleDescription = null;
            this.btnGetSys.AccessibleName = null;
            resources.ApplyResources(this.btnGetSys, "btnGetSys");
            this.btnGetSys.BackgroundImage = null;
            this.btnGetSys.Font = null;
            this.btnGetSys.Name = "btnGetSys";
            this.btnGetSys.Click += new System.EventHandler(this.btnGetSys_Click);
            // 
            // GroupBox2
            // 
            this.GroupBox2.AccessibleDescription = null;
            this.GroupBox2.AccessibleName = null;
            resources.ApplyResources(this.GroupBox2, "GroupBox2");
            this.GroupBox2.BackgroundImage = null;
            this.GroupBox2.Controls.Add(this.button2);
            this.GroupBox2.Controls.Add(this.label3);
            this.GroupBox2.Controls.Add(this.txtDatumID);
            this.GroupBox2.Controls.Add(this.btnGetDatum);
            this.GroupBox2.Font = null;
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.TabStop = false;
            // 
            // button2
            // 
            this.button2.AccessibleDescription = null;
            this.button2.AccessibleName = null;
            resources.ApplyResources(this.button2, "button2");
            this.button2.BackgroundImage = null;
            this.button2.Font = null;
            this.button2.Name = "button2";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Font = null;
            this.label3.Name = "label3";
            // 
            // txtDatumID
            // 
            this.txtDatumID.AccessibleDescription = null;
            this.txtDatumID.AccessibleName = null;
            resources.ApplyResources(this.txtDatumID, "txtDatumID");
            this.txtDatumID.BackgroundImage = null;
            this.txtDatumID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDatumID.Font = null;
            this.txtDatumID.Name = "txtDatumID";
            // 
            // btnGetDatum
            // 
            this.btnGetDatum.AccessibleDescription = null;
            this.btnGetDatum.AccessibleName = null;
            resources.ApplyResources(this.btnGetDatum, "btnGetDatum");
            this.btnGetDatum.BackgroundImage = null;
            this.btnGetDatum.Font = null;
            this.btnGetDatum.Name = "btnGetDatum";
            this.btnGetDatum.Click += new System.EventHandler(this.btnGetDatum_Click);
            // 
            // panelReferenceSystem
            // 
            this.panelReferenceSystem.AccessibleDescription = null;
            this.panelReferenceSystem.AccessibleName = null;
            resources.ApplyResources(this.panelReferenceSystem, "panelReferenceSystem");
            this.panelReferenceSystem.BackgroundImage = null;
            this.panelReferenceSystem.Controls.Add(this.btnExport);
            this.panelReferenceSystem.Controls.Add(this.btnImport);
            this.panelReferenceSystem.Controls.Add(this.GroupBox2);
            this.panelReferenceSystem.Controls.Add(this.GroupBox1);
            this.panelReferenceSystem.Font = null;
            this.panelReferenceSystem.Name = "panelReferenceSystem";
            // 
            // btnExport
            // 
            this.btnExport.AccessibleDescription = null;
            this.btnExport.AccessibleName = null;
            resources.ApplyResources(this.btnExport, "btnExport");
            this.btnExport.BackgroundImage = null;
            this.btnExport.Font = null;
            this.btnExport.Name = "btnExport";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnImport
            // 
            this.btnImport.AccessibleDescription = null;
            this.btnImport.AccessibleName = null;
            resources.ApplyResources(this.btnImport, "btnImport");
            this.btnImport.BackgroundImage = null;
            this.btnImport.Font = null;
            this.btnImport.Name = "btnImport";
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // exportDialog
            // 
            resources.ApplyResources(this.exportDialog, "exportDialog");
            // 
            // FormSpatialReference
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.BackgroundImage = null;
            this.Controls.Add(this.panelReferenceSystem);
            this.Controls.Add(this.panel2);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = null;
            this.Name = "FormSpatialReference";
            this.panel2.ResumeLayout(false);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
            this.GroupBox2.ResumeLayout(false);
            this.GroupBox2.PerformLayout();
            this.panelReferenceSystem.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		public ISpatialReference SpatialReference  
		{
			get { return _sRef; }
			set { _sRef=value; }
		}

		private void btnGetSys_Click(object sender, System.EventArgs e)
		{
			FormSpatialReferenceSystems dlg=new FormSpatialReferenceSystems(ProjDBTables.projs);
			if(dlg.ShowDialog()==DialogResult.OK) 
			{
				if(dlg.SpatialRefererence!=null) 
				{
					IGeodeticDatum datum=null;
					if(_sRef!=null) 
					{
						datum=_sRef.Datum;
					}
					_sRef=dlg.SpatialRefererence;
					if(datum!=null) _sRef.Datum=datum;

					MakeGUI();
				}
			}
		}

		private void btnGetDatum_Click(object sender, System.EventArgs e)
		{
			if(_sRef==null) return; 

			FormSpatialReferenceSystems dlg=new FormSpatialReferenceSystems(ProjDBTables.datums);
			if(dlg.ShowDialog()==DialogResult.OK) 
			{
				if(dlg.GeodeticDatum!=null) 
				{
					_sRef.Datum=dlg.GeodeticDatum;
				}

				MakeGUI();
			}
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			if(_sRef==null) return;

			FormPropertyGrid dlg=new FormPropertyGrid(
				new SpatialReferenceProperties((gView.Framework.Geometry.SpatialReference)_sRef));

			if(dlg.ShowDialog()==DialogResult.OK) 
			{
				MakeGUI();
			}
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			if(_sRef==null) return;

			FormPropertyGrid dlg=new FormPropertyGrid(
				new GeodeticDatumProperties((GeodeticDatum)_sRef.Datum));

			if(dlg.ShowDialog()==DialogResult.OK) 
			{
				_sRef.Datum=(IGeodeticDatum)dlg.SelectedObject;
				MakeGUI();
			}
		}

		public bool canModify 
		{
            set
            {
                btnGetSys.Enabled = btnGetDatum.Enabled =
                    button1.Enabled = button2.Enabled =
                    btnImport.Enabled = value;
            }
		}
		public void RefreshGUI() 
		{
			MakeGUI();
		}
		private void MakeGUI() 
		{
			if(_sRef!=null) 
				txtID.Text=_sRef.Name;
			else
				txtID.Text="";

			txtDescription.Lines=this.referenceDescription;
			txtDatumID.Lines=this.datumDescription;
		}

		private string [] datumDescription 
		{
			get 
			{
				if(_sRef==null) return "".Split(' ');
				if(_sRef.Datum==null) return "".Split(' ');

				string descr=((GeodeticDatum)_sRef.Datum).Name+"|";
				descr+="|X Axis: "+_sRef.Datum.X_Axis.ToString();
				descr+="|Y Axis: "+_sRef.Datum.Y_Axis.ToString();
				descr+="|Z Axis: "+_sRef.Datum.Z_Axis.ToString();
				descr+="|X Rotation: "+_sRef.Datum.X_Rotation.ToString();
				descr+="|Y Rotation: "+_sRef.Datum.Y_Rotation.ToString();
				descr+="|Z Rotation: "+_sRef.Datum.Z_Rotation.ToString();
				descr+="|Scale Difference: "+_sRef.Datum.Scale_Diff.ToString();

				return descr.Split('|');
			}
		}

		private string [] referenceDescription
		{
			get 
			{
				if(_sRef==null) return "".Split(' ');
				if(_sRef.Parameters==null) return "".Split(' ');

				string descr=_sRef.Description+"|";
				foreach(string parameter in _sRef.Parameters) 
				{
					descr+="|"+parameter;
				}
				return descr.Split('|');
			}
		}

        private void btnImport_Click(object sender, EventArgs e)
        {
            List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
            filters.Add(new OpenDataFilter());
            filters.Add(new OpenSpatialReferenceFilter());

            ExplorerDialog dlg = new ExplorerDialog("Import Spatial Reference", filters, true);
            dlg.MulitSelection = false;

            if (dlg.ShowDialog() == DialogResult.OK && dlg.ExplorerObjects.Count==1)
            {
                IExplorerObject exObject = dlg.ExplorerObjects[0];

                if (exObject.Object is IFeatureDataset)
                {
                    _sRef = ((IFeatureDataset)exObject.Object).SpatialReference;
                }
                else if (exObject.Object is IRasterDataset)
                {
                    _sRef = ((IRasterDataset)exObject.Object).SpatialReference;
                }
                else if (exObject.Object is IFeatureClass)
                {
                    _sRef = ((IFeatureClass)exObject.Object).SpatialReference;
                }
                else if (exObject.Object is IRasterClass)
                {
                    _sRef = ((IRasterClass)exObject.Object).SpatialReference;
                }
                else if (exObject.Object is IWebServiceClass)
                {
                    _sRef = ((IWebServiceClass)exObject.Object).SpatialReference;
                }
                else if (exObject.Object is ISpatialReference)
                {
                    _sRef = exObject.Object as ISpatialReference;
                }
                else
                {
                    MessageBox.Show("Can't import spatial reference from data object...");
                    return;
                }

                MakeGUI();
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (SpatialReference == null) return;

            if (exportDialog.ShowDialog() == DialogResult.OK)
            {
                string txt = "";
                switch (exportDialog.FilterIndex)
                {
                    case 1:
                        txt = gView.Framework.Geometry.SpatialReference.ToWKT(SpatialReference);
                        break;
                    case 2:
                        txt = gView.Framework.Geometry.SpatialReference.ToESRIWKT(SpatialReference);
                        break;
                }

                StreamWriter sw = new StreamWriter(exportDialog.FileName, false, System.Text.Encoding.ASCII);
                sw.WriteLine(txt);
                sw.Close();
            }
        }
	}
}

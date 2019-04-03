using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Xml;

namespace gView.Framework.UI.Dialogs
{
	/// <summary>
	/// Zusammenfassung für FormComponents.
	/// </summary>
	public class FormComponents : System.Windows.Forms.Form, IControl
	{
		private System.Windows.Forms.ListBox lstComponents;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.TreeView treeView1;
        private ImageList imageList1;
        private ImageList imageList2;
        private IContainer components;

		public FormComponents()
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormComponents));
            this.lstComponents = new System.Windows.Forms.ListBox();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // lstComponents
            // 
            this.lstComponents.AccessibleDescription = null;
            this.lstComponents.AccessibleName = null;
            resources.ApplyResources(this.lstComponents, "lstComponents");
            this.lstComponents.BackgroundImage = null;
            this.lstComponents.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstComponents.Font = null;
            this.lstComponents.Name = "lstComponents";
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1});
            resources.ApplyResources(this.mainMenu1, "mainMenu1");
            // 
            // menuItem1
            // 
            resources.ApplyResources(this.menuItem1, "menuItem1");
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2,
            this.menuItem3});
            // 
            // menuItem2
            // 
            resources.ApplyResources(this.menuItem2, "menuItem2");
            this.menuItem2.Index = 0;
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // menuItem3
            // 
            resources.ApplyResources(this.menuItem3, "menuItem3");
            this.menuItem3.Index = 1;
            this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
            // 
            // openFileDialog1
            // 
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // splitter1
            // 
            this.splitter1.AccessibleDescription = null;
            this.splitter1.AccessibleName = null;
            resources.ApplyResources(this.splitter1, "splitter1");
            this.splitter1.BackgroundImage = null;
            this.splitter1.Font = null;
            this.splitter1.Name = "splitter1";
            this.splitter1.TabStop = false;
            // 
            // treeView1
            // 
            this.treeView1.AccessibleDescription = null;
            this.treeView1.AccessibleName = null;
            resources.ApplyResources(this.treeView1, "treeView1");
            this.treeView1.BackgroundImage = null;
            this.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeView1.Font = null;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Name = "treeView1";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "tag.png");
            this.imageList1.Images.SetKeyName(1, "16-tool-a.png");
            // 
            // imageList2
            // 
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList2.Images.SetKeyName(0, "brick.png");
            // 
            // FormComponents
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.BackgroundImage = null;
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.lstComponents);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = null;
            this.Menu = this.mainMenu1;
            this.Name = "FormComponents";
            this.Load += new System.EventHandler(this.FormComponents_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			if(openFileDialog1.ShowDialog(this)==DialogResult.OK) 
			{
				gView.Framework.system.UI.AssemblyExplorer explorer=new gView.Framework.system.UI.AssemblyExplorer();
				if(explorer.Explore(openFileDialog1.FileName)=="") 
				{
					MessageBox.Show("No Components found!");
					return;
				}
				if(lstComponents.Items.IndexOf(openFileDialog1.FileName)==-1) 
				{
					addComponent(openFileDialog1.FileName);
					lstComponents.Items.Add(openFileDialog1.FileName);
				}
				
				writeComponents();
				makeTree();
			}
		}

		private void writeComponents() 
		{
			gView.Framework.system.UI.AssemblyExplorer explorer=new gView.Framework.system.UI.AssemblyExplorer();

			// Refresh Components
			string str="";
			foreach(string assemblyname in lstComponents.Items) 
			{
				str+=explorer.Explore(assemblyname)+"\n";
			}

            string filename = gView.Framework.system.SystemVariables.MyCommonApplicationData + @"\gViewGisOS_plugins.xml";
			StreamWriter sw=new StreamWriter(filename); 
			sw.WriteLine("<components>\n"+str+"</components>");
			sw.Close();

			RegistryKey key=Registry.LocalMachine.OpenSubKey(@"Software\gViewGisOS",true);
			if(key!=null) 
			{
				key.SetValue("comp",filename);
				key.Close();
			}
		}
		private void fillList() 
		{
			lstComponents.Items.Clear();

			RegistryKey key=Registry.LocalMachine.OpenSubKey(@"Software\gViewGisOS\assemblies",false);
			if(key==null) return;

			foreach(string assembly in key.GetValueNames()) 
			{
				if(assembly.IndexOf("assembly_")!=0) continue;

				lstComponents.Items.Add(key.GetValue(assembly).ToString());
			}

			writeComponents();
		}

		private void makeTree() 
		{
			treeView1.Nodes.Clear();

			try 
			{
				RegistryKey key=Registry.LocalMachine.OpenSubKey(@"Software\gViewGisOS",false);
				if(key==null) return;
				string filename=key.GetValue("comp").ToString();

				XmlDocument doc=new XmlDocument();
				doc.Load(filename);

				foreach(XmlNode nodename in doc.ChildNodes[0].ChildNodes) 
				{
					TreeNode parent=null;
					foreach(TreeNode node in treeView1.Nodes) 
					{
						if(node.Text==nodename.Name) 
						{
							parent=node;
							break;
						}
					}
					if(parent==null) 
					{
						parent=new TreeNode(nodename.Name,0,0);
						treeView1.Nodes.Add(parent);
					}

					TreeNode nNode=new TreeNode(nodename.Attributes["fullname"].Value+"             ("+
						nodename.Attributes["assembly"].Value+")",1,1);
					parent.Nodes.Add(nNode);
				}
			} 
			catch 
			{
				return;
			}
		}

		private void addComponent(string assemblyPath) 
		{
			removeComponent(assemblyPath);

			Registry.LocalMachine.CreateSubKey(@"Software\gViewGisOS\assemblies");
			RegistryKey key=Registry.LocalMachine.OpenSubKey(@"Software\gViewGisOS\assemblies",true);

			int num=1;
			while(true) 
			{
				string assembly="assembly_"+num.ToString();

				bool found=false;
				foreach(string valuename in key.GetValueNames()) 
				{
					if(valuename==assembly) 
					{
						found=true;
						break;
					}
				}
				if(!found) 
				{
					key.SetValue(assembly,assemblyPath);
					break;
				}
				num++;
			}
			key.Close();
		}

		private void removeComponent(string assemblyPath) 
		{
			RegistryKey key=Registry.LocalMachine.OpenSubKey(@"Software\gViewGisOS\assemblies",true);
			if(key==null) return;

			foreach(string valuename in key.GetValueNames()) 
			{
				if(valuename.IndexOf("assembly_")==0) 
				{
					if(key.GetValue(valuename).ToString().ToLower()==assemblyPath.ToLower())
					{
						key.DeleteValue(valuename);
					}
				}
			}
			key.Close();
		}

		private void menuItem3_Click(object sender, System.EventArgs e)
		{
			if(lstComponents.SelectedItem==null) return;
			removeComponent(lstComponents.SelectedItem.ToString());
			fillList();
			makeTree();
		}

		private void FormComponents_Load(object sender, System.EventArgs e)
		{
			fillList();
			makeTree();
		}

        #region IControl Member

        public void OnShowControl(object hook)
        {
            FormComponents_Load(this, new EventArgs());
        }

        public void UnloadControl() { }

        #endregion
    }
}

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using gView.Framework;
using gView.Framework.Symbology;
using gView.Framework.UI;

namespace gView.Framework.Symbology.UI
{
	/// <summary>
	/// Zusammenfassung für PropertyForm_SimpleLineSymbol.
	/// </summary>
	internal class PropertyForm_SimpleLineSymbol : System.Windows.Forms.Form,IPropertyPageUI,gView.Framework.Symbology.UI.IPropertyPanel
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.PropertyGrid propertyGrid;
		public System.Windows.Forms.Panel panelLineSymbol;
		private SimpleLineSymbol _symbol;

		public PropertyForm_SimpleLineSymbol()
		{
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyForm_SimpleLineSymbol));
            this.panelLineSymbol = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.panelLineSymbol.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelLineSymbol
            // 
            this.panelLineSymbol.AccessibleDescription = null;
            this.panelLineSymbol.AccessibleName = null;
            resources.ApplyResources(this.panelLineSymbol, "panelLineSymbol");
            this.panelLineSymbol.BackgroundImage = null;
            this.panelLineSymbol.Controls.Add(this.tabControl1);
            this.panelLineSymbol.Font = null;
            this.panelLineSymbol.Name = "panelLineSymbol";
            // 
            // tabControl1
            // 
            this.tabControl1.AccessibleDescription = null;
            this.tabControl1.AccessibleName = null;
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.BackgroundImage = null;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Font = null;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.AccessibleDescription = null;
            this.tabPage1.AccessibleName = null;
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.BackgroundImage = null;
            this.tabPage1.Controls.Add(this.propertyGrid);
            this.tabPage1.Font = null;
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // propertyGrid
            // 
            this.propertyGrid.AccessibleDescription = null;
            this.propertyGrid.AccessibleName = null;
            resources.ApplyResources(this.propertyGrid, "propertyGrid");
            this.propertyGrid.BackgroundImage = null;
            this.propertyGrid.Font = null;
            this.propertyGrid.HelpForeColor = System.Drawing.SystemColors.Control;
            this.propertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // PropertyForm_SimpleLineSymbol
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.BackgroundImage = null;
            this.Controls.Add(this.panelLineSymbol);
            this.Font = null;
            this.Icon = null;
            this.Name = "PropertyForm_SimpleLineSymbol";
            this.Load += new System.EventHandler(this.PropertyForm_SimpleLineSymbol_Load);
            this.panelLineSymbol.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void PropertyForm_SimpleLineSymbol_Load(object sender, System.EventArgs e)
		{
			
		}

		private void propertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
		{
			if(PropertyChanged!=null) PropertyChanged(_symbol);
		}

		#region IPropertryPageUI
		public event PropertyChangedEvent PropertyChanged=null;
		#endregion

        #region IPropertyPanel Member

        public object PropertyPanel(ISymbol symbol)
        {
            if (symbol is SimpleLineSymbol)
            {
                _symbol = (SimpleLineSymbol)symbol;
            }

            if (_symbol == null) return null;
            propertyGrid.SelectedObject = new CustomClass(_symbol);

            return panelLineSymbol;
        }

        #endregion
    }

	internal class comboItem 
	{
		public int Value=0;
		public string Text="";

		public comboItem(int val,string text) 
		{
			Value=val;
			Text=text;
		}

		public override string ToString()
		{
			return Text;
		}

	}
}

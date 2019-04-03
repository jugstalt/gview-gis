using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Symbology;

namespace gView.Framework.Symbology.UI
{
	/// <summary>
	/// Zusammenfassung für PropertyForm_SimplePointSymbol.
	/// </summary>
	internal class PropertyForm_SimplePointSymbol : System.Windows.Forms.Form,IPropertyPageUI,gView.Framework.Symbology.UI.IPropertyPanel
	{
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.PropertyGrid propertyGrid;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		public System.Windows.Forms.Panel panelSymbol;
		private ISymbol _symbol;

		public PropertyForm_SimplePointSymbol()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyForm_SimplePointSymbol));
            this.panelSymbol = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.panelSymbol.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelSymbol
            // 
            this.panelSymbol.AccessibleDescription = null;
            this.panelSymbol.AccessibleName = null;
            resources.ApplyResources(this.panelSymbol, "panelSymbol");
            this.panelSymbol.BackgroundImage = null;
            this.panelSymbol.Controls.Add(this.tabControl1);
            this.panelSymbol.Font = null;
            this.panelSymbol.Name = "panelSymbol";
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
            // PropertyForm_SimplePointSymbol
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.BackgroundImage = null;
            this.Controls.Add(this.panelSymbol);
            this.Font = null;
            this.Icon = null;
            this.Name = "PropertyForm_SimplePointSymbol";
            this.panelSymbol.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

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
            _symbol = symbol;


            propertyGrid.SelectedObject = new CustomClass(_symbol);
            return panelSymbol;
        }

        #endregion
    }
}

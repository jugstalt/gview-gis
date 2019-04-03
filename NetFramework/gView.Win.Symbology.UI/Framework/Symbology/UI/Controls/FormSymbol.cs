using System;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.Symbology;
using gView.Framework.Symbology.UI.Controls;

namespace gView.Framework.Symbology.UI
{
	/// <summary>
	/// Zusammenfassung für FormSymbol.
	/// </summary>
	public class FormSymbol : System.Windows.Forms.Form
    {
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Panel panel3;
        private SymbolControl symbolControl1;
        
		public FormSymbol(ISymbol symbol)
		{
			InitializeComponent();

            symbolControl1.Symbol = symbol;
		}


		public ISymbol Symbol 
		{
			get 
			{
                return symbolControl1.Symbol;
			}
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSymbol));
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnOk = new System.Windows.Forms.Button();
            this.symbolControl1 = new gView.Framework.Symbology.UI.Controls.SymbolControl();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.AccessibleDescription = null;
            this.panel2.AccessibleName = null;
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.BackgroundImage = null;
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Controls.Add(this.panel3);
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
            // 
            // panel3
            // 
            this.panel3.AccessibleDescription = null;
            this.panel3.AccessibleName = null;
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.BackgroundImage = null;
            this.panel3.Controls.Add(this.btnOk);
            this.panel3.Font = null;
            this.panel3.Name = "panel3";
            // 
            // btnOk
            // 
            this.btnOk.AccessibleDescription = null;
            this.btnOk.AccessibleName = null;
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.BackgroundImage = null;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Font = null;
            this.btnOk.Name = "btnOk";
            // 
            // symbolControl1
            // 
            this.symbolControl1.AccessibleDescription = null;
            this.symbolControl1.AccessibleName = null;
            resources.ApplyResources(this.symbolControl1, "symbolControl1");
            this.symbolControl1.BackgroundImage = null;
            this.symbolControl1.Font = null;
            this.symbolControl1.Name = "symbolControl1";
            this.symbolControl1.Symbol = null;
            // 
            // FormSymbol
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.BackgroundImage = null;
            this.Controls.Add(this.symbolControl1);
            this.Controls.Add(this.panel2);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = null;
            this.Name = "FormSymbol";
            this.Load += new System.EventHandler(this.FormSymbol_Load);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void FormSymbol_Load(object sender, System.EventArgs e)
		{
		}
	}
}

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace gView.Framework.Symbology.UI
{
	/// <summary>
	/// Zusammenfassung für Form_UITypeEditor_Character.
	/// </summary>
	internal class Form_UITypeEditor_Character : System.Windows.Forms.Form
	{
		public System.Windows.Forms.Panel panelChars;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private IWindowsFormsEditorService _wfes;
		private byte _c;

		public Form_UITypeEditor_Character(IWindowsFormsEditorService wfes,Font font,byte charakter)
		{
			//
			// Erforderlich für die Windows Form-Designerunterstützung
			//
			InitializeComponent();

			_wfes=wfes;
			_c   =charakter;

			if(font!=null) 
			{
				panelChars.Font=new Font(font.Name,12);
				int size=(int)(panelChars.Font.Size*2);
				panelChars.Width=panelChars.Height=size*16;
				
				byte c=0;
				for(int y=0;y<16;y++) 
				{
					for(int x=0;x<16;x++) 
					{
						Button btn=new Button();
						btn.Text=((char)c).ToString();
						btn.Width=btn.Height=size;
						btn.Top=y*size;
						btn.Left=x*size;
						btn.FlatStyle=FlatStyle.Popup;
						btn.BackColor=Color.Silver;
						btn.Click+=new EventHandler(btn_Click);
						panelChars.Controls.Add(btn);
						c++;
					}
				}
			}
		}

		public byte Charakter 
		{
			get { return _c; }
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
			this.panelChars = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// panelChars
			// 
			this.panelChars.BackColor = System.Drawing.Color.Silver;
			this.panelChars.Location = new System.Drawing.Point(16, 24);
			this.panelChars.Name = "panelChars";
			this.panelChars.Size = new System.Drawing.Size(400, 304);
			this.panelChars.TabIndex = 0;
			// 
			// Form_UITypeEditor_Character
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(456, 365);
			this.Controls.Add(this.panelChars);
			this.Name = "Form_UITypeEditor_Character";
			this.Text = "Form_UITypeEditor_Character";
			this.ResumeLayout(false);

		}
		#endregion

		private void btn_Click(object sender, System.EventArgs e)
		{
			if(!(sender is Button)) return; 
			_c=(byte)((Button)sender).Text[0];
			_wfes.CloseDropDown();
		}
	}
}

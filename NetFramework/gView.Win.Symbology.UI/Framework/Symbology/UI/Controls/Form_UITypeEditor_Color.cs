using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using gView.Framework.Symbology;

namespace gView.Framework.Symbology.UI
{
	/// <summary>
	/// Zusammenfassung für Form_UITypeEditor_Color.
	/// </summary>
	internal class Form_UITypeEditor_Color : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ColorDialog colorDialog1;
		public System.Windows.Forms.ListBox lbColor;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private Color _color;
		public System.Windows.Forms.Panel panelColor;
		private System.Windows.Forms.Button btnChooseColor;
		private System.Windows.Forms.Button btnTransparency;
		private System.Windows.Forms.Button btn100Transparency;
		public System.Windows.Forms.Panel panelSymbol;
		private System.Windows.Forms.Button btnSymbolComposer;
		private System.Windows.Forms.Button btnNoSymbol;
		private IWindowsFormsEditorService _wfes;
		private ISymbol _symbol=null;

		public Form_UITypeEditor_Color(IWindowsFormsEditorService wfes,Color color)
		{
			InitializeComponent();

			_wfes=wfes;
			_color=color;
		}

		public Form_UITypeEditor_Color(IWindowsFormsEditorService wfes,ISymbol symbol) 
		{
			InitializeComponent();

			_wfes=wfes;
			_symbol=symbol;
		}

		public Color Color 
		{
			get { return _color; }
		}
		public ISymbol Symbol 
		{
			get { return _symbol; }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_UITypeEditor_Color));
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.lbColor = new System.Windows.Forms.ListBox();
            this.panelColor = new System.Windows.Forms.Panel();
            this.btn100Transparency = new System.Windows.Forms.Button();
            this.btnTransparency = new System.Windows.Forms.Button();
            this.btnChooseColor = new System.Windows.Forms.Button();
            this.panelSymbol = new System.Windows.Forms.Panel();
            this.btnNoSymbol = new System.Windows.Forms.Button();
            this.btnSymbolComposer = new System.Windows.Forms.Button();
            this.panelColor.SuspendLayout();
            this.panelSymbol.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbColor
            // 
            this.lbColor.AccessibleDescription = null;
            this.lbColor.AccessibleName = null;
            resources.ApplyResources(this.lbColor, "lbColor");
            this.lbColor.BackgroundImage = null;
            this.lbColor.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbColor.Items.AddRange(new object[] {
            resources.GetString("lbColor.Items"),
            resources.GetString("lbColor.Items1"),
            resources.GetString("lbColor.Items2")});
            this.lbColor.Name = "lbColor";
            this.lbColor.SelectedIndexChanged += new System.EventHandler(this.lbColor_SelectedIndexChanged);
            // 
            // panelColor
            // 
            this.panelColor.AccessibleDescription = null;
            this.panelColor.AccessibleName = null;
            resources.ApplyResources(this.panelColor, "panelColor");
            this.panelColor.BackgroundImage = null;
            this.panelColor.Controls.Add(this.btn100Transparency);
            this.panelColor.Controls.Add(this.btnTransparency);
            this.panelColor.Controls.Add(this.btnChooseColor);
            this.panelColor.Font = null;
            this.panelColor.Name = "panelColor";
            // 
            // btn100Transparency
            // 
            this.btn100Transparency.AccessibleDescription = null;
            this.btn100Transparency.AccessibleName = null;
            resources.ApplyResources(this.btn100Transparency, "btn100Transparency");
            this.btn100Transparency.BackColor = System.Drawing.SystemColors.Control;
            this.btn100Transparency.BackgroundImage = null;
            this.btn100Transparency.Font = null;
            this.btn100Transparency.Name = "btn100Transparency";
            this.btn100Transparency.UseVisualStyleBackColor = false;
            this.btn100Transparency.Click += new System.EventHandler(this.btn100Transparency_Click);
            // 
            // btnTransparency
            // 
            this.btnTransparency.AccessibleDescription = null;
            this.btnTransparency.AccessibleName = null;
            resources.ApplyResources(this.btnTransparency, "btnTransparency");
            this.btnTransparency.BackColor = System.Drawing.SystemColors.Control;
            this.btnTransparency.BackgroundImage = null;
            this.btnTransparency.Font = null;
            this.btnTransparency.Name = "btnTransparency";
            this.btnTransparency.UseVisualStyleBackColor = false;
            this.btnTransparency.Click += new System.EventHandler(this.btnTransparency_Click);
            // 
            // btnChooseColor
            // 
            this.btnChooseColor.AccessibleDescription = null;
            this.btnChooseColor.AccessibleName = null;
            resources.ApplyResources(this.btnChooseColor, "btnChooseColor");
            this.btnChooseColor.BackColor = System.Drawing.SystemColors.Control;
            this.btnChooseColor.BackgroundImage = null;
            this.btnChooseColor.Font = null;
            this.btnChooseColor.Name = "btnChooseColor";
            this.btnChooseColor.UseVisualStyleBackColor = false;
            this.btnChooseColor.Click += new System.EventHandler(this.btnChooseColor_Click);
            // 
            // panelSymbol
            // 
            this.panelSymbol.AccessibleDescription = null;
            this.panelSymbol.AccessibleName = null;
            resources.ApplyResources(this.panelSymbol, "panelSymbol");
            this.panelSymbol.BackgroundImage = null;
            this.panelSymbol.Controls.Add(this.btnNoSymbol);
            this.panelSymbol.Controls.Add(this.btnSymbolComposer);
            this.panelSymbol.Font = null;
            this.panelSymbol.Name = "panelSymbol";
            // 
            // btnNoSymbol
            // 
            this.btnNoSymbol.AccessibleDescription = null;
            this.btnNoSymbol.AccessibleName = null;
            resources.ApplyResources(this.btnNoSymbol, "btnNoSymbol");
            this.btnNoSymbol.BackColor = System.Drawing.SystemColors.Control;
            this.btnNoSymbol.BackgroundImage = null;
            this.btnNoSymbol.Font = null;
            this.btnNoSymbol.Name = "btnNoSymbol";
            this.btnNoSymbol.UseVisualStyleBackColor = false;
            this.btnNoSymbol.Click += new System.EventHandler(this.btnNoSymbol_Click);
            // 
            // btnSymbolComposer
            // 
            this.btnSymbolComposer.AccessibleDescription = null;
            this.btnSymbolComposer.AccessibleName = null;
            resources.ApplyResources(this.btnSymbolComposer, "btnSymbolComposer");
            this.btnSymbolComposer.BackColor = System.Drawing.SystemColors.Control;
            this.btnSymbolComposer.BackgroundImage = null;
            this.btnSymbolComposer.Font = null;
            this.btnSymbolComposer.Name = "btnSymbolComposer";
            this.btnSymbolComposer.UseVisualStyleBackColor = false;
            this.btnSymbolComposer.Click += new System.EventHandler(this.btnSymbolComposer_Click);
            // 
            // Form_UITypeEditor_Color
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.BackgroundImage = null;
            this.Controls.Add(this.panelSymbol);
            this.Controls.Add(this.panelColor);
            this.Controls.Add(this.lbColor);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = null;
            this.Name = "Form_UITypeEditor_Color";
            this.Load += new System.EventHandler(this.Form_UITypeEditor_Color_Load);
            this.panelColor.ResumeLayout(false);
            this.panelSymbol.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void Form_UITypeEditor_Color_Load(object sender, System.EventArgs e)
		{
		
		}

		private void btnMore_Click(object sender, System.EventArgs e)
		{
			colorDialog1.ShowDialog(this);
		}

		private void lbColor_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			switch(lbColor.SelectedIndex) 
			{
				case 0:
					colorDialog1.Color=_color;
					if(colorDialog1.ShowDialog()==DialogResult.OK) 
					{
						_color=colorDialog1.Color;
						_wfes.CloseDropDown();
					}
					break;
				case 1:
					break;
				case 2:
					_color=Color.Transparent;
					_wfes.CloseDropDown();
					break;
			}
		}

		private void btnChooseColor_Click(object sender, System.EventArgs e)
		{
			colorDialog1.Color=_color;
			if(colorDialog1.ShowDialog()==DialogResult.OK) 
			{
				_color=colorDialog1.Color;
				_wfes.CloseDropDown();
			}
		}

		private void btnTransparency_Click(object sender, System.EventArgs e)
		{
			FormColorTransparency dlg=new FormColorTransparency(_color);
			if(dlg.ShowDialog()==DialogResult.OK) 
			{
				_color=dlg.Color;
				_wfes.CloseDropDown();
			}
		}

		private void btn100Transparency_Click(object sender, System.EventArgs e)
		{
			_color=Color.Transparent;
			_wfes.CloseDropDown();
		}

		private void btnSymbolComposer_Click(object sender, System.EventArgs e)
		{
			if(_symbol!=null) 
			{
				FormSymbol dlg=new FormSymbol(_symbol);
				if(dlg.ShowDialog()==DialogResult.OK) 
				{
					_symbol=dlg.Symbol;
				}
			}
			_wfes.CloseDropDown();
		}

		private void btnNoSymbol_Click(object sender, System.EventArgs e)
		{
			_symbol=null;
			_wfes.CloseDropDown();
		}
	}
}

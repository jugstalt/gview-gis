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
	internal class FormSymbol_old : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cmbSymbolTypes;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Panel panelProperties;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Panel panel3;
		private SymbolCollectionComposer symbolCollectionComposer;
		private ISymbol _symbol=null;
        private TextSymbolAlignment _txtSymbolAlignment = TextSymbolAlignment.Center;

		public FormSymbol_old(ISymbol symbol)
		{
			if(symbol!=null) 
			{
				_symbol=(ISymbol)symbol.Clone();
                if (_symbol is ILabel)
                {
                    _txtSymbolAlignment = ((ILabel)_symbol).TextSymbolAlignment;
                    ((ILabel)_symbol).TextSymbolAlignment = TextSymbolAlignment.Center;
                }
			}

			InitializeComponent();
		}


		public ISymbol Symbol 
		{
			get 
			{
                ISymbol symbol = symbolCollectionComposer.Symbol;
                if (symbol is ILabel)
                {
                    ((ILabel)symbol).TextSymbolAlignment = _txtSymbolAlignment;
                }
                return symbol;
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.cmbSymbolTypes = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.panelProperties = new System.Windows.Forms.Panel();
            this.symbolCollectionComposer = new gView.Framework.Symbology.UI.Controls.SymbolCollectionComposer();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmbSymbolTypes);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(166, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(442, 24);
            this.panel1.TabIndex = 0;
            // 
            // cmbSymbolTypes
            // 
            this.cmbSymbolTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbSymbolTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSymbolTypes.Location = new System.Drawing.Point(80, 0);
            this.cmbSymbolTypes.Name = "cmbSymbolTypes";
            this.cmbSymbolTypes.Size = new System.Drawing.Size(362, 21);
            this.cmbSymbolTypes.TabIndex = 1;
            this.cmbSymbolTypes.SelectedIndexChanged += new System.EventHandler(this.cmbSymbolTypes_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Symbol Type:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(166, 365);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(442, 56);
            this.panel2.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnCancel);
            this.panel3.Controls.Add(this.btnOk);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(194, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(248, 56);
            this.panel3.TabIndex = 2;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(8, 8);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 40);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOk.Location = new System.Drawing.Point(128, 8);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(112, 40);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "OK";
            // 
            // panelProperties
            // 
            this.panelProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProperties.Location = new System.Drawing.Point(166, 24);
            this.panelProperties.Name = "panelProperties";
            this.panelProperties.Size = new System.Drawing.Size(442, 341);
            this.panelProperties.TabIndex = 2;
            // 
            // symbolCollectionComposer
            // 
            this.symbolCollectionComposer.Dock = System.Windows.Forms.DockStyle.Left;
            this.symbolCollectionComposer.Location = new System.Drawing.Point(0, 0);
            this.symbolCollectionComposer.Name = "symbolCollectionComposer";
            this.symbolCollectionComposer.Size = new System.Drawing.Size(163, 421);
            this.symbolCollectionComposer.Symbol = null;
            this.symbolCollectionComposer.TabIndex = 3;
            this.symbolCollectionComposer.SelectedSymbolChanged += new gView.Framework.Symbology.UI.Controls.SymbolCollectionComposer.SelectedSymbolChangedEvent(this.symbolCollectionComposer_SelectedSymbolChanged);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(163, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 421);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // FormSymbol
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(608, 421);
            this.Controls.Add(this.panelProperties);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.symbolCollectionComposer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormSymbol";
            this.Text = "Symbol Composer";
            this.Load += new System.EventHandler(this.FormSymbol_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void FormSymbol_Load(object sender, System.EventArgs e)
		{
			if(_symbol==null) this.Close();

            if (_symbol is ITextSymbol)
            {
                ((ITextSymbol)_symbol).Text = "Label";
            }

			symbolCollectionComposer.AddSymbol(_symbol);
			symbolCollectionComposer.Init();
			//_symbol=(ISymbol)((SymbolCollectionItem)((SymbolCollection)symbolCollectionComposer.Symbol).Symbols[0]).Symbol;
			
			MakeGUI();
		}

		private void MakeGUI() 
		{
			cmbSymbolTypes.Items.Clear();

			if(PlugInManager.IsPlugin(_symbol)) 
			{
				PlugInManager compManager=new PlugInManager();

                foreach (var symbolType in compManager.GetPlugins(Plugins.Type.ISymbol)) 
				{
					ISymbol symbol=compManager.CreateInstance<ISymbol>(symbolType);
                    if (symbol is SymbolCollection) continue;

                    if(_symbol.GetType().Equals(symbol.GetType()))
					    symbol=_symbol;

					if(_symbol is IPointSymbol && symbol is IPointSymbol) 
					{
						cmbSymbolTypes.Items.Add(new SymbolItem(symbol));
					}
					if(_symbol is ILineSymbol && symbol is ILineSymbol) 
					{ 
						cmbSymbolTypes.Items.Add(new SymbolItem(symbol));
					}
					if(_symbol is IFillSymbol && symbol is IFillSymbol) 
					{
						cmbSymbolTypes.Items.Add(new SymbolItem(symbol));
					}
                    if (_symbol is ITextSymbol && symbol is ITextSymbol)
                    {
                        ((ITextSymbol)symbol).Text = "Label";
                        cmbSymbolTypes.Items.Add(new SymbolItem(symbol));
                    }
                    if (_symbol.GetType().Equals(symbol.GetType()) &&
                        cmbSymbolTypes.Items.Count > 0)
                    {
                        cmbSymbolTypes.SelectedItem = cmbSymbolTypes.Items[cmbSymbolTypes.Items.Count - 1];
                    }
				}
			}
		}
		private void cmbSymbolTypes_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			panelProperties.Controls.Clear();

			if(cmbSymbolTypes.SelectedItem==null) return;

			ISymbol symbol=((SymbolItem)cmbSymbolTypes.SelectedItem).Symbol;

			if(symbol is IPropertyPage) 
			{
                Control control = ((IPropertyPage)symbol).PropertyPage(symbol) as Control;
                if (control != null)
                {
                    control.Dock = DockStyle.Fill;
                    if (control.Parent is IPropertyPageUI)
                    {
                        ((IPropertyPageUI)control.Parent).PropertyChanged += new PropertyChangedEvent(PropertyChanged);
                    }
                    else if (control is IPropertyPageUI)
                    {
                        ((IPropertyPageUI)control).PropertyChanged += new PropertyChangedEvent(PropertyChanged);
                    }
                    panelProperties.Controls.Add(control);
                }
			}

			symbolCollectionComposer.ReplaceSelectedSymbol(symbol);
		}

		private void PropertyChanged(object propertyObject) 
		{
			symbolCollectionComposer.Refresh();
		}

		private void symbolCollectionComposer_SelectedSymbolChanged(gView.Framework.Symbology.ISymbol symbol)
		{
			if(symbol==null) return;
			_symbol=symbol;
			MakeGUI();
		}
	}

	internal class SymbolItem 
	{
		private ISymbol _sym;
		
		public SymbolItem(ISymbol sym) 
		{
			_sym=sym;
		}

		public override string ToString()
		{
			return _sym.Name;
		}

		public ISymbol Symbol 
		{
			get { return _sym; }
		}
	}
}

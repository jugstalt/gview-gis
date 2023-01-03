using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Windows.Forms;

namespace gView.Framework.Symbology.UI.Controls
{
    /// <summary>
    /// Zusammenfassung für FormSymbol.
    /// </summary>
    public class SymbolControl : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbSymbolTypes;
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Panel panelProperties;
        private System.Windows.Forms.Splitter splitter1;
        private SymbolCollectionComposer symbolCollectionComposer;
        private ISymbol _symbol = null;
        private Panel panel2;
        private SymbolSelectorControl symbolSelectorControl1;
        private TextSymbolAlignment _txtSymbolAlignment = TextSymbolAlignment.Center;

        public SymbolControl()
        {
            InitializeComponent();
        }
        public SymbolControl(ISymbol symbol)
            : this()
        {
            if (symbol != null)
            {
                _symbol = (ISymbol)symbol.Clone();

                if (_symbol is ILabel)
                {
                    _txtSymbolAlignment = ((ILabel)_symbol).TextSymbolAlignment;
                    ((ILabel)_symbol).TextSymbolAlignment = TextSymbolAlignment.Center;
                }

                symbolSelectorControl1.SymbolProtoType = _symbol;
            }
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
            set
            {
                if (value != null)
                {
                    _symbol = (ISymbol)value.Clone();

                    if (_symbol is ILabel)
                    {
                        _txtSymbolAlignment = ((ILabel)_symbol).TextSymbolAlignment;
                        ((ILabel)_symbol).TextSymbolAlignment = TextSymbolAlignment.Center;
                    }

                    symbolSelectorControl1.SymbolProtoType = _symbol;
                }
            }
        }
        /// <summary>
        /// Die verwendeten Ressourcen bereinigen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SymbolControl));
            this.panel1 = new System.Windows.Forms.Panel();
            this.cmbSymbolTypes = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelProperties = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel2 = new System.Windows.Forms.Panel();
            this.symbolSelectorControl1 = new gView.Framework.Symbology.UI.Controls.SymbolSelectorControl();
            this.symbolCollectionComposer = new gView.Framework.Symbology.UI.Controls.SymbolCollectionComposer();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmbSymbolTypes);
            this.panel1.Controls.Add(this.label1);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // cmbSymbolTypes
            // 
            resources.ApplyResources(this.cmbSymbolTypes, "cmbSymbolTypes");
            this.cmbSymbolTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSymbolTypes.Name = "cmbSymbolTypes";
            this.cmbSymbolTypes.SelectedIndexChanged += new System.EventHandler(this.cmbSymbolTypes_SelectedIndexChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // panelProperties
            // 
            resources.ApplyResources(this.panelProperties, "panelProperties");
            this.panelProperties.Name = "panelProperties";
            // 
            // splitter1
            // 
            resources.ApplyResources(this.splitter1, "splitter1");
            this.splitter1.Name = "splitter1";
            this.splitter1.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.symbolSelectorControl1);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // symbolSelectorControl1
            // 
            resources.ApplyResources(this.symbolSelectorControl1, "symbolSelectorControl1");
            this.symbolSelectorControl1.Name = "symbolSelectorControl1";
            this.symbolSelectorControl1.OnSymbolSelected += new System.EventHandler(this.symbolSelectorControl1_OnSymbolSelected);
            // 
            // symbolCollectionComposer
            // 
            resources.ApplyResources(this.symbolCollectionComposer, "symbolCollectionComposer");
            this.symbolCollectionComposer.Name = "symbolCollectionComposer";
            this.symbolCollectionComposer.Symbol = null;
            this.symbolCollectionComposer.SelectedSymbolChanged += new gView.Framework.Symbology.UI.Controls.SymbolCollectionComposer.SelectedSymbolChangedEvent(this.symbolCollectionComposer_SelectedSymbolChanged);
            // 
            // SymbolControl
            // 
            this.Controls.Add(this.panelProperties);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.symbolCollectionComposer);
            this.Controls.Add(this.panel2);
            this.Name = "SymbolControl";
            resources.ApplyResources(this, "$this");
            this.Load += new System.EventHandler(this.FormSymbol_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void FormSymbol_Load(object sender, System.EventArgs e)
        {
            if (_symbol == null)
            {
                return;
            }

            if (_symbol is ITextSymbol)
            {
                ((ITextSymbol)_symbol).Text = "Label";
            }

            symbolCollectionComposer.AddSymbol(_symbol);
            symbolCollectionComposer.Init();

            MakeGUI();
        }

        private void symbolSelectorControl1_OnSymbolSelected(object sender, EventArgs e)
        {
            var symbolSelector = sender as SymbolSelectorControl;

            if (symbolSelector?.SelectedSymbol != null)
            {
                _symbol = symbolSelector.SelectedSymbol;
            }

            symbolCollectionComposer.Symbol = new SymbolCollection();
            FormSymbol_Load(this, new EventArgs());
        }

        private void MakeGUI()
        {
            cmbSymbolTypes.Items.Clear();

            if (PlugInManager.IsPlugin(_symbol))
            {
                PlugInManager compManager = new PlugInManager();

                foreach (var symbolType in compManager.GetPlugins(Plugins.Type.ISymbol))
                {
                    ISymbol symbol = compManager.CreateInstance<ISymbol>(symbolType);
                    if (symbol is SymbolCollection)
                    {
                        continue;
                    }

                    if (_symbol.GetType().Equals(symbol.GetType()))
                    {
                        symbol = _symbol;
                    }

                    if (_symbol is IPointSymbol && symbol is IPointSymbol)
                    {
                        cmbSymbolTypes.Items.Add(new SymbolItem(symbol));
                    }
                    if (_symbol is ILineSymbol && symbol is ILineSymbol)
                    {
                        cmbSymbolTypes.Items.Add(new SymbolItem(symbol));
                    }
                    if (_symbol is IFillSymbol && symbol is IFillSymbol)
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

            if (cmbSymbolTypes.SelectedItem == null)
            {
                return;
            }

            ISymbol symbol = ((SymbolItem)cmbSymbolTypes.SelectedItem).Symbol;

            if (symbol != null)
            {
                if (symbol is IPropertyPage)
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
        }

        private void PropertyChanged(object propertyObject)
        {
            symbolCollectionComposer.Refresh();
        }

        private void symbolCollectionComposer_SelectedSymbolChanged(gView.Framework.Symbology.ISymbol symbol)
        {
            if (symbol == null)
            {
                return;
            }

            _symbol = symbol;
            MakeGUI();
        }

        #region Classes

        internal class SymbolItem
        {
            private ISymbol _sym;

            public SymbolItem(ISymbol sym)
            {
                _sym = sym;
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


        #endregion
    }
}

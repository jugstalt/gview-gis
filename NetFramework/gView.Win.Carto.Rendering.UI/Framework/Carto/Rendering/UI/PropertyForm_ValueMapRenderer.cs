using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using System;
using System.Windows.Forms;

namespace gView.Framework.Carto.Rendering.UI
{
    /// <summary>
    /// Zusammenfassung für PropertyPage_ValueMapRenderer.
    /// </summary>
    internal class PropertyPage_ValueMapRenderer : System.Windows.Forms.Form, IPropertyPanel
    {
        public System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ComboBox cmbFields;
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private IFeatureClass _fc;
        private System.Windows.Forms.Panel panel3;
        private gView.Framework.UI.Controls.SymbolsListView symbolsListView1;
        private System.Windows.Forms.Button btnAllValues;
        private Button btnInsertValue;
        private Button btnRemoveAllValues;
        private Button btnRotation;
        private GroupBox groupBox2;
        private gView.Framework.Symbology.UI.Controls.ColorGradientComboBox cmbGradient;
        private Button btnCarography;
        private Button btnInsertAllOthers;
        private ValueMapRenderer _renderer;

        public PropertyPage_ValueMapRenderer()
        {
            //
            // Erforderlich für die Windows Form-Designerunterstützung
            //
            InitializeComponent();

            cmbGradient.InsertStandardItems();
        }

        public void BuildList()
        {
            symbolsListView1.Clear();
            string[] labels = new string[2];
            //labels[0] = "all other values";
            //if (_renderer.DefaultSymbol is ILegendItem)
            //    labels[1] = ((ILegendItem)_renderer.DefaultSymbol).LegendLabel;
            //else
            //    labels[1] = "";
            //symbolsListView1.addSymbol(_renderer.DefaultSymbol, labels);

            foreach (string key in _renderer.Keys)
            {
                ISymbol symbol = _renderer[key];
                if (symbol is ISymbol)
                {
                    labels[0] = key;
                    if (symbol is ILegendItem)
                    {
                        labels[1] = ((ILegendItem)symbol).LegendLabel;
                    }
                    symbolsListView1.addSymbol(symbol, labels);
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyPage_ValueMapRenderer));
            this.panel1 = new System.Windows.Forms.Panel();
            this.symbolsListView1 = new gView.Framework.UI.Controls.SymbolsListView();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnCarography = new System.Windows.Forms.Button();
            this.btnRotation = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnInsertAllOthers = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cmbGradient = new gView.Framework.Symbology.UI.Controls.ColorGradientComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbFields = new System.Windows.Forms.ComboBox();
            this.btnRemoveAllValues = new System.Windows.Forms.Button();
            this.btnInsertValue = new System.Windows.Forms.Button();
            this.btnAllValues = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.symbolsListView1);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // symbolsListView1
            // 
            this.symbolsListView1.AllowDrop = true;
            resources.ApplyResources(this.symbolsListView1, "symbolsListView1");
            this.symbolsListView1.LegendText = "";
            this.symbolsListView1.Name = "symbolsListView1";
            this.symbolsListView1.ValueText = "";
            this.symbolsListView1.OnSymbolChanged += new gView.Framework.UI.Controls.SymbolsListView.SymbolChanged(this.symbolsListView1_OnSymbolChanged);
            this.symbolsListView1.OnLabelChanged += new gView.Framework.UI.Controls.SymbolsListView.LabelChanged(this.symbolsListView1_OnLabelChanged);
            this.symbolsListView1.AfterLegendOrdering += new System.EventHandler(this.symbolsListView1_AfterLegendOrdering);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnCarography);
            this.panel3.Controls.Add(this.btnRotation);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // btnCarography
            // 
            resources.ApplyResources(this.btnCarography, "btnCarography");
            this.btnCarography.Name = "btnCarography";
            this.btnCarography.UseVisualStyleBackColor = true;
            this.btnCarography.Click += new System.EventHandler(this.btnCarography_Click);
            // 
            // btnRotation
            // 
            resources.ApplyResources(this.btnRotation, "btnRotation");
            this.btnRotation.Name = "btnRotation";
            this.btnRotation.UseVisualStyleBackColor = true;
            this.btnRotation.Click += new System.EventHandler(this.btnRotation_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnInsertAllOthers);
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Controls.Add(this.btnRemoveAllValues);
            this.panel2.Controls.Add(this.btnInsertValue);
            this.panel2.Controls.Add(this.btnAllValues);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // btnInsertAllOthers
            // 
            resources.ApplyResources(this.btnInsertAllOthers, "btnInsertAllOthers");
            this.btnInsertAllOthers.Name = "btnInsertAllOthers";
            this.btnInsertAllOthers.Click += new System.EventHandler(this.btnInsertAllOthers_Click);
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.cmbGradient);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // cmbGradient
            // 
            this.cmbGradient.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            resources.ApplyResources(this.cmbGradient, "cmbGradient");
            this.cmbGradient.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbGradient.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGradient.FormattingEnabled = true;
            this.cmbGradient.Items.AddRange(new object[] {
            resources.GetString("cmbGradient.Items"),
            resources.GetString("cmbGradient.Items1")});
            this.cmbGradient.Name = "cmbGradient";
            this.cmbGradient.GradientSelected += new gView.Framework.Symbology.UI.Controls.GradientSelectedEventHandler(this.cmbGradient_GradientSelected);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbFields);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // cmbFields
            // 
            this.cmbFields.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbFields, "cmbFields");
            this.cmbFields.Name = "cmbFields";
            this.cmbFields.SelectedIndexChanged += new System.EventHandler(this.cmbFields_SelectedIndexChanged);
            // 
            // btnRemoveAllValues
            // 
            resources.ApplyResources(this.btnRemoveAllValues, "btnRemoveAllValues");
            this.btnRemoveAllValues.Name = "btnRemoveAllValues";
            this.btnRemoveAllValues.Click += new System.EventHandler(this.btnRemoveAllValues_Click);
            // 
            // btnInsertValue
            // 
            resources.ApplyResources(this.btnInsertValue, "btnInsertValue");
            this.btnInsertValue.Name = "btnInsertValue";
            this.btnInsertValue.Click += new System.EventHandler(this.btnInsertValue_Click);
            // 
            // btnAllValues
            // 
            resources.ApplyResources(this.btnAllValues, "btnAllValues");
            this.btnAllValues.Name = "btnAllValues";
            this.btnAllValues.Click += new System.EventHandler(this.btnAllValues_Click);
            // 
            // PropertyPage_ValueMapRenderer
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.panel1);
            this.Name = "PropertyPage_ValueMapRenderer";
            this.Load += new System.EventHandler(this.PropertyPage_ValueMapRenderer_Load);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void cmbFields_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            _renderer.ValueField = cmbFields.SelectedItem.ToString();
        }

        private void PropertyPage_ValueMapRenderer_Load(object sender, System.EventArgs e)
        {

        }

        async private void btnAllValues_Click(object sender, System.EventArgs e)
        {
            try
            {
                DistinctFilter filter = new DistinctFilter(cmbFields.SelectedItem.ToString());
                //QueryFilter filter=new QueryFilter();
                //filter.AddField("DISTINCT("+cmbFields.SelectedItem.ToString()+")");
                //filter.AddField(cmbFields.SelectedItem.ToString());
                filter.OrderBy = cmbFields.SelectedItem.ToString();

                Array array = Array.CreateInstance(typeof(string), _renderer.Keys.Count);
                _renderer.Keys.CopyTo(array, 0);
                foreach (string key in array)
                {
                    _renderer.RemoveSymbol(key);
                }

                IFeatureCursor cursor = (IFeatureCursor)await _fc.Search(filter);

                string[] labels = new string[2];
                IFeature feature;
                while ((feature = await cursor.NextFeature()) != null)
                {
                    labels[0] = ((FieldValue)feature.Fields[0]).Value.ToString();
                    if (_renderer[labels[0]] == null)
                    {
                        _renderer[labels[0]] = null;  // neues Symbol erzeugen...
                    }
                    //symbolsListView1.addSymbol(_renderer[labels[0]],labels); 
                    int i = 0;
                }

                BuildList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void symbolsListView1_OnSymbolChanged(string key, ISymbol symbol)
        {
            ILegendItem legendItem = _renderer[key] as ILegendItem;
            _renderer.SetSymbol(legendItem, symbol);
        }

        private void symbolsListView1_OnLabelChanged(ISymbol symbol, int labelnr, string label)
        {
            if (_renderer == null || labelnr != 1)
            {
                return;
            }

            if (!(symbol is ILegendItem))
            {
                return;
            } ((ILegendItem)symbol).LegendLabel = label;
        }

        private void symbolsListView1_OnDeleteItem(string key)
        {
            if (_renderer == null)
            {
                return;
            }

            _renderer.RemoveSymbol(key);
        }

        private void symbolsListView1_AfterLegendOrdering(object sender, EventArgs e)
        {
            string[] keys = symbolsListView1.OrderedKeys;

            _renderer.ReorderLegendItems(keys);
        }

        private void btnRemoveAllValues_Click(object sender, EventArgs e)
        {
            if (_renderer.Keys.Count == 0)
            {
                return;
            }

            Array array = Array.CreateInstance(typeof(string), _renderer.Keys.Count);
            _renderer.Keys.CopyTo(array, 0);
            foreach (string key in array)
            {
                _renderer.RemoveSymbol(key);
            }

            BuildList();
        }

        private void btnInsertValue_Click(object sender, EventArgs e)
        {
            if (_renderer == null)
            {
                return;
            }

            PropertyForm_ValueMapRenderer_Dialog_InsertValue dlg = new PropertyForm_ValueMapRenderer_Dialog_InsertValue();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _renderer[dlg.Key] = null;  // Creates new Symbol
                ISymbol symbol = _renderer[dlg.Key];
                if (symbol == null)
                {
                    return;
                }

                if (symbol is ILegendItem)
                {
                    ((ILegendItem)symbol).LegendLabel = dlg.Label;
                }

                string[] labels = new string[2];
                labels[0] = dlg.Key;
                labels[1] = dlg.Label;

                symbolsListView1.addSymbol(symbol, labels);
            }
        }

        private void btnInsertAllOthers_Click(object sender, EventArgs e)
        {
            if (_renderer == null)
            {
                return;
            }

            if (_renderer[null] != null)
            {
                return;
            }

            _renderer[null] = null;  // Creates new Symbol
            ISymbol symbol = _renderer[null];
            if (symbol == null)
            {
                return;
            }

            if (symbol is ILegendItem)
            {
                ((ILegendItem)symbol).LegendLabel = "All other values";
            }

            string[] labels = new string[2];
            labels[0] = "__gview_all_other_values__";
            labels[1] = "All other values";

            symbolsListView1.addSymbol(symbol, labels);
        }

        private void btnRotation_Click(object sender, EventArgs e)
        {
            if (_renderer == null || _fc == null)
            {
                return;
            }

            FormRotationType dlg = new FormRotationType(_renderer.SymbolRotation, _fc);
            dlg.ShowDialog();
        }

        #region IPropertyPanel Member

        public object PropertyPanel(IFeatureRenderer renderer, IFeatureLayer layer)
        {
            if (layer != null)
            {
                _fc = layer.FeatureClass;
            }

            _renderer = renderer as ValueMapRenderer;

            if (_fc == null || _renderer == null)
            {
                return null;
            }

            _renderer.GeometryType = layer.LayerGeometryType; // _fc.GeometryType;

            foreach (IField field in _fc.Fields.ToEnumerable())
            {
                if (field.type == FieldType.binary || field.type == FieldType.Shape)
                {
                    continue;
                }

                cmbFields.Items.Add(field.name);
                if (field.name == _renderer.ValueField)
                {
                    cmbFields.SelectedIndex = cmbFields.Items.Count - 1;
                }
            }

            btnRotation.Enabled = (_fc.GeometryType == GeometryType.Point || _fc.GeometryType == GeometryType.Multipoint);

            if (cmbFields.SelectedIndex == -1)
            {
                cmbFields.SelectedIndex = 0;
            }

            symbolsListView1.OnSymbolChanged += new gView.Framework.UI.Controls.SymbolsListView.SymbolChanged(symbolsListView1_OnSymbolChanged);
            symbolsListView1.OnLabelChanged += new gView.Framework.UI.Controls.SymbolsListView.LabelChanged(symbolsListView1_OnLabelChanged);
            symbolsListView1.OnDeleteItem += new gView.Framework.UI.Controls.SymbolsListView.DeleteItem(symbolsListView1_OnDeleteItem);
            BuildList();

            return panel1;
        }

        #endregion

        private void cmbGradient_GradientSelected(object sender, gView.Framework.Symbology.UI.Controls.GradientSelectedEventArgs args)
        {
            if (_renderer == null || _renderer.Keys == null ||
                _renderer.Keys.Count == 0 ||
                args == null || args.ColorGradient == null)
            {
                return;
            }

            try
            {
                double span = _renderer.Keys.Count;

                ColorGradient gradient = args.ColorGradient;
                int i = 0;
                foreach (string key in _renderer.Keys)
                {
                    ISymbol symbol = _renderer[key];
                    if (symbol is ISymbol)
                    {
                        Helper.AlterSymbolColor(symbol, gradient.Color1, gradient.Color2, (double)i / span);
                    }

                    i++;
                }

                BuildList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exeception");
            }
        }

        private void btnCarography_Click(object sender, EventArgs e)
        {
            if (_renderer == null)
            {
                return;
            }

            FormCartographicInterpretation dlg = new FormCartographicInterpretation(_renderer.CartoMethod);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _renderer.CartoMethod = dlg.CartographicMethod;
            }
        }

        private void btnRemoveSelected_Click(object sender, EventArgs e)
        {
            symbolsListView1.RemoveSelected();
        }


    }
}

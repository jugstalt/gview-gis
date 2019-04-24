using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Symbology;
using gView.Framework.UI;
using gView.Framework.Geometry;
using gView.Framework.Carto.UI;
using gView.Framework.Symbology.UI;

namespace gView.Framework.Carto.Rendering.UI
{
    /// <summary>
    /// Zusammenfassung für PropertyForm_SimpleRenderer.
    /// </summary>
    internal class PropertyForm_SimpleRenderer : System.Windows.Forms.Form, IPropertyPanel
    {
        public System.Windows.Forms.Panel panel1;
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Button btnChooseSymbol;
        private System.Windows.Forms.GroupBox GroupBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private GroupBox groupBox2;
        private Button btnRotation;
        private IFeatureRenderer _renderer = null;
        private GroupBox gbCartography;
        private RadioButton btnCartoSymbolOrdering;
        private RadioButton btnCartoSimple;
        private IFeatureClass _featureClass = null;

        public PropertyForm_SimpleRenderer()
        {

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyForm_SimpleRenderer));
            this.panel1 = new System.Windows.Forms.Panel();
            this.gbCartography = new System.Windows.Forms.GroupBox();
            this.btnCartoSymbolOrdering = new System.Windows.Forms.RadioButton();
            this.btnCartoSimple = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnRotation = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.btnChooseSymbol = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.gbCartography.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.GroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.gbCartography);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.panel3);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // gbCartography
            // 
            this.gbCartography.Controls.Add(this.btnCartoSymbolOrdering);
            this.gbCartography.Controls.Add(this.btnCartoSimple);
            resources.ApplyResources(this.gbCartography, "gbCartography");
            this.gbCartography.Name = "gbCartography";
            this.gbCartography.TabStop = false;
            // 
            // btnCartoSymbolOrdering
            // 
            resources.ApplyResources(this.btnCartoSymbolOrdering, "btnCartoSymbolOrdering");
            this.btnCartoSymbolOrdering.Name = "btnCartoSymbolOrdering";
            this.btnCartoSymbolOrdering.TabStop = true;
            this.btnCartoSymbolOrdering.UseVisualStyleBackColor = true;
            this.btnCartoSymbolOrdering.CheckedChanged += new System.EventHandler(this.btnCartoSymbolOrdering_CheckedChanged);
            // 
            // btnCartoSimple
            // 
            resources.ApplyResources(this.btnCartoSimple, "btnCartoSimple");
            this.btnCartoSimple.Name = "btnCartoSimple";
            this.btnCartoSimple.TabStop = true;
            this.btnCartoSimple.UseVisualStyleBackColor = true;
            this.btnCartoSimple.CheckedChanged += new System.EventHandler(this.btnCartoSimple_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnRotation);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
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
            this.panel2.Controls.Add(this.GroupBox1);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.btnChooseSymbol);
            resources.ApplyResources(this.GroupBox1, "GroupBox1");
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.TabStop = false;
            // 
            // btnChooseSymbol
            // 
            resources.ApplyResources(this.btnChooseSymbol, "btnChooseSymbol");
            this.btnChooseSymbol.Name = "btnChooseSymbol";
            this.btnChooseSymbol.Click += new System.EventHandler(this.btnChooseSymbol_Click);
            this.btnChooseSymbol.Paint += new System.Windows.Forms.PaintEventHandler(this.btnChooseSymbol_Paint);
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // PropertyForm_SimpleRenderer
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.panel1);
            this.Name = "PropertyForm_SimpleRenderer";
            this.panel1.ResumeLayout(false);
            this.gbCartography.ResumeLayout(false);
            this.gbCartography.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.GroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void btnChooseSymbol_Click(object sender, System.EventArgs e)
        {
            if (_renderer == null) return;

            if (_renderer is IFeatureRenderer2)
            {
                FormSymbol dlg = new FormSymbol(((IFeatureRenderer2)_renderer).Symbol);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ((IFeatureRenderer2)_renderer).Symbol = dlg.Symbol;
                    UpdateUI();
                }
            }
        }

        private void btnChooseSymbol_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (_renderer == null) return;

            SymbolPreview.Draw(
                e.Graphics,
                new Rectangle(5, 5, btnChooseSymbol.Width - 10, btnChooseSymbol.Height - 10),
                ((IFeatureRenderer2)_renderer).Symbol, false);
        }

        private void btnRotation_Click(object sender, EventArgs e)
        {
            if (_featureClass == null && !(_renderer is SimpleRenderer)) return;

            FormRotationType dlg = new FormRotationType(((SimpleRenderer)_renderer).SymbolRotation, _featureClass);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                dlg = null;
            }
        }

        private void UpdateUI()
        {
            if (_renderer is SimpleRenderer)
            {
                SimpleRenderer simpleRenderer = (SimpleRenderer)_renderer;
                gbCartography.Enabled = simpleRenderer.Symbol is ISymbolCollection;
                switch (simpleRenderer.CartoMethod)
                {
                    case SimpleRenderer.CartographicMethod.Simple:
                        btnCartoSimple.Checked = true;
                        break;
                    case SimpleRenderer.CartographicMethod.SymbolOrder:
                        btnCartoSymbolOrdering.Checked = true;
                        break;
                }
            }
            else
            {
                gbCartography.Enabled = false;
            }
        }

        #region IPropertyPanel Member

        public object PropertyPanel(IFeatureRenderer renderer, IFeatureLayer layer)
        {
            _renderer = renderer;
            if(layer!=null)
                _featureClass = layer.FeatureClass;

            InitializeComponent();

            btnRotation.Enabled = false;
            if (_featureClass != null)
            {
                if (_featureClass.GeometryType == geometryType.Point ||
                    _featureClass.GeometryType == geometryType.Multipoint)
                    btnRotation.Enabled = true;
            }

            UpdateUI();

            return panel1;
        }

        #endregion

        private void btnCartoSimple_CheckedChanged(object sender, EventArgs e)
        {
            if (btnCartoSimple.Checked && _renderer is SimpleRenderer)
                ((SimpleRenderer)_renderer).CartoMethod = SimpleRenderer.CartographicMethod.Simple;
        }

        private void btnCartoSymbolOrdering_CheckedChanged(object sender, EventArgs e)
        {
            if (btnCartoSymbolOrdering.Checked && _renderer is SimpleRenderer)
                ((SimpleRenderer)_renderer).CartoMethod = SimpleRenderer.CartographicMethod.SymbolOrder;
        }
    }
}

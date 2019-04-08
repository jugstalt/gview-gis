using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Interoperability.OGC.SLD;
using gView.Framework.UI.Dialogs;
using System.Xml;
using gView.Framework.Geometry;

namespace gView.Interoperability.OGC.UI.SLD
{
    public partial class FormNewSLDRule : Form
    {
        private IFeatureLayer _layer = null;
        private SLDRenderer.Rule _rule;
        private IQueryFilter _queryFilter = null;

        public FormNewSLDRule(IFeatureLayer layer, SLDRenderer.Rule rule)
        {
            if (rule == null || layer == null)
                return;

            InitializeComponent();

            symbolControl1.Symbol = rule.Symbol;

            _layer = layer;
            _rule = rule;

            btnQueryBuilder.Enabled = (_layer.FeatureClass != null);

            switch (rule.filterType)
            {
                case SLDRenderer.Rule.FilterType.None:
                    radioFilterNone.Checked = true;
                    break;
                case SLDRenderer.Rule.FilterType.OgcFilter:
                    radioFilterOgc.Checked = true;
                    break;
                case SLDRenderer.Rule.FilterType.ElseFilter:
                    radioFilterElse.Checked = true;
                    break;
            }

            if (rule.Filter != null && _layer != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(rule.Filter.ToXmlString(true));
                XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                ns.AddNamespace("OGC", "http://www.opengis.net/ogc");

                _queryFilter = gView.Framework.OGC.WFS.Filter.FromWFS(
                    _layer.FeatureClass, doc.SelectSingleNode("OGC:Filter", ns), GmlVersion.v1);
                gView.Framework.OGC.WFS.Filter.AppendSortBy(_queryFilter, doc.SelectSingleNode("OGC:SortBy", ns));

                txtQuery.Text = _queryFilter.WhereClause;
            }

            if (rule.MinScale <= 1.0 && rule.MaxScale <= 1.0)
            {
                radioAllScales.Checked = true;
            }
            else
            {
                radioScales.Checked = true;
            }

            txtMinScale.Value = (decimal)Math.Abs(rule.MinScale);
            txtMaxScale.Value = (decimal)Math.Abs(rule.MaxScale);

            TabScalesGUI();
        }

        #region Tab Filter
        private void btnQueryBuilder_Click(object sender, EventArgs e)
        {
            if (_rule == null || _layer == null) return;

            FormQueryBuilder dlg = new FormQueryBuilder(_layer);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtQuery.Text = dlg.whereClause;
            }
        }
        #endregion

        #region Tab Scales
        private void radioAllScales_CheckedChanged(object sender, EventArgs e)
        {
            TabScalesGUI();
        }

        private void radioScales_CheckedChanged(object sender, EventArgs e)
        {
            TabScalesGUI();
        }

        private void TabScalesGUI()
        {
            if (_rule == null) return;

            txtMinScale.Enabled = txtMaxScale.Enabled =
                radioScales.Checked;

            if (radioAllScales.Checked)
            {
                if (_rule.MinScale > 0) _rule.MinScale = -_rule.MinScale;
                if (_rule.MaxScale > 0) _rule.MaxScale = -_rule.MaxScale;
            }
        }

        private void txtMinScale_ValueChanged(object sender, EventArgs e)
        {
            if (_rule == null) return;
            _rule.MinScale = (double)txtMinScale.Value;
        }

        private void txtMaxScale_ValueChanged(object sender, EventArgs e)
        {
            if (_rule == null) return;
            _rule.MaxScale = (double)txtMaxScale.Value;
        }
        #endregion

        public SLDRenderer.Rule Rule
        {
            get { return _rule; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_rule != null)
            {
                _rule.Symbol = symbolControl1.Symbol;

                if (radioFilterNone.Checked)
                    _rule.filterType = SLDRenderer.Rule.FilterType.None;
                else if (radioFilterOgc.Checked)
                    _rule.filterType = SLDRenderer.Rule.FilterType.OgcFilter;
                else if (radioFilterElse.Checked)
                    _rule.filterType = SLDRenderer.Rule.FilterType.ElseFilter;

                if (txtQuery.Text.Trim() != String.Empty)
                {
                    if (_queryFilter == null)
                        _queryFilter = new QueryFilter();
                    _queryFilter.WhereClause = txtQuery.Text;
                    _rule.Filter = new gView.Framework.OGC.WFS.Filter(_layer.FeatureClass, _queryFilter, Framework.Geometry.GmlVersion.v1);
                }
                else if (_queryFilter != null)
                {
                    _queryFilter.WhereClause = txtQuery.Text;
                    _rule.Filter = new gView.Framework.OGC.WFS.Filter(_layer.FeatureClass, _queryFilter, Framework.Geometry.GmlVersion.v1);
                }

                _rule.MinScale = (double)txtMinScale.Value;
                _rule.MaxScale = (double)txtMaxScale.Value;

                if (radioAllScales.Checked)
                {
                    if (radioAllScales.Checked)
                    {
                        if (_rule.MinScale > 0) _rule.MinScale = -_rule.MinScale;
                        if (_rule.MaxScale > 0) _rule.MaxScale = -_rule.MaxScale;
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Carto;
using gView.Framework.Symbology;
using gView.Framework.UI;
using gView.Framework.Carto.UI;
using gView.Framework.Symbology.UI;

namespace gView.Framework.Carto.Rendering.UI
{
    internal partial class PropertyForm_SimpleLabelRenderer : Form,IPropertyPanel2
    {
        private SimpleLabelRenderer _renderer;
        private IFeatureClass _fc;

        public PropertyForm_SimpleLabelRenderer()
        {
            
        }

        private void MakeGUI()
        {
            if (_fc != null && _renderer != null)
            {
                foreach (IField field in _fc.Fields.ToEnumerable())
                {
                    if (field.type == FieldType.binary || field.type == FieldType.Shape) continue;

                    if (field.type == FieldType.String && _renderer.FieldName == "")
                        _renderer.FieldName = field.name;

                    LabelFieldItem item = new LabelFieldItem(field);
                    cmbFields.Items.Add(item);

                    if (field.name == _renderer.FieldName && !_renderer.UseExpression)
                    {
                        cmbFields.SelectedItem = item;
                    }
                }
                cmbFields.Items.Add("{Expression}");

                if (_renderer.UseExpression)
                    cmbFields.SelectedIndex = cmbFields.Items.Count - 1;
            }

            if (_renderer.TextSymbol != null)
            {
                switch (_renderer.TextSymbol.TextSymbolAlignment)
                {
                    case TextSymbolAlignment.rightAlignOver:
                        radioButton1.Checked = true;
                        break;
                    case TextSymbolAlignment.Over:
                        radioButton2.Checked = true;
                        break;
                    case TextSymbolAlignment.leftAlignOver:
                        radioButton3.Checked = true;
                        break;
                    case TextSymbolAlignment.rightAlignCenter:
                        radioButton4.Checked = true;
                        break;
                    case TextSymbolAlignment.Center:
                        radioButton5.Checked = true;
                        break;
                    case TextSymbolAlignment.leftAlignCenter:
                        radioButton6.Checked = true;
                        break;
                    case TextSymbolAlignment.rightAlignUnder:
                        radioButton7.Checked = true;
                        break;
                    case TextSymbolAlignment.Under:
                        radioButton8.Checked = true;
                        break;
                    case TextSymbolAlignment.leftAlignUnder:
                        radioButton9.Checked = true;
                        break;
                }
            }
            DrawPreview();

            cmbHowManyLabels.SelectedIndex = (int)_renderer.HowManyLabels;
            cmbLabelPriority.SelectedIndex = (int)_renderer.LabelPriority;

            gbOrientation.Visible = _fc.GeometryType == Geometry.geometryType.Polyline;
        }

        private void DrawPreview()
        {
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromHwnd(panelPreview.Handle);

            Rectangle rect=new Rectangle(0, 0, panelPreview.Width, panelPreview.Height);
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                gr.FillRectangle(brush, rect);
            }
            using (Pen pen = new Pen(Color.Gray, 0))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                gr.DrawLine(pen, 0, rect.Height / 2, rect.Width, rect.Height / 2);
                gr.DrawLine(pen, rect.Width / 2, 0, rect.Width / 2, rect.Height);
            }

            if (_renderer.TextSymbol is ISymbol)
            {
                _renderer.TextSymbol.Text = "Label";
                SymbolPreview.Draw(gr, rect, (ISymbol)_renderer.TextSymbol, false);
            }
            gr.Dispose();
            gr = null;
        }

        private void PropertyForm_SimpleLabelRenderer_Load(object sender, EventArgs e)
        {
            
        }

        private void cmbFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_renderer == null) return;

            if (cmbFields.SelectedItem is LabelFieldItem)
            {
                if (((LabelFieldItem)cmbFields.SelectedItem).Field == null)
                {
                    _renderer.FieldName = "";
                }
                else
                {
                    _renderer.FieldName = ((LabelFieldItem)cmbFields.SelectedItem).Field.name;
                }
                _renderer.UseExpression = false;
            }
            else if (cmbFields.SelectedIndex == cmbFields.Items.Count - 1)
            {
                _renderer.UseExpression = true;
            }
            DrawPreview();
        }

        private void btnSymbol_Click(object sender, EventArgs e)
        {
            if (!(_renderer.TextSymbol is ISymbol)) return;

            FormSymbol dlg = new FormSymbol((ISymbol)_renderer.TextSymbol);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (dlg.Symbol is ITextSymbol)
                {
                    _renderer.TextSymbol = dlg.Symbol as ITextSymbol;
                    DrawPreview();
                }
            }
        }

        private void SetSymbolAlignment(TextSymbolAlignment align)
        {
            if (_renderer == null) return;
            if (_renderer.TextSymbol is ILabel)
            {
                ((ILabel)_renderer.TextSymbol).TextSymbolAlignment = align;
            }
            DrawPreview();
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            SetSymbolAlignment(TextSymbolAlignment.rightAlignOver);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            SetSymbolAlignment(TextSymbolAlignment.Over);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            SetSymbolAlignment(TextSymbolAlignment.leftAlignOver);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            SetSymbolAlignment(TextSymbolAlignment.rightAlignCenter);
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            SetSymbolAlignment(TextSymbolAlignment.Center);
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            SetSymbolAlignment(TextSymbolAlignment.leftAlignCenter);
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            SetSymbolAlignment(TextSymbolAlignment.rightAlignUnder);
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            SetSymbolAlignment(TextSymbolAlignment.Under);
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            SetSymbolAlignment(TextSymbolAlignment.leftAlignUnder);
        }

        private void panelPreview_Paint(object sender, PaintEventArgs e)
        {
            DrawPreview();
        }

        private void cmbHowManyLabels_SelectedIndexChanged(object sender, EventArgs e)
        {
            _renderer.HowManyLabels = (SimpleLabelRenderer.howManyLabels)cmbHowManyLabels.SelectedIndex;
        }

        private void cmbLabelPriority_SelectedIndexChanged(object sender, EventArgs e)
        {
            _renderer.LabelPriority = (SimpleLabelRenderer.labelPriority)cmbLabelPriority.SelectedIndex;
        }

        private void btnRotation_Click(object sender, EventArgs e)
        {
            if (_renderer == null || _fc == null) return;

            FormRotationType dlg = new FormRotationType(_renderer.SymbolRotation, _fc);
            dlg.ShowDialog();
        }

        private void btnExpression_Click(object sender, EventArgs e)
        {
            if (_renderer == null) return;

            FormLabelExpression labelExpression = new FormLabelExpression(_fc);
            labelExpression.Expression = _renderer.LabelExpression;

            if (labelExpression.ShowDialog() == DialogResult.OK)
            {
                _renderer.LabelExpression = labelExpression.Expression;
                cmbFields.SelectedIndex = cmbFields.Items.Count - 1;
            }
        }

        private void btnOrientation_Click(object sender, EventArgs e)
        {
            switch (_fc.GeometryType)
            {
                case Geometry.geometryType.Polyline:
                    FormLineLabellingOrientation dlgLLO = new FormLineLabellingOrientation(_renderer);
                    if (dlgLLO.ShowDialog() == DialogResult.OK)
                        _renderer.CartoLineLabelling = dlgLLO.CartoLineLabelling;
                    break;
            }
        }

        #region IPropertyPanel2 Member

        public object PropertyPanel(ILabelRenderer renderer, IFeatureLayer layer)
        {
            _renderer = renderer as SimpleLabelRenderer;
            if (layer != null)
                _fc = layer.FeatureClass;

            InitializeComponent();

            MakeGUI();

            return panel1;
        }

        #endregion
    }

    internal class LabelFieldItem
    {
        public IField _field;

        public LabelFieldItem(IField field)
        {
            _field = field;
        }

        public IField Field
        {
            get
            {
                return _field;
            }
        }

        public override string ToString()
        {
            if (_field == null) return "null";
            return _field.aliasname;
        }
    }
}
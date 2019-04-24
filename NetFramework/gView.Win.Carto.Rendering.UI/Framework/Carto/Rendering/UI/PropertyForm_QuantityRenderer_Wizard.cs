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
using gView.Framework.UI.Controls;
using gView.Framework.Carto.UI;
using gView.Framework.Symbology.UI;

namespace gView.Framework.Carto.Rendering.UI
{
    public partial class PropertyForm_QuantityRenderer_Wizard : Form
    {
        private QuantityRenderer _renderer;
        private IFeatureClass _fc;
        private ISymbol _minSymbol, /*_midSymbol,*/ _maxSymbol;

        public PropertyForm_QuantityRenderer_Wizard(
            QuantityRenderer renderer,
            IFeatureClass fc)
        {
            InitializeComponent();

            _renderer = renderer;
            _fc = fc;
        }

        private void PropertyForm_QuantityRenderer_Wizard_Load(object sender, EventArgs e)
        {
            if (_fc == null || _renderer == null ||
                String.IsNullOrEmpty(_renderer.ValueField)) return;

            _minSymbol = RendererFunctions.CreateStandardSymbol(_fc.GeometryType);
            //_midSymbol = RendererFunctions.CreateStandardSymbol(_fc.GeometryType);
            _maxSymbol = RendererFunctions.CreateStandardSymbol(_fc.GeometryType);

            IField field = _fc.FindField(_renderer.ValueField);
            if (field == null) return;
            switch (field.type)
            {
                case FieldType.ID:
                case FieldType.integer:
                    numMin.DataType = numMax.DataType = 
                    numStepWidth.DataType=NumericTextBox.NumericDataType.intType;
                    break;
                case FieldType.smallinteger:
                    numMin.DataType = numMax.DataType =
                    numStepWidth.DataType = NumericTextBox.NumericDataType.shortType;
                    break;
                case FieldType.biginteger:
                    numMin.DataType = numMax.DataType =
                    numStepWidth.DataType = NumericTextBox.NumericDataType.longType;
                    break;
                case FieldType.Float:
                    numMin.DataType = numMax.DataType =
                    numStepWidth.DataType = NumericTextBox.NumericDataType.floatType;
                    break;
                case FieldType.Double:
                    numMin.DataType = numMax.DataType =
                    numStepWidth.DataType = NumericTextBox.NumericDataType.doubleType;
                    break;
            }

            object minObj = FunctionFilter.QueryScalar(
                _fc,
                new FunctionFilter("MIN", _renderer.ValueField, "fieldMin"),
                "fieldMin");
            object maxObj = FunctionFilter.QueryScalar(
                _fc,
                new FunctionFilter("MAX", _renderer.ValueField, "fieldMax"),
                "fieldMax");

            if (minObj != null)
                numMin.Double= Convert.ToDouble(minObj);
            if (maxObj != null)
                numMax.Double = Convert.ToDouble(maxObj);

            btnFixStepCount.Checked = true;
            numStepCount.Double = 10;
            numStepWidth.Double = (double)((int)((numMax.Double - numMin.Double) / 10));
        }

        private void btnMinSymbol_Paint(object sender, PaintEventArgs e)
        {
            SymbolPreview.Draw(e.Graphics,
                    new Rectangle(0, 0, btnMinSymbol.Width, btnMinSymbol.Height),
                    _minSymbol);
        }

        //private void btnMidSymbol_Paint(object sender, PaintEventArgs e)
        //{
        //    SymbolPreview.Draw(e.Graphics,
        //            new Rectangle(0, 0, btnMidSymbol.Width, btnMidSymbol.Height),
        //            _midSymbol);
        //}

        private void btnMaxSymbol_Paint(object sender, PaintEventArgs e)
        {
            SymbolPreview.Draw(e.Graphics,
                    new Rectangle(0, 0, btnMaxSymbol.Width, btnMaxSymbol.Height),
                    _maxSymbol);
        }

        private void btnMinSymbol_Click(object sender, EventArgs e)
        {
            FormSymbol dlg = new FormSymbol(_minSymbol);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _minSymbol = dlg.Symbol;
            }
        }

        private void btnMaxSymbol_Click(object sender, EventArgs e)
        {
            FormSymbol dlg = new FormSymbol(_maxSymbol);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _maxSymbol = dlg.Symbol;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (Math.Abs(numMin.Double - numMax.Double) < 1e-10) return;

            if (_renderer == null) return;

            foreach (QuantityRenderer.QuantityClass qC in _renderer.QuantityClasses)
                _renderer.RemoveClass(qC);

            double stepWidth = numStepWidth.Double;
            if (btnFixStepCount.Checked)
                stepWidth = (numMax.Double - numMin.Double) / Math.Max(numStepCount.Double, 1);

            double x;
            ISymbol symbol;
            QuantityRenderer.QuantityClass qClass;
            for (x = numMin.Double; x < numMax.Double - stepWidth; x += stepWidth)
            {
                symbol = AlternateSymbol(_minSymbol, _maxSymbol, (x - numMin.Double) / (numMax.Double - numMin.Double));
                if (symbol == null) continue;

                if (symbol is ILegendItem)
                {
                    ((ILegendItem)symbol).LegendLabel = x.ToString() + " - " + (x + stepWidth).ToString();
                }
                qClass = new QuantityRenderer.QuantityClass(
                    x, x + stepWidth, symbol);
                try
                {
                    _renderer.AddClass(qClass);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            symbol = _maxSymbol.Clone() as ISymbol;
            if (symbol is ILegendItem)
            {
                ((ILegendItem)symbol).LegendLabel = x.ToString() + " - " + (numMax.Double).ToString();
            }

            qClass = new QuantityRenderer.QuantityClass(
                    x, numMax.Double, symbol);

            try
            {
                _renderer.AddClass(qClass);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        internal ISymbol AlternateSymbol(ISymbol from, ISymbol to, double fac)
        {
            if (fac >= 1.0)
                return to.Clone() as ISymbol;


            ISymbol symbol = from.Clone() as ISymbol;
            if (symbol == null) return null;

            if (fac == 0.0) return symbol;

            if (symbol is IPenColor &&
                from is IPenColor && to is IPenColor)
            {
                ((IPenColor)symbol).PenColor = Helper.AlterColor(
                    ((IPenColor)from).PenColor, ((IPenColor)to).PenColor, fac);
            }
            if (symbol is IBrushColor &&
                from is IBrushColor && to is IBrushColor)
            {
                ((IBrushColor)symbol).FillColor = Helper.AlterColor(
                    ((IBrushColor)from).FillColor, ((IBrushColor)to).FillColor, fac);
            }
            if (symbol is IFontColor &&
                from is IFontColor && to is IFontColor)
            {
                ((IFontColor)symbol).FontColor = Helper.AlterColor(
                    ((IFontColor)from).FontColor, ((IFontColor)to).FontColor, fac);
            }
            if (symbol is IPenWidth &&
                from is IPenWidth && to is IPenWidth)
            {
                ((IPenWidth)symbol).PenWidth =
                    (float)((double)((IPenWidth)from).PenWidth + (((IPenWidth)to).PenWidth - ((IPenWidth)from).PenWidth) * fac);
            }
            if (symbol is ISymbolSize &&
                from is ISymbolSize && to is ISymbolSize)
            {
                ((ISymbolSize)symbol).SymbolSize =
                    (float)((double)((ISymbolSize)from).SymbolSize + (((ISymbolSize)to).SymbolSize - ((ISymbolSize)from).SymbolSize) * fac);
            }
            if (symbol is ISymbolWidth &&
                from is ISymbolWidth && to is ISymbolWidth)
            {
                ((ISymbolWidth)symbol).SymbolWidth =
                    (float)((double)((ISymbolWidth)from).SymbolWidth + (((ISymbolWidth)to).SymbolWidth - ((ISymbolWidth)from).SymbolWidth) * fac);
            }
            return symbol;
        }
    }
}
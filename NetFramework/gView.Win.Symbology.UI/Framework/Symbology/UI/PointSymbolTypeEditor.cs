using gView.Framework.Carto.UI;
using gView.Framework.Sys.UI.Extensions;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace gView.Framework.Symbology.UI
{
    internal class PointSymbolTypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService wfes = provider.GetService(
                typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (wfes != null)
            {
                ISymbol symbol = (ISymbol)value;
                if (symbol == null)
                {
                    symbol = new SimplePointSymbol();
                }

                Form_UITypeEditor_Color dlg = new Form_UITypeEditor_Color(wfes, symbol);
                wfes.DropDownControl(dlg.panelSymbol);
                return dlg.Symbol;
            }
            return value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (!(e.Value is ISymbol))
            {
                return;
            }

            using (var bitmap = GraphicsEngine.Current.Engine.CreateBitmap(e.Bounds.Width, e.Bounds.Height))
            using (var canvas = bitmap.CreateCanvas())
            {
                new SymbolPreview(null).Draw(canvas, new GraphicsEngine.CanvasRectangle(0, 0, bitmap.Width, bitmap.Height), (ISymbol)e.Value);
                e.Graphics.DrawImage(bitmap.ToGdiBitmap(), new Point(e.Bounds.X, e.Bounds.Y));
            }
        }
    }
}

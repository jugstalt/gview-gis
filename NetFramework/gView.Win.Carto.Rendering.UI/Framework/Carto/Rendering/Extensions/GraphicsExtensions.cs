using gView.Framework.Carto.UI;
using gView.Framework.Symbology;
using gView.Framework.Sys.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Win.Carto.Rendering.UI.Framework.Carto.Rendering.Extensions
{
    static public class GraphicsExtensions
    {
        static public void DrawSymbol(this Graphics gr, ISymbol symbol, Rectangle rect)
        {
            using (var bitmap = GraphicsEngine.Current.Engine.CreateBitmap(rect.Width, rect.Height))
            using (var canvas = bitmap.CreateCanvas())
            {
                new SymbolPreview(null).Draw(
                    canvas,
                    new GraphicsEngine.CanvasRectangle(0, 0, bitmap.Width, bitmap.Height),
                    symbol, false);

                gr.DrawImage(bitmap.ToGdiBitmap(), new Point(rect.X, rect.Y));
            }
        }
    }
}

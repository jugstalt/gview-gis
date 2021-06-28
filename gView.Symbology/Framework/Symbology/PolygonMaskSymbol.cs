using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.system;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace gView.Framework.Symbology
{
    [gView.Framework.system.RegisterPlugIn("48177A8B-1B3F-480a-87DF-9F7E1DE57D7B")]
    public sealed class PolygonMaskSymbol : LegendItem, IFillSymbol
    {
        #region IFillSymbol Member

        public void FillPath(IDisplay display, GraphicsPath path)
        {
            using (SolidBrush brush = new SolidBrush(display.BackgroundColor))
            {
                display.Canvas.FillPath(brush, path);
            }
        }

        #endregion

        #region ISymbol Member

        public void Draw(IDisplay display, IGeometry geometry)
        {
            Polygon p = new Polygon();
            p.AddRing(display.Envelope.ToPolygon(0)[0]);
            if (geometry is IPolygon)
            {
                for (int i = 0; i < ((IPolygon)geometry).RingCount; i++)
                {
                    p.AddRing(((IPolygon)geometry)[i]);
                }
            }
            else if (geometry is IAggregateGeometry)
            {
                for (int g = 0; g < ((IAggregateGeometry)geometry).GeometryCount; g++)
                {
                    if (((IAggregateGeometry)geometry)[g] is IPolygon)
                    {
                        IPolygon poly = (IPolygon)((IAggregateGeometry)geometry)[g];
                        for (int i = 0; i < poly.RingCount; i++)
                        {
                            p.AddRing(poly[i]);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                return;
            }
            GraphicsPath gp = DisplayOperations.Geometry2GraphicsPath(display, p);
            if (gp != null)
            {
                this.FillPath(display, gp);
                gp.Dispose(); gp = null;
            }
        }

        public void Release()
        {

        }

        public string Name
        {
            get { return "Polygon Mask"; }
        }

        #endregion

        #region IClone2 Member

        public object Clone(CloneOptions options)
        {
            return new PolygonMaskSymbol();
        }

        #endregion

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set { }
        }

        public bool RequireClone()
        {
            return false;
        }

        #endregion
    }
}

using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Symbology.UI;
using gView.Framework.UI;
using System;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace gView.Framework.Symbology
{
    [gView.Framework.system.RegisterPlugIn("C13DF1F1-FB0A-4C47-AF73-05A184880612")]
    public class SymbolDotedLineSymbol : Symbol, ILineSymbol, IPropertyPage, IPersistable
    {
        [Browsable(true)]
        [DisplayName("Symbol")]
        [Category("Point Symbol")]
        [UsePointSymbolPicker()]
        public IPointSymbol PointSymbol
        {
            get; set;
        }

        [Browsable(true)]
        [DisplayName("Symbol")]
        [Category("Line Symbol")]
        [UseLineSymbolPicker()]
        public ILineSymbol LineSymbol
        {
            get; set;
        }

        #region ILineSymbol

        [Browsable(false)]
        public string Name => "Symbol Doted Line";

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode { get; set; }

        public void Draw(IDisplay display, IGeometry geometry)
        {
            if(this.LineSymbol!=null)
            {
                this.LineSymbol.Draw(display, geometry);
            }
            if(this.PointSymbol!=null && geometry is IPolyline)
            {
                double pixelStep = 20.0;
                double step = pixelStep * (display.mapScale / (96 / 0.0254));  // [m]

                var polyline = (IPolyline)geometry;

                foreach(var path in polyline)
                {
                    if (path == null)
                        continue;

                    var length = path.Length;
                    var pointCount = path.PointCount;

                    var pointCollection = length > step ?
                        SpatialAlgorithms.Algorithm.PathPoints(path, step, length, step) :
                        new PointCollection();

                    pointCollection.AddPoint(path[0]);
                    pointCollection.AddPoint(path[path.PointCount - 1]);

                    for (int i = 0; i < pointCount; i++)
                    {
                        if (pointCollection[i] is PointM2)
                        {
                            ((IPointSymbol)this.PointSymbol).Angle =
                                (float)(Convert.ToSingle(((PointM2)pointCollection[i]).M2) * 360.0 / Math.PI);
                        }
                        else
                        {
                            ((IPointSymbol)this.PointSymbol).Angle = 0;
                        }

                        this.PointSymbol.Draw(display, pointCollection[i]);
                    }
                }
            }
        }

        public void DrawPath(IDisplay display, GraphicsPath path)
        {

        }

        public void Release()
        {
            if (this.PointSymbol != null)
            {
                this.PointSymbol.Release();
            }

            if (this.LineSymbol != null)
            {
                this.LineSymbol.Release();
            }
        }

        #endregion

        #region IClone2
        public object Clone(IDisplay display)
        {
            if (display == null)
            {
                return Clone();
            }

            float fac = 1;
            if (display.refScale > 1 && display.mapScale >= 1)
            {
                fac = (float)(display.refScale / display.mapScale);
            }

            if (display.dpi != 96.0)
            {
                fac *= (float)(display.dpi / 96.0);
            }

            SymbolDotedLineSymbol cloneSym = new SymbolDotedLineSymbol();
            if (this.LineSymbol != null)
            {
                cloneSym.LineSymbol = this.LineSymbol.Clone(display) as ILineSymbol;
            }

            if (this.PointSymbol != null)
            {
                cloneSym.PointSymbol = this.PointSymbol.Clone(display) as IPointSymbol;
            }

            cloneSym.LegendLabel = _legendLabel;
            return cloneSym;
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPageObject()
        {
            return null;
        }

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"/gView.Win.Symbology.UI.dll");

            IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Symbology.UI.PropertyForm_SymbolDotedLineSymbol") as IPropertyPanel;
            if (p != null)
            {
                return p.PropertyPanel(this);
            }

            return null;
        }

        #endregion

        #region IPersistable

        public void Load(IPersistStream stream)
        {
            base.Load(stream);

            this.PointSymbol = (IPointSymbol)stream.Load("pointsymbol");
            this.LineSymbol = (ILineSymbol)stream.Load("linesymbol");
        }

        public void Save(IPersistStream stream)
        {
            base.Save(stream);

            if (this.PointSymbol != null)
            {
                stream.Save("pointsymbol", this.PointSymbol);
            }

            if (this.LineSymbol != null)
            {
                stream.Save("linesymbol", this.LineSymbol);
            }
        }

        #endregion
    }
}

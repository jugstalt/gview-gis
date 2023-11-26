using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using System.ComponentModel;

namespace gView.Framework.Symbology
{
    [RegisterPlugInAttribute("E043E059-47E9-42A0-ACF0-FB1012DC8AA2")]
    public sealed class GradientFillSymbol : LegendItemWidthWhithOutlineSymbol,
                                             IFillSymbol,
                                             IPenColor,
                                             IPenDashStyle,
                                             IPenWidth
    {
        public enum GradientRectType { Feature = 0, Display = 1 }

        private ColorGradient _gradient;
        private GradientRectType _rectType = GradientRectType.Feature;

        public GradientFillSymbol()
        {
            _gradient = new ColorGradient(ArgbColor.Red, ArgbColor.Blue);
            _gradient.Angle = 45f;
        }

        public GradientFillSymbol(ColorGradient gradient)
        {
            _gradient = new ColorGradient(gradient);
        }

        ~GradientFillSymbol()
        {
            this.Release();
        }

        public override string ToString()
        {
            return this.Name;
        }

        [Browsable(true)]
        [Category("Fill Symbol")]
        [ReadOnly(true)]
        [UseColorGradientPicker]
        public ColorGradient ColorGradient
        {
            get { return _gradient; }
            set
            {
                if (_gradient != null)
                {
                    _gradient = value;
                }
            }
        }

        [Browsable(true)]
        [Category("Fill Symbol")]
        public GradientRectType GradientRectangle
        {
            get { return _rectType; }
            set { _rectType = value; }
        }

        #region IPenColor Member

        [Browsable(false)]
        public ArgbColor PenColor
        {
            get
            {
                if (OutlineSymbol is IPenColor)
                {
                    return ((IPenColor)OutlineSymbol).PenColor;
                }
                return ArgbColor.Transparent;
            }
            set
            {
                if (OutlineSymbol is IPenColor)
                {
                    ((IPenColor)OutlineSymbol).PenColor = value;
                }
            }
        }

        #endregion

        #region IPenWidth Member

        [Browsable(false)]
        public float PenWidth
        {
            get
            {
                if (OutlineSymbol is IPenWidth)
                {
                    return ((IPenWidth)OutlineSymbol).PenWidth;
                }
                return 0;
            }
            set
            {
                if (OutlineSymbol is IPenWidth)
                {
                    ((IPenWidth)OutlineSymbol).PenWidth = value;
                }
            }
        }

        [Browsable(false)]
        public DrawingUnit PenWidthUnit
        {
            get
            {
                if (OutlineSymbol is IPenWidth)
                {
                    return ((IPenWidth)OutlineSymbol).PenWidthUnit;
                }
                return DrawingUnit.Pixel;
            }
            set
            {
                if (OutlineSymbol is IPenWidth)
                {
                    ((IPenWidth)OutlineSymbol).PenWidthUnit = value;
                }
            }
        }

        [Browsable(true)]
        [Category("Reference Scaling")]
        [UseWidthPicker()]
        public float MaxPenWidth
        {
            get
            {
                if (OutlineSymbol is IPenWidth)
                {
                    return ((IPenWidth)OutlineSymbol).MaxPenWidth;
                }
                return 0;
            }
            set
            {
                if (OutlineSymbol is IPenWidth)
                {
                    ((IPenWidth)OutlineSymbol).MaxPenWidth = value;
                }
            }
        }

        [Browsable(true)]
        [Category("Reference Scaling")]
        [UseWidthPicker()]
        public float MinPenWidth
        {
            get
            {
                if (OutlineSymbol is IPenWidth)
                {
                    return ((IPenWidth)OutlineSymbol).MinPenWidth;
                }
                return 0;
            }
            set
            {
                if (OutlineSymbol is IPenWidth)
                {
                    ((IPenWidth)OutlineSymbol).MinPenWidth = value;
                }
            }
        }

        #endregion

        #region IPenDashStyle Member

        [Browsable(false)]
        public LineDashStyle PenDashStyle
        {
            get
            {
                if (OutlineSymbol is IPenDashStyle)
                {
                    return ((IPenDashStyle)OutlineSymbol).PenDashStyle;
                }
                return LineDashStyle.Solid;
            }
            set
            {
                if (OutlineSymbol is IPenDashStyle)
                {
                    ((IPenDashStyle)OutlineSymbol).PenDashStyle = value;
                }
            }
        }

        #endregion

        #region IFillSymbol Member

        public void FillPath(IDisplay display, IGraphicsPath path)
        {
            //display.GraphicsContext.SmoothingMode = (SmoothingMode)this.Smoothingmode;

            if (_gradient != null)
            {
                CanvasRectangleF rect =
                    (_rectType == GradientRectType.Feature ?
                    path.GetBounds() :
                    new CanvasRectangleF(0, 0, display.ImageWidth, display.ImageHeight));

                using (var brush = _gradient.CreateNewLinearGradientBrush(rect))
                {
                    display.Canvas.FillPath(brush, path);
                }
            }
            //if (OutlineSymbol != null)
            //{
            //    if (OutlineSymbol is ILineSymbol)
            //        ((ILineSymbol)OutlineSymbol).DrawPath(display, path);
            //    else if (OutlineSymbol is SymbolCollection)
            //    {
            //        foreach (SymbolCollectionItem item in ((SymbolCollection)OutlineSymbol).Symbols)
            //        {
            //            if (!item.Visible) continue;
            //            if (item.Symbol is ILineSymbol)
            //            {
            //                ((ILineSymbol)item.Symbol).DrawPath(display, path);
            //            }
            //        }
            //    }
            //}
        }

        #endregion

        #region ISymbol Member

        public bool SupportsGeometryType(GeometryType geomType) => geomType == GeometryType.Polygon;

        public void Draw(IDisplay display, IGeometry geometry)
        {
            var gp = DisplayOperations.Geometry2GraphicsPath(display, geometry);
            if (gp != null)
            {
                this.FillPath(display, gp);

                SimpleFillSymbol.DrawOutlineSymbol(display, OutlineSymbol, geometry, gp);
                gp.Dispose(); gp = null;
            }
        }

        public void Release()
        {
            if (OutlineSymbol != null)
            {
                OutlineSymbol.Release();
                OutlineSymbol = null;
            }
        }


        [Browsable(false)]
        public string Name
        {
            get
            {
                return "Gradient Fill Symbol";
            }
        }

        #endregion

        #region IPersistable Member

        new public void Load(IPersistStream stream)
        {
            base.Load(stream);

            _gradient = (ColorGradient)stream.Load("gradient", _gradient, _gradient);
            _rectType = (GradientRectType)stream.Load("recttype", (int)GradientRectType.Feature);
            OutlineSymbol = (ISymbol)stream.Load("outlinesymbol");
        }

        new public void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("gradient", _gradient);
            stream.Save("recttype", (int)_rectType);
            if (OutlineSymbol != null)
            {
                stream.Save("outlinesymbol", OutlineSymbol);
            }
        }

        #endregion

        #region IClone2
        public object Clone(CloneOptions options)
        {
            var display = options?.Display;

            if (display == null)
            {
                return Clone();
            }

            float fac = 1;
            if (options.ApplyRefScale)
            {
                fac = (float)(display.ReferenceScale / display.MapScale);
                fac = options.RefScaleFactor(fac);
            }

            if (display.Dpi != 96.0)
            {
                fac *= (float)(display.Dpi / 96.0);
            }

            GradientFillSymbol fSym = new GradientFillSymbol(_gradient);
            if (OutlineSymbol != null)
            {
                fSym.OutlineSymbol = (ISymbol)OutlineSymbol.Clone(options);
            }

            fSym._rectType = _rectType;
            fSym.LegendLabel = _legendLabel;
            return fSym;
        }
        #endregion

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmoothingMode
        {
            get => OutlineSymbol != null
               ? OutlineSymbol.SymbolSmoothingMode
               : SymbolSmoothing.None;
            set
            {
                if (OutlineSymbol != null)
                {
                    OutlineSymbol.SymbolSmoothingMode = value;
                }
            }
        }

        public bool RequireClone()
        {
            return OutlineSymbol != null && OutlineSymbol.RequireClone();
        }

        #endregion
    }
}

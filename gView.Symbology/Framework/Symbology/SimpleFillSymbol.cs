using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Symbology.UI;
using gView.Framework.system;
using gView.Framework.UI;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using System.ComponentModel;
using System.Reflection;

namespace gView.Framework.Symbology
{
    [gView.Framework.system.RegisterPlugIn("1496A1A8-8087-4eba-86A0-23FB91197B22")]
    public sealed class SimpleFillSymbol : LegendItem, IFillSymbol, IPropertyPage, IPenColor, IBrushColor, IPenWidth, IPenDashStyle
    {
        private IBrush _brush;
        private ArgbColor _color;
        private ISymbol _outlineSymbol = null;

        public SimpleFillSymbol()
        {
            _color = ArgbColor.Red;
            _brush = Current.Engine.CreateSolidBrush(_color);
        }

        private SimpleFillSymbol(ArgbColor color)
        {
            _color = color;
            _brush = Current.Engine.CreateSolidBrush(_color);
        }

        ~SimpleFillSymbol()
        {
            this.Release();
        }

        public override string ToString()
        {
            return this.Name;
        }

        [Browsable(true)]
        [Category("Fill Symbol")]
        [UseColorPicker()]
        public ArgbColor Color
        {
            get
            {
                return _color;
            }
            set
            {
                _brush.Color = value;
                _color = value;
            }
        }

        [Browsable(true)]
        [DisplayName("Symbol")]
        [Category("Outline Symbol")]
        [UseLineSymbolPicker()]
        public ISymbol OutlineSymbol
        {
            get
            {
                return _outlineSymbol;
            }
            set
            {
                _outlineSymbol = value;
            }
        }

        [Browsable(true)]
        [DisplayName("Smoothingmode")]
        [Category("Outline Symbol")]
        public SymbolSmoothing SmoothingMode
        {
            get
            {
                if (_outlineSymbol is Symbol)
                {
                    return ((Symbol)_outlineSymbol).Smoothingmode;
                }

                return SymbolSmoothing.None;
            }
            set
            {
                if (_outlineSymbol is Symbol)
                {
                    ((Symbol)_outlineSymbol).Smoothingmode = value;
                }
            }
        }

        [Browsable(true)]
        [DisplayName("Color")]
        [Category("Outline Symbol")]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        public ArgbColor OutlineColor
        {
            get { return PenColor; }
            set { PenColor = value; }
        }

        [Browsable(true)]
        [DisplayName("DashStyle")]
        [Category("Outline Symbol")]
        //[Editor(typeof(gView.Framework.UI.DashStyleTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseDashStylePicker()]
        public LineDashStyle OutlineDashStyle
        {
            get { return PenDashStyle; }
            set { PenDashStyle = value; }
        }

        [Browsable(true)]
        [Category("Outline Symbol")]
        [DisplayName("Width")]
        //[Editor(typeof(gView.Framework.UI.PenWidthTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseWidthPicker()]
        public float OutlineWidth
        {
            get { return PenWidth; }
            set { PenWidth = value; }
        }

        public bool SupportsGeometryType(GeometryType geomType) => geomType == GeometryType.Polygon;

        #region IFillSymbol Member

        public void FillPath(IDisplay display, IGraphicsPath path)
        {
            if (_outlineSymbol == null || this.OutlineColor.IsTransparent)
            {
                display.Canvas.SmoothingMode = (SmoothingMode)this.SmoothingMode;
            }

            if (!_color.IsTransparent)
            {
                display.Canvas.FillPath(_brush, path);
            }

            display.Canvas.SmoothingMode = GraphicsEngine.SmoothingMode.None;

            //if (_outlineSymbol != null)
            //{
            //    if (_outlineSymbol is ILineSymbol)
            //    {
            //        ((ILineSymbol)_outlineSymbol).DrawPath(display, path);
            //    }
            //    else if (_outlineSymbol is SymbolCollection)
            //    {
            //        foreach (SymbolCollectionItem item in ((SymbolCollection)_outlineSymbol).Symbols)
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

        public void Draw(IDisplay display, IGeometry geometry)
        {
            var gp = DisplayOperations.Geometry2GraphicsPath(display, geometry);
            if (gp != null)
            {
                this.FillPath(display, gp);

                if (!this.OutlineColor.IsTransparent)
                {
                    SimpleFillSymbol.DrawOutlineSymbol(display, _outlineSymbol, geometry, gp);
                }

                gp.Dispose(); gp = null;
            }
        }

        public void Release()
        {
            if (_brush != null)
            {
                _brush.Dispose();
                _brush = null;
            }
            if (_outlineSymbol != null)
            {
                _outlineSymbol.Release();
            }
        }


        [Browsable(false)]
        public string Name
        {
            get
            {
                return "Fill Symbol";
            }
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

            IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Symbology.UI.PropertyForm_SimpleFillSymbol") as IPropertyPanel;
            if (p != null)
            {
                return p.PropertyPanel(this);
            }

            return null;
        }

        #endregion

        #region IPersistable Member

        new public void Load(IPersistStream stream)
        {
            base.Load(stream);

            this.Color = ArgbColor.FromArgb((int)stream.Load("color", ArgbColor.Red.ToArgb()));
            _outlineSymbol = (ISymbol)stream.Load("outlinesymbol");
        }

        new public void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("color", this.Color.ToArgb());
            if (_outlineSymbol != null)
            {
                stream.Save("outlinesymbol", _outlineSymbol);
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

            SimpleFillSymbol fSym = new SimpleFillSymbol(_brush.Color);
            if (_outlineSymbol != null)
            {
                fSym._outlineSymbol = (ISymbol)_outlineSymbol.Clone(options);
            }

            fSym.LegendLabel = _legendLabel;
            //fSym.Smoothingmode = this.Smoothingmode;
            return fSym;
        }

        #endregion

        #region IPenColor Member

        [Browsable(false)]
        public ArgbColor PenColor
        {
            get
            {
                if (_outlineSymbol is IPenColor)
                {
                    return ((IPenColor)_outlineSymbol).PenColor;
                }
                return ArgbColor.Transparent;
            }
            set
            {
                if (_outlineSymbol is IPenColor)
                {
                    ((IPenColor)_outlineSymbol).PenColor = value;
                }
            }
        }

        #endregion

        #region IBrushColor Member

        [Browsable(false)]
        public ArgbColor FillColor
        {
            get
            {
                return this.Color;
            }
            set
            {
                this.Color = value;
            }
        }

        #endregion

        #region IPenWidth Member

        [Browsable(false)]
        public float PenWidth
        {
            get
            {
                if (_outlineSymbol is IPenWidth)
                {
                    return ((IPenWidth)_outlineSymbol).PenWidth;
                }
                return 0;
            }
            set
            {
                if (_outlineSymbol is IPenWidth)
                {
                    ((IPenWidth)_outlineSymbol).PenWidth = value;
                }
            }
        }

        [Browsable(false)]
        public DrawingUnit PenWidthUnit
        {
            get
            {
                if (_outlineSymbol is IPenWidth)
                {
                    return ((IPenWidth)_outlineSymbol).PenWidthUnit;
                }
                return DrawingUnit.Pixel;
            }
            set
            {
                if (_outlineSymbol is IPenWidth)
                {
                    ((IPenWidth)_outlineSymbol).PenWidthUnit = value;
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
                if (_outlineSymbol is IPenWidth)
                {
                    return ((IPenWidth)_outlineSymbol).MaxPenWidth;
                }

                return 0f;
            }
            set
            {
                if (_outlineSymbol is IPenWidth)
                {
                    ((IPenWidth)_outlineSymbol).MaxPenWidth = value;
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
                if (_outlineSymbol is IPenWidth)
                {
                    return ((IPenWidth)_outlineSymbol).MinPenWidth;
                }

                return 0f;
            }
            set
            {
                if (_outlineSymbol is IPenWidth)
                {
                    ((IPenWidth)_outlineSymbol).MinPenWidth = value;
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
                if (_outlineSymbol is IPenDashStyle)
                {
                    return ((IPenDashStyle)_outlineSymbol).PenDashStyle;
                }
                return LineDashStyle.Solid;
            }
            set
            {
                if (_outlineSymbol is IPenDashStyle)
                {
                    ((IPenDashStyle)_outlineSymbol).PenDashStyle = value;
                }
            }
        }

        #endregion

        public static void DrawOutlineSymbol(IDisplay display, ISymbol outlineSymbol, IGeometry geometry, IGraphicsPath gp)
        {
            #region Überprüfen auf dash!!!

            if (outlineSymbol != null)
            {
                bool isDash = false;
                if (outlineSymbol is IPenDashStyle &&
                    ((IPenDashStyle)outlineSymbol).PenDashStyle != LineDashStyle.Solid)
                {
                    isDash = true;
                }
                else if (outlineSymbol is SymbolCollection)
                {
                    foreach (SymbolCollectionItem item in ((SymbolCollection)outlineSymbol).Symbols)
                    {
                        if (item.Symbol is IPenDashStyle && ((IPenDashStyle)item.Symbol).PenDashStyle != LineDashStyle.Solid)
                        {
                            isDash = true;
                        }
                    }
                }

                if (isDash)
                {
                    if (geometry is IPolygon)
                    {
                        outlineSymbol.Draw(display, new Polyline((IPolygon)geometry));
                    }
                    else
                    {
                        outlineSymbol.Draw(display, geometry);
                    }
                }
                else
                {
                    if (outlineSymbol is ILineSymbol)
                    {
                        ((ILineSymbol)outlineSymbol).DrawPath(display, gp);
                    }
                    else if (outlineSymbol is SymbolCollection)
                    {
                        foreach (SymbolCollectionItem item in ((SymbolCollection)outlineSymbol).Symbols)
                        {
                            if (!item.Visible)
                            {
                                continue;
                            }

                            if (item.Symbol is ILineSymbol)
                            {
                                ((ILineSymbol)item.Symbol).DrawPath(display, gp);
                            }
                        }
                    }
                }
            }

            #endregion
        }

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set
            {
                if (_outlineSymbol != null)
                {
                    _outlineSymbol.SymbolSmothingMode = value;
                }
            }
        }

        public bool RequireClone()
        {
            return _outlineSymbol != null && _outlineSymbol.RequireClone();
        }

        #endregion
    }
}

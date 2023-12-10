#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.Common;
using gView.Framework.Geometry;
using gView.Framework.Symbology.UI;
using gView.Framework.Symbology.UI.Abstractions;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using System.ComponentModel;

namespace gView.Framework.Symbology
{
    [RegisterPlugIn("1496A1A8-8087-4eba-86A0-23FB91197B22")]
    public sealed class SimpleFillSymbol : LegendItemWidthWhithOutlineSymbol,
                                           IFillSymbol,
                                           IPenColor,
                                           IBrushColor,
                                           IPenWidth,
                                           IPenDashStyle,
                                           IQuickSymolPropertyProvider
    {
        private IBrush? _brush;
        private ArgbColor _color;

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
        public ArgbColor Color
        {
            get
            {
                return _color;
            }
            set
            {
                if (_brush != null)
                {
                    _brush.Color = value;
                }
                _color = value;
            }
        }

        public bool SupportsGeometryType(GeometryType geomType) => geomType == GeometryType.Polygon;

        #region IFillSymbol Member

        public void FillPath(IDisplay display, IGraphicsPath path)
        {
            if (OutlineSymbol == null || this.OutlineColor.IsTransparent)
            {
                display.Canvas.SmoothingMode = (SmoothingMode)this.SmoothingMode;
            }

            if (!_color.IsTransparent)
            {
                display.Canvas.FillPath(_brush, path);
            }

            display.Canvas.SmoothingMode = GraphicsEngine.SmoothingMode.None;
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
                    SimpleFillSymbol.DrawOutlineSymbol(display, OutlineSymbol, geometry, gp);
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
            if (OutlineSymbol != null)
            {
                OutlineSymbol.Release();
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

        #region IPersistable Member

        new public void Load(IPersistStream stream)
        {
            base.Load(stream);

            this.Color = ArgbColor.FromArgb((int)stream.Load("color", ArgbColor.Red.ToArgb()));
            OutlineSymbol = (ISymbol)stream.Load("outlinesymbol");
        }

        new public void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("color", this.Color.ToArgb());
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

            SimpleFillSymbol fSym = new SimpleFillSymbol(_brush?.Color ?? ArgbColor.Transparent);
            if (OutlineSymbol != null)
            {
                fSym.OutlineSymbol = (ISymbol)OutlineSymbol.Clone(options);
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

        #region IQuickSymolPropertyProvider

        public IQuickSymbolProperties? GetQuickSymbolProperties()
        {
            var provideQuickProperties = this switch
            {
                { OutlineSymbol: null } => true,
                { Color.IsTransparent: true } => true,
                IOutlineSymbol oSymbol when oSymbol is ISymbolCollection => false,
                IPenColor pColor when pColor.PenColor.EqualBase(this.FillColor) => true,
                _ => false
            };

            return provideQuickProperties
                ? new QuickPolygonSymbolProperties(this)
                : null;

        }

        #endregion
    }
}

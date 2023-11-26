#nullable enable

using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Symbology.UI.Abstractions;
using gView.Framework.Symbology.UI;
using gView.Framework.system;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using System.ComponentModel;

namespace gView.Framework.Symbology
{
    [gView.Framework.system.RegisterPlugIn("E37D7D86-DF11-410f-ADD1-EA89C1E89605")]
    public sealed class HatchSymbol : LegendItemWidthWhithOutlineSymbol,
                                      IFillSymbol,
                                      IPersistable,
                                      IBrushColor,
                                      IPenColor,
                                      IPenWidth,
                                      IPenDashStyle,
                                      IQuickSymolPropertyProvider
    {
        private IBrushCollection? _brush;
        private ArgbColor _forecolor;
        private ArgbColor _backcolor;
        private HatchStyle _hatchStyle;

        public HatchSymbol()
        {
            _forecolor = ArgbColor.Red;
            _backcolor = ArgbColor.Transparent;
            _brush = Current.Engine.CreateHatchBrush(_hatchStyle = HatchStyle.BackwardDiagonal, ForeColor, BackColor);
        }

        private HatchSymbol(ArgbColor foreColor, ArgbColor backColor, HatchStyle hatchStyle)
        {
            _forecolor = foreColor;
            _backcolor = backColor;
            _brush = Current.Engine.CreateHatchBrush(_hatchStyle = hatchStyle, _forecolor, _backcolor);
        }

        ~HatchSymbol()
        {
            this.Release();
        }

        public override string ToString()
        {
            return this.Name;
        }


        [Browsable(true)]
        [Category("Fill Symbol")]
        [UseHatchStylePicker()]
        public HatchStyle HatchStyle
        {
            get
            {
                return _hatchStyle;
            }
            set
            {
                if (_brush != null)
                {
                    _brush.Dispose();
                }

                _brush = Current.Engine.CreateHatchBrush(_hatchStyle = value, ForeColor, BackColor);
            }
        }

        [Browsable(true)]
        [Category("Fill Symbol")]
        [UseColorPicker()]
        public ArgbColor ForeColor
        {
            get
            {
                return _forecolor;
            }
            set
            {
                _forecolor = value;

                HatchStyle hs = this.HatchStyle;
                if (_brush != null)
                {
                    _brush.Dispose();
                }

                _brush = Current.Engine.CreateHatchBrush(hs, ForeColor, BackColor);
            }
        }


        [Browsable(true)]
        [Category("Fill Symbol")]
        [UseColorPicker()]
        public ArgbColor BackColor
        {
            get
            {
                return _backcolor;
            }
            set
            {
                _backcolor = value;

                HatchStyle hs = this.HatchStyle;
                if (_brush != null)
                {
                    _brush.Dispose();
                }

                _brush = Current.Engine.CreateHatchBrush(hs, ForeColor, BackColor);
            }
        }

        #region IFillSymbol Member

        public void FillPath(IDisplay display, IGraphicsPath path)
        {
            if (_forecolor.A > 0 || _backcolor.A > 0)
            {
                display.Canvas.FillPath(_brush, path);
            }
            //if(OutlineSymbol!=null) 
            //{
            //    if(OutlineSymbol is ILineSymbol)
            //        ((ILineSymbol)OutlineSymbol).DrawPath(display,path);
            //    else if(OutlineSymbol is SymbolCollection) 
            //    {
            //        foreach(SymbolCollectionItem item in ((SymbolCollection)OutlineSymbol).Symbols) 
            //        {
            //            if(!item.Visible) continue;
            //            if(item.Symbol is ILineSymbol) 
            //            {
            //                ((ILineSymbol)item.Symbol).DrawPath(display,path);
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
                return "Hatch Symbol";
            }
        }

        #endregion

        #region IPersistable Member

        [Browsable(false)]
        public string PersistID
        {
            get
            {
                return null!;
            }
        }

        new public void Load(IPersistStream stream)
        {
            base.Load(stream);

            this.ForeColor = ArgbColor.FromArgb((int)stream.Load("forecolor", ArgbColor.Red.ToArgb()));
            this.BackColor = ArgbColor.FromArgb((int)stream.Load("backcolor", ArgbColor.Transparent.ToArgb()));
            this.HatchStyle = (HatchStyle)stream.Load("hatchstyle", HatchStyle.Horizontal);
            OutlineSymbol = (ISymbol)stream.Load("outlinesymbol");
        }

        new public void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("forecolor", _forecolor.ToArgb());
            stream.Save("backcolor", _backcolor.ToArgb());
            stream.Save("hatchstyle", (int)_hatchStyle);
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
            if (options?.ApplyRefScale == true)
            {
                fac = (float)(display.ReferenceScale / display.MapScale);
                fac = options.RefScaleFactor(fac);
            }
            fac *= options?.DpiFactor ?? 1;

            HatchSymbol hSym = new HatchSymbol(_forecolor, _backcolor, _hatchStyle);
            if (OutlineSymbol != null)
            {
                hSym.OutlineSymbol = (ISymbol)OutlineSymbol.Clone(options);
            }

            hSym.LegendLabel = _legendLabel;

            return hSym;
        }
        #endregion

        #region IBrushColor Member

        [Browsable(false)]
        public ArgbColor FillColor
        {
            get
            {
                return this.ForeColor;
            }
            set
            {
                this.ForeColor = value;
            }
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

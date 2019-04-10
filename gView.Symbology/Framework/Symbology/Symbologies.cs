using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using gView.Framework;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using gView.Framework.Symbology;
using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.UI;
using gView.Framework.IO;
using gView.Framework.system;
using System.Reflection;
using gView.Framework.Symbology.UI;
using System.IO;
using gView.Symbology.Framework.Symbology.IO;
using System.Threading.Tasks;

namespace gView.Framework.Symbology
{
    internal class DisplayOperations
    {
        public static System.Drawing.Drawing2D.GraphicsPath Geometry2GraphicsPath(IDisplay display, IGeometry geometry)
        {
            try
            {
                if (geometry is IPolygon)
                {
                    return ConvertPolygon(display, (IPolygon)geometry);
                }
                else if (geometry is IPolyline)
                {
                    return ConvertPolyline(display, (IPolyline)geometry);
                }
                else if (geometry is IEnvelope)
                {
                    return ConvertEnvelope(display, (IEnvelope)geometry);
                }
            }
            catch
            {
            }
            return null;
        }

        private static System.Drawing.Drawing2D.GraphicsPath ConvertPolygon(IDisplay display, IPolygon polygon)
        {
            //if (polygon == null || polygon.RingCount == 0)
            //    return null;

            GraphicsPath gp = new GraphicsPath();

            for (int r = 0; r < polygon.RingCount; r++)
            {
                bool first = true;
                int count = 0;
                IRing ring = polygon[r];
                int pCount = ring.PointCount;

                //double o_x = -1e10, o_y = -1e10;
                float o_x = float.MinValue, o_y = float.MinValue;
                gp.StartFigure();
                for (int p = 0; p < pCount; p++)
                {
                    IPoint point = ring[p];
                    double x = point.X, y = point.Y;
                    
                    display.World2Image(ref x, ref y);

                    //
                    // Auf 0.1 Pixel runden, sonst kann es bei fast
                    // horizontalen (vertikalen) Linien zu Fehlern kommen
                    // -> Eine hälfte (beim Bruch) wird nicht mehr gezeichnet
                    //
                    x = Math.Round(x, 1);
                    y = Math.Round(y, 1);

                    if (!((float)o_x == (float)x &&
                        (float)o_y == (float)y))
                    {
                        if (!first)
                        {
                            gp.AddLine(
                                (float)o_x,
                                (float)o_y,
                                (float)x,
                                (float)y);
                            count++;
                        }
                        else
                            first = false;
                    }
                    o_x = (float)x;
                    o_y = (float)y;
                }
                if (count > 0)
                {
                    gp.CloseFigure();
                }
            }

            return gp;
        }
        private static System.Drawing.Drawing2D.GraphicsPath ConvertEnvelope(IDisplay display, IEnvelope envelope)
        {
            GraphicsPath gp = new GraphicsPath();

            double minx = envelope.minx, miny = envelope.miny;
            double maxx = envelope.maxx, maxy = envelope.maxy;
            display.World2Image(ref minx, ref miny);
            display.World2Image(ref maxx, ref maxy);

            gp.StartFigure();
            gp.AddLine((float)minx, (float)miny, (float)maxx, (float)miny);
            gp.AddLine((float)maxx, (float)miny, (float)maxx, (float)maxy);
            gp.AddLine((float)maxx, (float)maxy, (float)minx, (float)maxy);
            gp.CloseFigure();

            return gp;
        }
        private static System.Drawing.Drawing2D.GraphicsPath ConvertPolyline(IDisplay display, IPolyline polyline)
        {
            //if (polyline == null || polyline.PathCount == 0)
            //    return null;

            GraphicsPath gp = new GraphicsPath();
            
            for (int r = 0; r < polyline.PathCount; r++)
            {
                bool first = true;
                int count = 0;
                IPath path = polyline[r];
                int pCount = path.PointCount;

                //double o_x = -1e10, o_y = -1e10;
                float o_x = float.MinValue, o_y = float.MinValue;
                gp.StartFigure();
                for (int p = 0; p < pCount; p++)
                {
                    IPoint point = path[p];
                    double x = point.X, y = point.Y;
                    display.World2Image(ref x, ref y);

                    //
                    // Auf 0.1 Pixel runden, sonst kann es bei fast
                    // horizontalen (vertikalen) Linien zu Fehlern kommen
                    // -> Eine hälfte (beim Bruch) wird nicht mehr gezeichnet
                    //
                    x = Math.Round(x, 1);
                    y = Math.Round(y, 1);

                    if (!((float)o_x == (float)x &&
                          (float)o_y == (float)y))
                    {
                        if (!first)
                        {
                            gp.AddLine(
                                (float)o_x,
                                (float)o_y,
                                (float)x,
                                (float)y);
                            count++;
                        }
                        else
                            first = false;
                    }
                    o_x = (float)x;
                    o_y = (float)y;
                }
                /*
                if(count>0) 
                { 
                    gp.CloseFigure();
                }
                */
            }

            return gp;
        }
    }

    internal class SymbolTransformation
    {
        static public void Transform(float angle, float h, float v, out float x, out float y)
        {
            double c = Math.Cos(((double)angle) * Math.PI / 180.0);
            double s = Math.Sin(((double)angle) * Math.PI / 180.0);

            x = (float)((double)h * c + (double)v * s);
            y = (float)(-(double)h * s + (double)v * c);
        }

        static public float[] Rotate(float angle, float x, float y)
        {
            double c = Math.Cos(((double)angle) * Math.PI / 180.0);
            double s = Math.Sin(((double)angle) * Math.PI / 180.0);

            float[] xx = new float[2];
            xx[0] = (float)((double)x * c + (double)y * s);
            xx[1] = (float)(-(double)x * s + (double)y * c);

            return xx;
        }
    }

    public class LegendItem : Cloner, ILegendItem, IPersistable
    {
        protected string _legendLabel = "";
        protected bool _showInTOC = true;

        #region ILegendInfo Members

        [Browsable(false)]
        public string LegendLabel
        {
            get
            {
                return _legendLabel;
            }
            set
            {
                _legendLabel = value;
            }
        }

        [Browsable(false)]
        public bool ShowInTOC
        {
            get
            {
                return _showInTOC;
            }
            set
            {
                _showInTOC = value;
            }
        }

        [Browsable(false)]
        virtual public int IconHeight
        {
            get { return 0; }
        }

        #endregion

        #region IPersistable Member

        public Task<bool> Load(IPersistStream stream)
        {
            _legendLabel = (string)stream.Load("legendLabel", "");
            _showInTOC = (bool)stream.Load("showInTOC", true);

            return Task.FromResult(true);
        }

        public Task<bool> Save(IPersistStream stream)
        {
            stream.Save("legendLabel", _legendLabel);
            stream.Save("showInTOC", _showInTOC);

            return Task.FromResult(true);
        }

        #endregion
    }

    public class Symbol : LegendItem
    {
        private SymbolSmoothing _smothingMode = SymbolSmoothing.None;

        public Symbol() { }

        [Browsable(true)]
        public SymbolSmoothing Smoothingmode
        {
            get { return _smothingMode; }
            set { _smothingMode = value; }
        }

        async public Task<bool> Load(IPersistStream stream)
        {
            await base.Load(stream);

            this.Smoothingmode = (SymbolSmoothing)stream.Load("smoothing", (int)SymbolSmoothing.None);

            return true;
        }

        async public Task<bool> Save(IPersistStream stream)
        {
            await base.Save(stream);

            stream.Save("smoothing", (int)this.Smoothingmode);

            return true;
        }
    }

    [gView.Framework.system.RegisterPlugIn("D03DE1E2-B2D1-4C25-A39E-29DE828C43BC")]
    public sealed class NullSymbol : INullSymbol
    {
        private geometryType _geomType = geometryType.Unknown;

        public NullSymbol() { }
        public NullSymbol(geometryType geomType)
        {
            _geomType = geomType;
        }

        public void Draw(IDisplay display, IGeometry geometry)
        {
        }

        public void Release()
        {
        }

        public string Name
        {
            get { return "Null Symbol"; }
        }

        public Task<bool> Load(IPersistStream stream)
        {
            _geomType = (geometryType)stream.Load("geomtype", (int)geometryType.Unknown);

            return Task.FromResult(true);
        }

        public Task<bool> Save(IPersistStream stream)
        {
            stream.Save("geomtype", (int)_geomType);

            return Task.FromResult(true);
        }

        public object Clone()
        {
            return this;
        }

        public object Clone(IDisplay display)
        {
            return this;
        }

        public string LegendLabel
        {
            get
            {
                return String.Empty;
            }
            set
            {
            }
        }

        public bool ShowInTOC
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public int IconHeight
        {
            get { return 0; }
        }

        public geometryType GeomtryType
        {
            get { return _geomType; }
            set { _geomType = value; }
        }

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set { }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("062AD1EA-A93C-4c3c-8690-830E65DC6D91")]
    public sealed class SymbolCollection : LegendItem, ISymbolCollection, ISymbol, ILabel, ISymbolRotation, ITextSymbol, IPenColor,IBrushColor,IFontColor
    {
        private List<SymbolCollectionItem> _symbols;

        public SymbolCollection()
        {
            _symbols = new List<SymbolCollectionItem>();
        }

        public SymbolCollection(ISymbol symbol)
            : this()
        {
            AddSymbol(symbol);
        }

        public void AddSymbol(ISymbol symbol)
        {
            AddSymbol(symbol, true);
        }
        public void AddSymbol(ISymbol symbol, bool visible)
        {
            if (!PlugInManager.IsPlugin(symbol)) return;
            if (symbol is SymbolCollection)
            {
                foreach (SymbolCollectionItem item in ((SymbolCollection)symbol).Symbols)
                {
                    _symbols.Add(item);
                }
            }
            else
            {
                _symbols.Add(new SymbolCollectionItem(symbol, visible));
            }
        }

        public void RemoveSymbol(ISymbol symbol)
        {
            SymbolCollectionItem symbolItem = null;
            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol == symbol)
                {
                    symbolItem = item;
                    break;
                }
            }
            if (symbolItem != null) _symbols.Remove(symbolItem);
        }

        public void InsertBefore(ISymbol symbol, ISymbol before, bool visible)
        {
            SymbolCollectionItem beforeItem = null;
            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol == before)
                {
                    beforeItem = item;
                    break;
                }
            }
            if (beforeItem != null)
            {
                int index = _symbols.IndexOf(beforeItem);
                if (index > -1)
                {
                    _symbols.Insert(index, new SymbolCollectionItem(symbol, visible));
                }
            }
        }

        public int IndexOf(ISymbol symbol)
        {
            int index = 0;
            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol == symbol) return index;
                index++;
            }
            return -1;
        }
        public void ReplaceSymbol(ISymbol oldSymbol, ISymbol newSymbol)
        {
            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol == oldSymbol)
                {
                    item.Symbol = newSymbol;
                }
            }
        }
        public bool IsVisible(ISymbol symbol)
        {
            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol == symbol)
                {
                    return item.Visible;
                }
            }
            return false;
        }

        public List<ISymbolCollectionItem> Symbols
        {
            get
            {
                List<ISymbolCollectionItem> list = new List<ISymbolCollectionItem>();
                foreach (SymbolCollectionItem item in _symbols)
                {
                    list.Add(item);
                }
                return list;
            }
        }

        #region ISymbol Member

        public void Release()
        {
            foreach (SymbolCollectionItem sSym in _symbols)
            {
                if (sSym.Symbol == null) continue;
                sSym.Symbol.Release();
            }
            _symbols.Clear();
        }

        public string Name
        {
            get
            {
                return "SymbolCollection";
            }
        }

        public void Draw(IDisplay display, IGeometry geometry)
        {
            foreach (SymbolCollectionItem sSym in _symbols)
            {
                if (sSym.Symbol == null || !sSym.Visible) continue;
                sSym.Symbol.Draw(display, geometry);
            }
        }
        #endregion

        #region ILabel
        public string Text
        {
            get
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ITextSymbol) return ((ITextSymbol)item.Symbol).Text;
                }
                return "";
            }
            set
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ITextSymbol)
                        ((ITextSymbol)item.Symbol).Text = value;
                }
            }
        }
        public TextSymbolAlignment TextSymbolAlignment
        {
            get
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ITextSymbol) return ((ITextSymbol)item.Symbol).TextSymbolAlignment;
                }
                return TextSymbolAlignment.Center;
            }
            set
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ITextSymbol)
                        ((ITextSymbol)item.Symbol).TextSymbolAlignment = value;
                }
            }
        }

        public IDisplayCharacterRanges MeasureCharacterWidth(IDisplay display)
        {
            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol is ILabel) return ((ILabel)item.Symbol).MeasureCharacterWidth(display);
            }
            return null;
        }

        public List<IAnnotationPolygonCollision> AnnotationPolygon(IDisplay display, IGeometry geometry)
        {
            List<IAnnotationPolygonCollision> aPolygons = new List<IAnnotationPolygonCollision>();

            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol is ILabel)
                {
                    List<IAnnotationPolygonCollision> pList = ((ILabel)item.Symbol).AnnotationPolygon(display, geometry);
                    if (pList == null) continue;

                    foreach (AnnotationPolygon aPolygon in pList)
                    {
                        if (aPolygons != null) aPolygons.Add(aPolygon);
                    }
                }
            }

            return aPolygons;
        }

        #endregion

        #region IPersistable Member

        public string PersistID
        {
            get
            {
                return null;
            }
        }

        async public Task<bool> Load(IPersistStream stream)
        {
            await base.Load(stream);
            _symbols.Clear();

            SymbolCollectionItem item;
            while ((item = (SymbolCollectionItem)stream.Load("Item", null, new SymbolCollectionItem(null, false))) != null)
            {
                _symbols.Add(item);
            }

            return true;
        }

        async public Task<bool> Save(IPersistStream stream)
        {
            await base.Save(stream);
            foreach (SymbolCollectionItem item in _symbols)
            {
                stream.Save("Item", item);
            }

            return true;
        }

        #endregion

        #region IClone2
        public object Clone(IDisplay display)
        {
            SymbolCollection collection = new SymbolCollection();
            foreach (SymbolCollectionItem sSym in _symbols)
            {
                collection._symbols.Add(sSym.Clone(display));
            }
            collection.LegendLabel = _legendLabel;

            return collection;
        }
        #endregion

        #region ISymbolRotation Members

        public float Rotation
        {
            get
            {
                return 0;
            }
            set
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ISymbolRotation)
                    {
                        ((ISymbolRotation)item.Symbol).Rotation = value;
                    }
                }
            }
        }

        #endregion

        #region ITextSymbol Member

        public Font Font
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        [Browsable(true)]
        [Category("Reference Scaling")]
        public float MaxFontSize { get; set; }

        [Browsable(true)]
        [Category("Reference Scaling")]
        public float MinFontSize { get; set; }

        #endregion

        #region ISymbolTransformation Member

        public float HorizontalOffset
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        public float VerticalOffset
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        public float Angle
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        #endregion

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set
            {
                if (_symbols != null)
                {
                    foreach (SymbolCollectionItem item in _symbols)
                    {
                        if (item.Symbol != null)
                            item.Symbol.SymbolSmothingMode = value;
                    }
                }
            }
        }

        #endregion

        #region IPenColor Member

        public Color PenColor
        {
            get
            {
                return Color.Black;
            }
            set
            {
                if (_symbols == null)
                    return;

                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is IPenColor)
                        ((IPenColor)item.Symbol).PenColor = value;
                }
            }
        }

        #endregion

        #region IBrushColor Member

        public Color FillColor
        {
            get
            {
                return Color.Black;
            }
            set
            {
                if (_symbols == null)
                    return;

                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is IBrushColor)
                        ((IBrushColor)item.Symbol).FillColor = value;
                }
            }
        }

        #endregion

        #region IFontColor Member

        public Color FontColor
        {
            get
            {
                return Color.Black;
            }
            set
            {
                if (_symbols == null)
                    return;

                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is IFontColor)
                        ((IFontColor)item.Symbol).FontColor = value;
                }
            }
        }

        #endregion
    }

    public sealed class SymbolCollectionItem : ISymbolCollectionItem, IPersistable
    {
        private bool _visible;
        private ISymbol _symbol;

        public SymbolCollectionItem(ISymbol symbol, bool visible)
        {
            Symbol = symbol;
            Visible = visible;
        }
        public SymbolCollectionItem Clone(IDisplay display)
        {
            if (Symbol != null)
            {
                return new SymbolCollectionItem((ISymbol)Symbol.Clone(display), Visible);
            }
            else
            {
                return new SymbolCollectionItem(null, Visible);
            }
        }

        #region IPersistable Member

        public string PersistID
        {
            get
            {
                return null;
            }
        }

        public Task<bool> Load(IPersistStream stream)
        {
            Visible = (bool)stream.Load("visible");
            Symbol = (ISymbol)stream.Load("ISymbol");

            return Task.FromResult(true);
        }

        public Task<bool> Save(IPersistStream stream)
        {
            stream.Save("visible", Visible);
            stream.Save("ISymbol", Symbol);

            return Task.FromResult(true);
        }

        #endregion

        #region ISymbolCollectionItem Member

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public ISymbol Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("F73F40DD-BA55-40b1-B372-99F08B66D2D4")]
    public sealed class SimplePointSymbol : Symbol, IPointSymbol, IPropertyPage, ISymbolRotation, IBrushColor, IPenColor, IPenWidth, ISymbolSize, ISymbolWidth
    {
        public enum MarkerType { Circle = 0, Triangle = 1, Square = 2, Cross = 3, Star = 4 }

        private float _size = 5, _symbolWidth = 0, _angle = 0, _rotation = 0;
        //private IPoint _point=new gView.Framework.Geometry.Point();
        private float _xOffset = 0, _yOffset = 0, _hOffset = 0, _vOffset = 0;
        private SolidBrush _brush;
        private Pen _pen;
        private MarkerType _type = MarkerType.Circle;

        public SimplePointSymbol()
        {
            _brush = new SolidBrush(Color.Blue);
            _pen = new Pen(Color.Black);
        }

        private SimplePointSymbol(Color penColor, float penWidth, Color brushColor)
        {
            _brush = new SolidBrush(brushColor);
            _pen = new Pen(penColor, penWidth);
        }

        [Browsable(true)]
        public float Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = Math.Max(value, 1f);
            }
        }

        [Browsable(true)]
        public float SymbolWidth
        {
            get { return _symbolWidth; }
            set { _symbolWidth = value; }
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor) ,typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        public System.Drawing.Color Color
        {
            get
            {
                return _brush.Color;
            }
            set
            {
                _brush.Color = value;
            }
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.PenWidthTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseWidthPicker()]
        public float OutlineWidth
        {
            get { return _pen.Width; }
            set { _pen.Width = value; }
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        public Color OutlineColor
        {
            get { return _pen.Color; }
            set { _pen.Color = value; }
        }

        [Browsable(true)]
        public MarkerType Marker
        {
            get { return _type; }
            set { _type = value; }
        }

        #region IPointSymbol Member

        [Browsable(false)]
        public float HorizontalOffset
        {
            get { return _hOffset; }
            set
            {
                _hOffset = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        [Browsable(false)]
        public float VerticalOffset
        {
            get { return _vOffset; }
            set
            {
                _vOffset = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        [Browsable(false)]
        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        public void DrawPoint(IDisplay display, IPoint point)
        {
            //float x=(float)point.X,y=(float)point.Y;

            float x = (float)_xOffset - _size / 2;
            float y = (float)_yOffset - _size / 2;

            display.GraphicsContext.SmoothingMode = (SmoothingMode)this.Smoothingmode;

            display.GraphicsContext.TranslateTransform((float)point.X, (float)point.Y);
            display.GraphicsContext.RotateTransform(_angle + _rotation);

            switch (_type)
            {
                case MarkerType.Circle:
                    if (_brush.Color != Color.Transparent)
                        display.GraphicsContext.FillEllipse(_brush, x, y, _size, _size);
                    if (_pen.Color != Color.Transparent)
                        display.GraphicsContext.DrawEllipse(_pen, x, y, _size, _size);
                    break;
                case MarkerType.Triangle:
                    using (GraphicsPath gp = new GraphicsPath())
                    {
                        gp.StartFigure();
                        gp.AddLine(x, .866f * _size + y, _size + x, .866f * _size + y);
                        gp.AddLine(_size + x, .866f * _size + y, _size / 2 + x, y);
                        gp.CloseFigure();

                        if (_brush.Color != Color.Transparent)
                            display.GraphicsContext.FillPath(_brush, gp);
                        if (_pen.Color != Color.Transparent)
                            display.GraphicsContext.DrawPath(_pen, gp);
                    }
                    break;
                case MarkerType.Square:
                    if (_brush.Color != Color.Transparent)
                        display.GraphicsContext.FillRectangle(_brush, x, y, _size, _size);
                    if (_pen.Color != Color.Transparent)
                        display.GraphicsContext.DrawRectangle(_pen, x, y, _size, _size);
                    break;
                case MarkerType.Cross:
                    float sw = _symbolWidth;

                    using (GraphicsPath gp2 = new GraphicsPath())
                    {
                        gp2.StartFigure();
                        gp2.AddLine(x, y + _size / 2 - sw, x, y + _size / 2 + sw);
                        gp2.AddLine(x + _size / 2 - sw, y + _size / 2 + sw, x + _size / 2 - sw, y + _size);
                        gp2.AddLine(x + _size / 2 + sw, y + _size, x + _size / 2 + sw, y + _size / 2 + sw);
                        gp2.AddLine(x + _size, y + _size / 2 + sw, x + _size, y + _size / 2 - sw);
                        gp2.AddLine(x + _size / 2 + sw, y + _size / 2 - sw, x + _size / 2 + sw, y);
                        gp2.AddLine(x + _size / 2 - sw, y, x + _size / 2 - sw, y + _size / 2 - sw);
                        gp2.CloseFigure();

                        if (_brush.Color != Color.Transparent && sw > 0.0)
                            display.GraphicsContext.FillPath(_brush, gp2);
                        if (_pen.Color != Color.Transparent)
                            display.GraphicsContext.DrawPath(_pen, gp2);
                    }
                    break;
                case MarkerType.Star:
                    using (GraphicsPath gp3 = new GraphicsPath())
                    {
                        double w1 = 2.0 * Math.PI / 5.0;
                        double w2 = w1 / 2.0;
                        for (int i = 0; i < 5; i++)
                        {
                            float x1 = _size / 2 + _size / 2 * (float)Math.Sin(w1 * i);
                            float y1 = _size / 2 - _size / 2 * (float)Math.Cos(w1 * i);
                            float x2 = _size / 2 + _size / 5 * (float)Math.Sin(w1 * i + w2);
                            float y2 = _size / 2 - _size / 5 * (float)Math.Cos(w1 * i + w2);
                            gp3.AddLine(x1 + x, y1 + y, x2 + x, y2 + y);
                        }
                        gp3.CloseFigure();

                        if (_brush.Color != Color.Transparent)
                            display.GraphicsContext.FillPath(_brush, gp3);
                        if (_pen.Color != Color.Transparent)
                            display.GraphicsContext.DrawPath(_pen, gp3);
                    }
                    break;
            }
            display.GraphicsContext.ResetTransform();
            display.GraphicsContext.SmoothingMode = SmoothingMode.None;
        }

        #endregion

        #region ISymbol Member

        public void Draw(IDisplay display, IGeometry geometry)
        {
            if (geometry is IPoint)
            {
                double x = ((IPoint)geometry).X;
                double y = ((IPoint)geometry).Y;
                display.World2Image(ref x, ref y);
                IPoint p = new gView.Framework.Geometry.Point(x, y);
                DrawPoint(display, p);
            }
            else if (geometry is IMultiPoint)
            {
                for (int i = 0, to = ((IMultiPoint)geometry).PointCount; i < to; i++)
                {
                    IPoint p = ((IMultiPoint)geometry)[i];
                    Draw(display, p);
                }
            }
        }

        public void Release()
        {
            if (_brush != null) _brush.Dispose();
            _brush = null;
            if (_pen != null) _pen.Dispose();
            _pen = null;
        }

        [Browsable(false)]
        public string Name
        {
            get
            {
                return "Point Symbol";
            }
        }
        #endregion

        #region IPersistable Member

        [Browsable(false)]
        public string PersistID
        {
            get
            {
                return null;
            }
        }

        async public Task<bool> Load(IPersistStream stream)
        {
            await base.Load(stream);

            HorizontalOffset = (float)stream.Load("xOffset", (float)0);
            VerticalOffset = (float)stream.Load("yOffset", (float)0);
            Angle = (float)stream.Load("Angle", (float)0);
            Size = (float)stream.Load("size", (float)5);
            SymbolWidth = (float)stream.Load("symbolWidth", (float)0);
            OutlineWidth = (float)stream.Load("outlinewidth", (float)1);
            Color = Color.FromArgb((int)stream.Load("color", Color.Red.ToArgb()));
            OutlineColor = Color.FromArgb((int)stream.Load("outlinecolor", Color.Black.ToArgb()));
            Marker = (MarkerType)stream.Load("marker", (int)MarkerType.Circle);

            this.MaxPenWidth = (float)stream.Load("maxpenwidth", 0f);
            this.MinPenWidth = (float)stream.Load("minpenwidth", 0f);

            this.MaxSymbolSize = (float)stream.Load("maxsymbolsize", 0f);
            this.MinSymbolSize = (float)stream.Load("minsymbolsize", 0f);

            return true;
        }

        async public Task<bool> Save(IPersistStream stream)
        {
            await base.Save(stream);

            stream.Save("color", this.Color.ToArgb());
            stream.Save("xOffset", HorizontalOffset);
            stream.Save("yOffset", VerticalOffset);
            stream.Save("Angle", Angle);
            stream.Save("size", Size);
            stream.Save("symbolWidth", SymbolWidth);
            stream.Save("outlinewidth", OutlineWidth);
            stream.Save("outlinecolor", OutlineColor.ToArgb());
            stream.Save("marker", (int)_type);

            stream.Save("maxpenwidth", (float)this.MaxPenWidth);
            stream.Save("minpenwidth", (float)this.MinPenWidth);

            stream.Save("maxsymbolsize", this.MaxSymbolSize);
            stream.Save("minsymbolsize", this.MinSymbolSize);

            return true;
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
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Symbology.UI.dll");

            IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Symbology.UI.PropertyForm_SimplePointSymbol") as IPropertyPanel;
            if (p != null)
            {
                return p.PropertyPanel(this);
            }

            return null;
        }

        #endregion

        #region IClone2
        public object Clone(IDisplay display)
        {
            if (display == null) return Clone();
            float fac = 1;

            if (display.refScale > 1)
            {
                fac = ReferenceScaleHelper.RefscaleFactor(
                    (float)(display.refScale / display.mapScale),
                    this.SymbolSize, this.MinSymbolSize, this.MaxSymbolSize);
            }
            if (display.dpi != 96.0)
                fac *= (float)(display.dpi / 96.0);

            SimplePointSymbol pSym = new SimplePointSymbol(_pen.Color, _pen.Width * fac, _brush.Color);
            pSym._size = Math.Max(_size * fac, 1f);
            pSym._symbolWidth = _symbolWidth * fac;
            pSym.Angle = Angle;
            pSym.Marker = _type;
            pSym.HorizontalOffset = HorizontalOffset * fac;
            pSym.VerticalOffset = VerticalOffset * fac;
            pSym.LegendLabel = _legendLabel;

            return pSym;
        }
        #endregion

        #region ISymbolRotation Members

        [Browsable(false)]
        public float Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
            }
        }

        #endregion

        #region IBrushColor Member

        [Browsable(false)]
        public Color FillColor
        {
            get
            {
                if (_brush != null) return _brush.Color;
                return Color.Transparent;
            }
            set
            {
                if (_brush != null) _brush.Color = value;
            }
        }

        #endregion

        #region IPenColor Member

        [Browsable(false)]
        public Color PenColor
        {
            get
            {
                if (_pen != null) return _pen.Color;
                return Color.Transparent;
            }
            set
            {
                if (_pen != null) _pen.Color = value;
            }
        }

        #endregion

        #region IPenWidth Member
        [Browsable(false)]
        public float PenWidth
        {
            get
            {
                if (_pen != null) return _pen.Width;
                return 0f;
            }
            set
            {
                if (_pen != null) _pen.Width = value;
            }
        }

        private float _maxPenWidth, _minPenWidth;

        [Browsable(true)]
        [Category("Reference Scaling")]
        [UseWidthPicker()]
        public float MaxPenWidth
        {
            get
            {
                return _maxPenWidth;
            }
            set
            {
                _maxPenWidth = value;
            }
        }

        [Browsable(true)]
        [Category("Reference Scaling")]
        [UseWidthPicker()]
        public float MinPenWidth
        {
            get
            {
                return _minPenWidth;
            }
            set
            {
                _minPenWidth = value;
            }
        }

        #endregion

        #region ISymbolSize Member

        [Browsable(false)]
        public float SymbolSize
        {
            get
            {
                return this.Size;
            }
            set
            {
                this.Size = value;
            }
        }

        [Browsable(true)]
        [Category("Reference Scaling")]
        public float MaxSymbolSize { get; set; }

        [Browsable(true)]
        [Category("Reference Scaling")]
        public float MinSymbolSize { get; set; }

        #endregion

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set { this.Smoothingmode = value; }
        }

        #endregion
    }


    [gView.Framework.system.RegisterPlugIn("71E22086-D511-4a41-AAE1-BBC78572F277")]
    public sealed class TrueTypeMarkerSymbol : LegendItem, IPropertyPage, IPointSymbol, ISymbolRotation, IFontColor, ISymbolPositioningUI, ISymbolSize
    {
        private float _xOffset = 0, _yOffset = 0, _angle = 0, _rotation = 0, _hOffset = 0, _vOffset = 0;
        private SolidBrush _brush;
        private Font _font;
        private char _char = 'A';

        public TrueTypeMarkerSymbol()
        {
            _brush = new SolidBrush(Color.Black);
            _font = new Font("Arial", 10);
        }
        private TrueTypeMarkerSymbol(Font font, Color color)
        {
            _brush = new SolidBrush(color);
            _font = font;
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        public System.Drawing.Color Color
        {
            get
            {
                return _brush.Color;
            }
            set
            {
                _brush.Color = value;
            }
        }

        public Font Font
        {
            get { return _font; }
            set
            {
                if (_font != null) _font.Dispose();
                _font = value;
            }
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.CharacterTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
        [UseCharacterPicker()]
        public byte Charakter
        {
            get { return (byte)_char; }
            set { _char = (char)value; }
        }

        #region IPointSymbol Member

        public void DrawPoint(IDisplay display, IPoint point)
        {
            if (_font != null)
            {
                //point.X+=_xOffset;
                //point.Y+=_yOffset;

                display.GraphicsContext.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                format.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                display.GraphicsContext.TranslateTransform((float)point.X, (float)point.Y);
                display.GraphicsContext.RotateTransform(_angle + _rotation);

                double xo = _xOffset, yo = _yOffset;
                if (_angle != 0 || _rotation != 0)
                {
                    if (_rotation != 0)
                        SymbolTransformation.Transform(_angle + _rotation, _hOffset, _vOffset, out _xOffset, out _yOffset);

                    double cos_a = Math.Cos(((double)(-_angle - _rotation)) / 180.0 * Math.PI);
                    double sin_a = Math.Sin(((double)(-_angle - _rotation)) / 180.0 * Math.PI);

                    xo = (double)(_xOffset) * cos_a + (double)(_yOffset) * sin_a;
                    yo = -(double)(_xOffset) * sin_a + (double)(_yOffset) * cos_a;
                }

                display.GraphicsContext.DrawString(_char.ToString(), _font, _brush, (float)xo, (float)yo, format);
                display.GraphicsContext.ResetTransform();

                // Rotationspunkt muss gegen den Uhrzeiger mitbewegen,
                // damit sich Font im Uhrzeigersinn um das Zentrum des Fadenkreuzes
                // bewegt...
                //display.GraphicsContext.TranslateTransform((float)point.X, (float)point.Y);
                //display.GraphicsContext.RotateTransform(_angle + _rotation);

                //display.GraphicsContext.FillEllipse(Brushes.Red, (float)xo - 3, (float)yo - 3, 6, 6);
                //display.GraphicsContext.DrawEllipse(Pens.Black, (float)xo - 3, (float)yo - 3, 6, 6);

                //display.GraphicsContext.ResetTransform();
            }
        }

        [Browsable(false)]
        public float HorizontalOffset
        {
            get { return _hOffset; }
            set
            {
                _hOffset = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        [Browsable(false)]
        public float VerticalOffset
        {
            get { return _vOffset; }
            set
            {
                _vOffset = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        [Browsable(false)]
        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }
        #endregion

        #region ISymbol Member

        public void Release()
        {
            if (_brush != null) _brush.Dispose();
            _brush = null;
            if (_font != null) _font.Dispose();
            _font = null;
        }

        [Browsable(false)]
        public string Name
        {
            get
            {
                return "True Type Marker Symbol";
            }
        }

        public void Draw(IDisplay display, IGeometry geometry)
        {
            if (geometry is IPoint)
            {
                double x = ((IPoint)geometry).X;
                double y = ((IPoint)geometry).Y;
                display.World2Image(ref x, ref y);
                IPoint p = new gView.Framework.Geometry.Point(x, y);
                DrawPoint(display, p);
            }
            else if (geometry is IMultiPoint)
            {
                for (int i = 0, to=((IMultiPoint)geometry).PointCount; i < to; i++)
                {
                    IPoint p = ((IMultiPoint)geometry)[i];
                    Draw(display, p);
                }
            }
        }

        #endregion

        #region IPersistable Member

        async public Task<bool> Load(IPersistStream stream)
        {
            await base.Load(stream);

            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ASCIIEncoding encoder = new ASCIIEncoding();
                string soap = (string)stream.Load("font");

                // 
                // Im Size muss Punkt und Komma mit Systemeinstellungen
                // übereinstimmen
                //
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(soap);
                XmlNode sizeNode = doc.SelectSingleNode("//Size");
                if (sizeNode != null) sizeNode.InnerText = sizeNode.InnerText.Replace(".", ",");
                soap = doc.OuterXml;
                //
                //
                //

                ms.Write(encoder.GetBytes(soap), 0, soap.Length);
                ms.Position = 0;
                SoapFormatter formatter = new SoapFormatter();
                _font = (Font)formatter.Deserialize<Font>(ms);
            }
            catch { }

            _char = (char)stream.Load("char");
            _brush.Color = Color.FromArgb((int)stream.Load("color"));
            HorizontalOffset = (float)stream.Load("x");
            VerticalOffset = (float)stream.Load("y");
            Angle = (float)stream.Load("a");

            this.MaxSymbolSize = (float)stream.Load("maxsymbolsize", 0f);
            this.MinSymbolSize = (float)stream.Load("minsymbolsize", 0f);

            return true;
        }

        async public Task<bool> Save(IPersistStream stream)
        {
            await base.Save(stream);

            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                SoapFormatter formatter = new SoapFormatter();
                formatter.Serialize(ms, _font);
                ms.Position = 0;
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] b = new byte[ms.Length];
                ms.Read(b, 0, b.Length);
                string soap = encoder.GetString(b);

                stream.Save("font", soap);
            }
            catch { }
            stream.Save("char", _char);
            stream.Save("color", _brush.Color.ToArgb());
            stream.Save("x", HorizontalOffset);
            stream.Save("y", VerticalOffset);
            stream.Save("a", Angle);

            stream.Save("maxsymbolsize", this.MaxSymbolSize);
            stream.Save("minsymbolsize", this.MinSymbolSize);

            return true;
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
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Symbology.UI.dll");

            IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Symbology.UI.PropertyForm_SimplePointSymbol") as IPropertyPanel;
            if (p != null)
            {
                return p.PropertyPanel(this);
            }

            return null;
        }

        #endregion

        #region IClone2
        public object Clone(IDisplay display)
        {
            if (display == null) return Clone();
            float fac = 1;
            if (display.refScale > 1)
            {
                fac = ReferenceScaleHelper.RefscaleFactor(
                    (float)(display.refScale / display.mapScale),
                    this.SymbolSize, this.MinSymbolSize, this.MaxSymbolSize);
            }
            if (display.dpi != 96.0)
                fac *= (float)(display.dpi / 96.0);

            TrueTypeMarkerSymbol marker = new TrueTypeMarkerSymbol(new Font(Font.Name, Math.Max(Font.Size * fac / display.Screen.LargeFontsFactor, 2f), _font.Style), _brush.Color);
            marker.Angle = Angle;
            marker.HorizontalOffset = HorizontalOffset * fac;
            marker.VerticalOffset = VerticalOffset * fac;

            marker._char = _char;
            marker.LegendLabel = _legendLabel;

            return marker;
        }
        #endregion

        #region ISymbolRotation Members

        [Browsable(false)]
        public float Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
            }
        }

        #endregion

        #region IFontColor Member

        [Browsable(false)]
        public Color FontColor
        {
            get
            {
                if (_brush != null) return _brush.Color;
                return Color.Transparent;
            }
            set
            {
                if (_brush != null) _brush.Color = value;
            }
        }

        #endregion

        #region ISymbolPositioningUI Member

        public void HorizontalMove(float x)
        {
            float[] c = SymbolTransformation.Rotate(-this.Angle, HorizontalOffset, VerticalOffset);
            c[0] += x;
            c = SymbolTransformation.Rotate(this.Angle, c[0], c[1]);
            this.HorizontalOffset = c[0];
            this.VerticalOffset = c[1];
        }

        public void VertiacalMove(float y)
        {
            float[] c = SymbolTransformation.Rotate(-this.Angle, HorizontalOffset, VerticalOffset);
            c[1] += y;
            c = SymbolTransformation.Rotate(this.Angle, c[0], c[1]);
            this.HorizontalOffset = c[0];
            this.VerticalOffset = c[1];
        }

        #endregion

        #region ISymbolSize Member

        [Browsable(false)]
        public float SymbolSize
        {
            get
            {
                if (_font != null) return _font.Size;
                return 0;
            }
            set
            {
                this.Font = new Font(
                    (_font != null ? _font.FontFamily.Name : "Arial"), value);
            }
        }

        [Browsable(true)]
        [Category("Reference Scaling")]
        public float MaxSymbolSize { get; set; }

        [Browsable(true)]
        [Category("Reference Scaling")]
        public float MinSymbolSize { get; set; }

        #endregion

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set { }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("230881F2-F9E4-4593-BD25-5B614B9CB503")]
    public sealed class RasterMarkerSymbol : LegendItem, IPropertyPage, IPointSymbol, ISymbolRotation
    {
        private float _xOffset = 0, _yOffset = 0, _angle = 0, _rotation = 0, _hOffset = 0, _vOffset = 0;
        private float _sizeX = 10f, _sizeY = 10f;
        private string _filename = String.Empty;
        private Image _image = null;

        [UseFilePicker()]
        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
            }
        }

        public float SizeX
        {
            get { return _sizeX; }
            set { _sizeX = value; }
        }
        public float SizeY
        {
            get { return _sizeY; }
            set { _sizeY = value; }
        }

        #region IPropertyPage Member

        public object PropertyPageObject()
        {
            return null;
        }

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Symbology.UI.dll");

            IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Symbology.UI.PropertyForm_SimplePointSymbol") as IPropertyPanel;
            if (p != null)
            {
                return p.PropertyPanel(this);
            }

            return null;
        }

        #endregion

        #region IPointSymbol Member

        public void DrawPoint(IDisplay display, IPoint point)
        {
            if (!String.IsNullOrEmpty(_filename))
            {
                float x = (float)_xOffset - _sizeX / 2;
                float y = (float)_yOffset - _sizeY / 2;

                display.GraphicsContext.TranslateTransform((float)point.X, (float)point.Y);
                display.GraphicsContext.RotateTransform(_angle + _rotation);

                try
                {
                    if (_image == null) _image = Image.FromFile(_filename);
                    display.GraphicsContext.DrawImage(
                            _image,
                            new Rectangle((int)x, (int)y,
                            (int)_sizeX, (int)_sizeY),
                            new Rectangle(0, 0, _image.Width, _image.Height),
                            GraphicsUnit.Pixel);

                }
                catch
                {
                }
                display.GraphicsContext.ResetTransform();
            }
        }

        #endregion

        #region ISymbol Member

        public void Draw(IDisplay display, IGeometry geometry)
        {
            if (display != null && geometry is IPoint)
            {
                double x = ((IPoint)geometry).X;
                double y = ((IPoint)geometry).Y;
                display.World2Image(ref x, ref y);
                IPoint p = new gView.Framework.Geometry.Point(x, y);
                DrawPoint(display, p);
            }
            else if (geometry is IMultiPoint)
            {
                for (int i = 0, to = ((IMultiPoint)geometry).PointCount; i < to; i++)
                {
                    IPoint p = ((IMultiPoint)geometry)[i];
                    Draw(display, p);
                }
            }
        }

        public void Release()
        {
            if (_image != null)
            {
                _image.Dispose();
                _image = null;
            }
        }

        [Browsable(false)]
        public string Name
        {
            get { return "Raster Marker Symbol"; }
        }

        #endregion

        #region IClone2 Member

        public object Clone(IDisplay display)
        {
            if (display == null) return Clone();
            float fac = 1;
            if (display.refScale > 1)
                fac = (float)(display.refScale / display.mapScale);
            if (display.dpi != 96.0)
                fac *= (float)(display.dpi / 96.0);

            RasterMarkerSymbol marker = new RasterMarkerSymbol();
            marker.Angle = Angle;
            marker.HorizontalOffset = HorizontalOffset * fac;
            marker.VerticalOffset = VerticalOffset * fac;
            marker.SizeX = _sizeX * fac;
            marker.SizeY = _sizeY * fac;
            marker.Filename = _filename;
            marker.LegendLabel = _legendLabel;

            return marker;
        }

        #endregion

        #region ISymbolTransformation Member

        [Browsable(false)]
        public float HorizontalOffset
        {
            get { return _hOffset; }
            set
            {
                _hOffset = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        [Browsable(false)]
        public float VerticalOffset
        {
            get { return _vOffset; }
            set
            {
                _vOffset = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        [Browsable(false)]
        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        #endregion

        #region IPersistable Member

        async public Task<bool> Load(IPersistStream stream)
        {
            await base.Load(stream);

            Filename = (string)stream.Load("fn", String.Empty);
            HorizontalOffset = (float)stream.Load("x");
            VerticalOffset = (float)stream.Load("y");
            Angle = (float)stream.Load("a");
            SizeX = (float)stream.Load("sx", 10f);
            SizeY = (float)stream.Load("sy", 10f);

            return true;
        }

        async public Task<bool> Save(IPersistStream stream)
        {
            await base.Save(stream);

            stream.Save("fn", Filename);
            stream.Save("x", HorizontalOffset);
            stream.Save("y", VerticalOffset);
            stream.Save("a", Angle);
            stream.Save("sx", _sizeX);
            stream.Save("sy", _sizeY);

            return true;
        }

        #endregion

        #region ISymbolRotation Member

        [Browsable(false)]
        public float Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
            }
        }

        #endregion

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set { }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("91CC3F6F-0EC5-42b7-AA34-9C89803118E7")]
    public sealed class SimpleLineSymbol : Symbol, ILineSymbol, IPropertyPage, IPenColor, IPenWidth, IPenDashStyle
    {
        private Pen _pen;
        private Color _color;

        public SimpleLineSymbol()
        {
            _color = Color.Black;
            _pen = new Pen(_color, 1);
            _pen.LineJoin = LineJoin.Round;
        }

        private SimpleLineSymbol(Color color, float width)
        {
            _color = color;
            _pen = new Pen(_color, width);
            _pen.LineJoin = LineJoin.Round;
        }

        ~SimpleLineSymbol()
        {
            this.Release();
        }

        public override string ToString()
        {
            return this.Name;
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.DashStyleTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
        [UseDashStylePicker()]
        public DashStyle DashStyle
        {
            get
            {
                return _pen.DashStyle;
            }
            set
            {
                _pen.DashStyle = value;
            }
        }

        public System.Drawing.Drawing2D.LineCap LineStartCap
        {
            get
            {
                return _pen.StartCap;
            }
            set
            {
                _pen.StartCap = value;
            }
        }

        public System.Drawing.Drawing2D.LineCap LineEndCap
        {
            get
            {
                return _pen.EndCap;
            }
            set
            {
                _pen.EndCap = value;
            }
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.PenWidthTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseWidthPicker()]
        public float Width
        {
            get
            {
                return _pen.Width;
            }
            set
            {
                if (_pen == null)
                {
                    _pen = new Pen(_color, value);
                }
                else
                {
                    _pen.Width = value;
                }
            }
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        public System.Drawing.Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _pen.Color = value;
                _color = value;
            }
        }

        #region ILineSymbol Member

        public void DrawPath(IDisplay display, System.Drawing.Drawing2D.GraphicsPath path)
        {
            if (path != null)
            {
                display.GraphicsContext.SmoothingMode = (SmoothingMode)this.Smoothingmode;
                display.GraphicsContext.DrawPath(_pen, path);
                display.GraphicsContext.SmoothingMode = SmoothingMode.None;
            }
        }

        #endregion

        #region ISymbol Member

        public void Draw(IDisplay display, IGeometry geometry)
        {
            // Wenn DashStyle nicht Solid (und Antialiasing) soll Geometry erst geclippt werden,
            // da es sonst zu extrem Zeitaufwendigen Graphikopertation kommt...

            if (this.DashStyle != DashStyle.Solid &&
                this.Smoothingmode != SymbolSmoothing.None)
            {
                Envelope dispEnvelope = new Envelope(display.Envelope);
                //dispEnvelope.Raise(75);
                geometry = gView.Framework.SpatialAlgorithms.Clip.PerformClip(dispEnvelope, geometry);
                if (geometry == null) return;

                //GraphicsPath gp2 = DisplayOperations.Geometry2GraphicsPath(display, dispEnvelope);
                //if (gp2 != null)
                //{
                //    this.DrawPath(display, gp2);
                //    gp2.Dispose(); gp2 = null;
                //}
            }

            GraphicsPath gp = DisplayOperations.Geometry2GraphicsPath(display, geometry);
            if (gp != null)
            {
                if (this.LineStartCap == System.Drawing.Drawing2D.LineCap.ArrowAnchor ||
                    this.LineEndCap == System.Drawing.Drawing2D.LineCap.ArrowAnchor)
                {
                    //
                    // bei LineCap Arrow (Pfeil...) kann es bei sehr kurzen Linen
                    // zu einer Out of Memory Exception kommen...
                    //
                    try
                    {
                        this.DrawPath(display, gp);
                    }
                    catch
                    {
                        LineCap sCap = this.LineStartCap;
                        LineCap eCap = this.LineEndCap;
                        this.LineStartCap = (sCap == System.Drawing.Drawing2D.LineCap.ArrowAnchor) ? System.Drawing.Drawing2D.LineCap.Triangle : sCap;
                        this.LineEndCap = (eCap == System.Drawing.Drawing2D.LineCap.ArrowAnchor) ? System.Drawing.Drawing2D.LineCap.Triangle : eCap;

                        this.DrawPath(display, gp);

                        this.LineStartCap = sCap;
                        this.LineEndCap = eCap;
                    }
                }
                else
                {
                    this.DrawPath(display, gp);
                }
                gp.Dispose(); gp = null;
            }
        }

        public void Release()
        {
            if (_pen != null)
            {
                _pen.Dispose();
                _pen = null;
            }
        }


        [Browsable(false)]
        public string Name
        {
            get
            {
                return "Line Symbol";
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
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Symbology.UI.dll");

            IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Symbology.UI.PropertyForm_SimpleLineSymbol") as IPropertyPanel;
            if (p != null)
            {
                return p.PropertyPanel(this);
            }

            return null;
        }

        #endregion

        #region IPersistable Member

        [Browsable(false)]
        public string PersistID
        {
            get
            {
                return null;
            }
        }

        async public Task<bool> Load(IPersistStream stream)
        {
            await base.Load(stream);

            this.Color = Color.FromArgb((int)stream.Load("color", Color.Black.ToArgb()));
            this.Width = (float)stream.Load("width", (float)1);
            this.DashStyle = (DashStyle)stream.Load("dashstyle", DashStyle.Solid);

            this.LineStartCap = (LineCap)stream.Load("linescap", LineCap.Flat);
            this.LineEndCap = (LineCap)stream.Load("lineecap", LineCap.Flat);

            int cap_old = (int)stream.Load("linecap", (int)-1);
            if (cap_old >= 0)
            {
                this.LineStartCap = this.LineEndCap = (LineCap)cap_old;
            }

            this.MaxPenWidth= (float)stream.Load("maxwidth", 0f);
            this.MinPenWidth = (float)stream.Load("minwidth", 0f);

            return true;
        }

        async public Task<bool> Save(IPersistStream stream)
        {
            await base.Save(stream);

            stream.Save("color", this.Color.ToArgb());
            stream.Save("width", this.Width);
            stream.Save("dashstyle", (int)this.DashStyle);
            stream.Save("linescap", (int)this.LineStartCap);
            stream.Save("lineecap", (int)this.LineEndCap);

            stream.Save("maxwidth", (float)this.MaxPenWidth);
            stream.Save("minwidth", (float)this.MinPenWidth);

            return true;
        }

        #endregion

        #region IClone2
        public object Clone(IDisplay display)
        {
            if (display == null) return Clone();
            float fac = 1;
            if (display.refScale > 1)
                fac = (float)(display.refScale / display.mapScale);
            if (display.dpi != 96.0)
                fac *= (float)(display.dpi / 96.0);

            SimpleLineSymbol clone = new SimpleLineSymbol(_color, ReferenceScaleHelper.PenWidth(_pen.Width * fac, this, display));
            clone.DashStyle = this.DashStyle;
            clone.LineStartCap = this.LineStartCap;
            clone.LineEndCap = this.LineEndCap;
            clone.Smoothingmode = this.Smoothingmode;
            clone.LegendLabel = _legendLabel;
            return clone;
        }
        #endregion

        #region IPenColor Member

        [Browsable(false)]
        public Color PenColor
        {
            get
            {
                return Color;
            }
            set
            {
                Color = value;
            }
        }

        #endregion

        #region IPenWidth Member

        [Browsable(false)]
        public float PenWidth
        {
            get
            {
                return this.Width;
            }
            set
            {
                this.Width = value;
            }
        }

        private float _maxWidth, _minWidth;

        [Browsable(true)]
        [Category("Reference Scaling")]
        [UseWidthPicker()]
        public float MaxPenWidth
        {
            get
            {
                return _maxWidth;
            }
            set
            {
                _maxWidth = value;
            }
        }

        [Browsable(true)]
        [Category("Reference Scaling")]
        [UseWidthPicker()]
        public float MinPenWidth
        {
            get
            {
                return _minWidth;
            }
            set
            {
                _minWidth = value;
            }
        }

        #endregion

        #region IPenDashStyle Member

        [Browsable(false)]
        public DashStyle PenDashStyle
        {
            get
            {
                return this.DashStyle;
            }
            set
            {
                this.DashStyle = value;
            }
        }

        #endregion

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set { this.Smoothingmode = value; }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("1496A1A8-8087-4eba-86A0-23FB91197B22")]
    public sealed class SimpleFillSymbol : LegendItem, IFillSymbol, IPropertyPage, IPenColor, IBrushColor, IPenWidth, IPenDashStyle
    {
        private SolidBrush _brush;
        private Color _color;
        private ISymbol _outlineSymbol = null;

        public SimpleFillSymbol()
        {
            _color = Color.Red;
            _brush = new SolidBrush(_color);
        }

        private SimpleFillSymbol(Color color)
        {
            _color = color;
            _brush = new SolidBrush(_color);
        }

        ~SimpleFillSymbol()
        {
            this.Release();
        }


        [Browsable(true)]
        [Category("Fill Symbol")]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        public System.Drawing.Color Color
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
        //[Editor(typeof(gView.Framework.UI.LineSymbolTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
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
                    return ((Symbol)_outlineSymbol).Smoothingmode;

                return SymbolSmoothing.None;
            }
            set
            {
                if (_outlineSymbol is Symbol)
                    ((Symbol)_outlineSymbol).Smoothingmode = value;
            }
        }

        [Browsable(true)]
        [DisplayName("Color")]
        [Category("Outline Symbol")]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        public Color OutlineColor
        {
            get { return PenColor; }
            set { PenColor = value; }
        }

        [Browsable(true)]
        [DisplayName("DashStyle")]
        [Category("Outline Symbol")]
        //[Editor(typeof(gView.Framework.UI.DashStyleTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseDashStylePicker()]
        public DashStyle OutlineDashStyle
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

        #region IFillSymbol Member

        public void FillPath(IDisplay display, System.Drawing.Drawing2D.GraphicsPath path)
        {
            if (_outlineSymbol == null || this.OutlineColor == null || this.OutlineColor.A == 0)
                display.GraphicsContext.SmoothingMode = (SmoothingMode)this.SmoothingMode;

            if (_color.A > 0) display.GraphicsContext.FillPath(_brush, path);
            display.GraphicsContext.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

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
            GraphicsPath gp = DisplayOperations.Geometry2GraphicsPath(display, geometry);
            if (gp != null)
            {
                this.FillPath(display, gp);

                if (this.OutlineColor != null && this.OutlineColor.A > 0)
                    SimpleFillSymbol.DrawOutlineSymbol(display, _outlineSymbol, geometry, gp);
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
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Symbology.UI.dll");

            IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Symbology.UI.PropertyForm_SimpleFillSymbol") as IPropertyPanel;
            if (p != null)
            {
                return p.PropertyPanel(this);
            }

            return null;
        }

        #endregion

        #region IPersistable Member

        async public Task<bool> Load(IPersistStream stream)
        {
            await base.Load(stream);

            this.Color = Color.FromArgb((int)stream.Load("color", Color.Red.ToArgb()));
            _outlineSymbol = (ISymbol)stream.Load("outlinesymbol");

            return true;
        }

        async public Task<bool> Save(IPersistStream stream)
        {
            await base.Save(stream);

            stream.Save("color", this.Color.ToArgb());
            if (_outlineSymbol != null) stream.Save("outlinesymbol", _outlineSymbol);

            return true;
        }

        #endregion

        #region IClone2
        public object Clone(IDisplay display)
        {
            if (display == null) return Clone();
            float fac = 1;
            if (display.refScale > 1)
                fac = (float)(display.refScale / display.mapScale);
            if (display.dpi != 96.0)
                fac *= (float)(display.dpi / 96.0);

            SimpleFillSymbol fSym = new SimpleFillSymbol(_brush.Color);
            if (_outlineSymbol != null)
                fSym._outlineSymbol = (ISymbol)_outlineSymbol.Clone(display);

            fSym.LegendLabel = _legendLabel;
            //fSym.Smoothingmode = this.Smoothingmode;
            return fSym;
        }
        #endregion

        #region IPenColor Member

        [Browsable(false)]
        public Color PenColor
        {
            get
            {
                if (_outlineSymbol is IPenColor)
                {
                    return ((IPenColor)_outlineSymbol).PenColor;
                }
                return Color.Transparent;
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
        public Color FillColor
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

        private float _maxPenWidth, _minPenWidth;

        [Browsable(true)]
        [Category("Reference Scaling")]
        [UseWidthPicker()]
        public float MaxPenWidth
        {
            get
            {
                if(_outlineSymbol is IPenWidth)
                    return ((IPenWidth)_outlineSymbol).MaxPenWidth;
                return 0f;
            }
            set
            {
                if (_outlineSymbol is IPenWidth)
                    ((IPenWidth)_outlineSymbol).MaxPenWidth = value;
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
                    return ((IPenWidth)_outlineSymbol).MinPenWidth;
                return 0f;
            }
            set
            {
                if (_outlineSymbol is IPenWidth)
                    ((IPenWidth)_outlineSymbol).MinPenWidth = value;
            }
        }

        #endregion

        #region IPenDashStyle Member

        [Browsable(false)]
        public DashStyle PenDashStyle
        {
            get
            {
                if (_outlineSymbol is IPenDashStyle)
                {
                    return ((IPenDashStyle)_outlineSymbol).PenDashStyle;
                }
                return DashStyle.Solid;
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

        public static void DrawOutlineSymbol(IDisplay display, ISymbol outlineSymbol, IGeometry geometry, GraphicsPath gp)
        {
            #region Überprüfen auf dash!!!
            if (outlineSymbol != null)
            {
                bool isDash = false;
                if (outlineSymbol is IPenDashStyle &&
                    ((IPenDashStyle)outlineSymbol).PenDashStyle != DashStyle.Solid)
                {
                    isDash = true;
                }
                else if (outlineSymbol is SymbolCollection)
                {
                    foreach (SymbolCollectionItem item in ((SymbolCollection)outlineSymbol).Symbols)
                    {
                        if (item.Symbol is IPenDashStyle && ((IPenDashStyle)item.Symbol).PenDashStyle != DashStyle.Solid)
                            isDash = true;
                    }
                }

                if (isDash)
                {
                    if (geometry is IPolygon)
                        outlineSymbol.Draw(display, new Polyline((IPolygon)geometry));
                    else
                        outlineSymbol.Draw(display, geometry);
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
                            if (!item.Visible) continue;
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
                    _outlineSymbol.SymbolSmothingMode = value;
            }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("E37D7D86-DF11-410f-ADD1-EA89C1E89605")]
    public sealed class HatchSymbol : LegendItem, IFillSymbol, IPersistable, IPropertyPage, IBrushColor, IPenColor, IPenWidth, IPenDashStyle
    {
        private HatchBrush _brush;
        private Color _forecolor;
        private Color _backcolor;
        private ISymbol _outlineSymbol = null;

        public HatchSymbol()
        {
            _forecolor = Color.Red;
            _backcolor = Color.Transparent;
            _brush = new HatchBrush(HatchStyle.BackwardDiagonal, ForeColor, BackColor);
        }

        private HatchSymbol(Color foreColor, Color backColor, HatchStyle hatchStyle)
        {
            _forecolor = foreColor;
            _backcolor = backColor;
            _brush = new HatchBrush(hatchStyle, _forecolor, _backcolor);
        }

        ~HatchSymbol()
        {
            this.Release();
        }


        [Browsable(true)]
        [Category("Fill Symbol")]
        //[Editor(typeof(gView.Framework.UI.HatchStyleTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
        [UseHatchStylePicker()]
        public HatchStyle HatchStyle
        {
            get
            {
                if (_brush == null) return HatchStyle.Cross;
                return _brush.HatchStyle;
            }
            set
            {
                if (_brush != null) _brush.Dispose();
                _brush = new HatchBrush(value, ForeColor, BackColor);
            }
        }

        [Browsable(true)]
        [Category("Fill Symbol")]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        public Color ForeColor
        {
            get
            {
                return _forecolor;
            }
            set
            {
                _forecolor = value;

                HatchStyle hs = this.HatchStyle;
                if (_brush != null) _brush.Dispose();
                _brush = new HatchBrush(hs, ForeColor, BackColor);
            }
        }


        [Browsable(true)]
        [Category("Fill Symbol")]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        public Color BackColor
        {
            get
            {
                return _backcolor;
            }
            set
            {
                _backcolor = value;

                HatchStyle hs = this.HatchStyle;
                if (_brush != null) _brush.Dispose();
                _brush = new HatchBrush(hs, ForeColor, BackColor);
            }
        }

        [Browsable(true)]
        [DisplayName("Symbol")]
        [Category("Outline Symbol")]
        //[Editor(typeof(gView.Framework.UI.LineSymbolTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
        [DisplayName("Color")]
        [Category("Outline Symbol")]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        public Color OutlineColor
        {
            get { return PenColor; }
            set { PenColor = value; }
        }

        [Browsable(true)]
        [DisplayName("DashStyle")]
        [Category("Outline Symbol")]
        //[Editor(typeof(gView.Framework.UI.DashStyleTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseDashStylePicker()]
        public DashStyle OutlineDashStyle
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

        #region IFillSymbol Member

        public void FillPath(IDisplay display, GraphicsPath path)
        {
            if (_forecolor.A > 0 || _backcolor.A > 0) display.GraphicsContext.FillPath(_brush, path);
            //if(_outlineSymbol!=null) 
            //{
            //    if(_outlineSymbol is ILineSymbol)
            //        ((ILineSymbol)_outlineSymbol).DrawPath(display,path);
            //    else if(_outlineSymbol is SymbolCollection) 
            //    {
            //        foreach(SymbolCollectionItem item in ((SymbolCollection)_outlineSymbol).Symbols) 
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

        public void Draw(IDisplay display, IGeometry geometry)
        {
            GraphicsPath gp = DisplayOperations.Geometry2GraphicsPath(display, geometry);
            if (gp != null)
            {
                this.FillPath(display, gp);

                SimpleFillSymbol.DrawOutlineSymbol(display, _outlineSymbol, geometry, gp);
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
                return null;
            }
        }

        async public Task<bool> Load(IPersistStream stream)
        {
            await base.Load(stream);

            this.ForeColor = Color.FromArgb((int)stream.Load("forecolor", Color.Red.ToArgb()));
            this.BackColor = Color.FromArgb((int)stream.Load("backcolor", Color.Transparent.ToArgb()));
            this.HatchStyle = (HatchStyle)stream.Load("hatchstyle", HatchStyle.Cross);
            _outlineSymbol = (ISymbol)stream.Load("outlinesymbol");

            return true;
        }

        async public Task<bool> Save(IPersistStream stream)
        {
            await base.Save(stream);

            stream.Save("forecolor", _forecolor.ToArgb());
            stream.Save("backcolor", _backcolor.ToArgb());
            stream.Save("hatchstyle", (int)_brush.HatchStyle);
            if (_outlineSymbol != null) stream.Save("outlinesymbol", _outlineSymbol);

            return true;
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
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Symbology.UI.dll");

            IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Symbology.UI.PropertyForm_SimpleFillSymbol") as IPropertyPanel;
            if (p != null)
            {
                return p.PropertyPanel(this);
            }

            return null;
        }


        #endregion

        #region IClone2
        public object Clone(IDisplay display)
        {
            if (display == null) return Clone();
            float fac = 1;
            if (display.refScale > 1)
                fac = (float)(display.refScale / display.mapScale);
            if (display.dpi != 96.0)
                fac *= (float)(display.dpi / 96.0);

            HatchSymbol hSym = new HatchSymbol(_forecolor, _backcolor, _brush.HatchStyle);
            if (_outlineSymbol != null)
                hSym._outlineSymbol = (ISymbol)_outlineSymbol.Clone(display);
            hSym.LegendLabel = _legendLabel;

            return hSym;
        }
        #endregion

        #region IBrushColor Member

        [Browsable(false)]
        public Color FillColor
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
        public Color PenColor
        {
            get
            {
                if (_outlineSymbol is IPenColor)
                {
                    return ((IPenColor)_outlineSymbol).PenColor;
                }
                return Color.Transparent;
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
                return 0;
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
                return 0;
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
        public DashStyle PenDashStyle
        {
            get
            {
                if (_outlineSymbol is IPenDashStyle)
                {
                    return ((IPenDashStyle)_outlineSymbol).PenDashStyle;
                }
                return DashStyle.Solid;
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

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set
            {
                if (_outlineSymbol != null)
                    _outlineSymbol.SymbolSmothingMode = value;
            }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("48177A8B-1B3F-480a-87DF-9F7E1DE57D7B")]
    public sealed class PolygonMaskSymbol : LegendItem, IFillSymbol
    {
        #region IFillSymbol Member

        public void FillPath(IDisplay display, GraphicsPath path)
        {
            using (SolidBrush brush = new SolidBrush(display.BackgroundColor))
            {
                display.GraphicsContext.FillPath(brush, path);
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
                    p.AddRing(((IPolygon)geometry)[i]);
            }
            else if (geometry is IAggregateGeometry)
            {
                for (int g = 0; g < ((IAggregateGeometry)geometry).GeometryCount; g++)
                {
                    if (((IAggregateGeometry)geometry)[g] is IPolygon)
                    {
                        IPolygon poly = (IPolygon)((IAggregateGeometry)geometry)[g];
                        for (int i = 0; i < ((IPolygon)poly).RingCount; i++)
                            p.AddRing(((IPolygon)poly)[i]);
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

        public object Clone(IDisplay display)
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

        #endregion
    }

    [RegisterPlugIn("E043E059-47E9-42A0-ACF0-FB1012DC8AA2")]
    public sealed class GradientFillSymbol : LegendItem, IFillSymbol, IPenColor, IPenDashStyle, IPenWidth, IPropertyPage
    {
        public enum GradientRectType { Feature = 0, Display = 1 }

        private ColorGradient _gradient;
        private ISymbol _outlineSymbol = null;
        private GradientRectType _rectType = GradientRectType.Feature;

        public GradientFillSymbol()
        {
            _gradient = new ColorGradient(Color.Red, Color.Blue);
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
                    _gradient = value;
            }
        }

        [Browsable(true)]
        [Category("Fill Symbol")]
        public GradientRectType GradientRectangle
        {
            get { return _rectType; }
            set { _rectType = value; }
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
                    return ((Symbol)_outlineSymbol).Smoothingmode;

                return SymbolSmoothing.None;
            }
            set
            {
                if (_outlineSymbol is Symbol)
                    ((Symbol)_outlineSymbol).Smoothingmode = value;
            }
        }

        [Browsable(true)]
        [DisplayName("Color")]
        [Category("Outline Symbol")]
        [UseColorPicker()]
        public Color OutlineColor
        {
            get { return PenColor; }
            set { PenColor = value; }
        }

        [Browsable(true)]
        [DisplayName("DashStyle")]
        [Category("Outline Symbol")]
        //[Editor(typeof(gView.Framework.UI.DashStyleTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseDashStylePicker()]
        public DashStyle OutlineDashStyle
        {
            get { return PenDashStyle; }
            set { PenDashStyle = value; }
        }

        [Browsable(true)]
        [Category("Outline Symbol")]
        [DisplayName("Width")]
        [UseWidthPicker()]
        public float OutlineWidth
        {
            get { return PenWidth; }
            set { PenWidth = value; }
        }

        #region IPenColor Member

        [Browsable(false)]
        public Color PenColor
        {
            get
            {
                if (_outlineSymbol is IPenColor)
                {
                    return ((IPenColor)_outlineSymbol).PenColor;
                }
                return Color.Transparent;
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
                return 0;
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
                return 0;
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
        public DashStyle PenDashStyle
        {
            get
            {
                if (_outlineSymbol is IPenDashStyle)
                {
                    return ((IPenDashStyle)_outlineSymbol).PenDashStyle;
                }
                return DashStyle.Solid;
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

        #region IFillSymbol Member

        public void FillPath(IDisplay display, System.Drawing.Drawing2D.GraphicsPath path)
        {
            //display.GraphicsContext.SmoothingMode = (SmoothingMode)this.Smoothingmode;

            if (_gradient != null)
            {
                RectangleF rect =
                    (_rectType == GradientRectType.Feature ?
                    path.GetBounds() :
                    new RectangleF(0, 0, display.iWidth, display.iHeight));

                using (LinearGradientBrush brush = _gradient.CreateNewLinearGradientBrush(rect))
                {
                    display.GraphicsContext.FillPath(brush, path);
                }
            }
            //if (_outlineSymbol != null)
            //{
            //    if (_outlineSymbol is ILineSymbol)
            //        ((ILineSymbol)_outlineSymbol).DrawPath(display, path);
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
            GraphicsPath gp = DisplayOperations.Geometry2GraphicsPath(display, geometry);
            if (gp != null)
            {
                this.FillPath(display, gp);

                SimpleFillSymbol.DrawOutlineSymbol(display, _outlineSymbol, geometry, gp);
                gp.Dispose(); gp = null;
            }
        }

        public void Release()
        {
            if (_outlineSymbol != null)
            {
                _outlineSymbol.Release();
                _outlineSymbol = null;
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

        #region IPropertyPage Member

        public object PropertyPageObject()
        {
            return null;
        }

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Symbology.UI.dll");

            IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Symbology.UI.PropertyForm_SimpleFillSymbol") as IPropertyPanel;
            if (p != null)
            {
                return p.PropertyPanel(this);
            }

            return null;
        }

        #endregion

        #region IPersistable Member

        async public Task<bool> Load(IPersistStream stream)
        {
            await base.Load(stream);

            _gradient = (ColorGradient)stream.Load("gradient", _gradient, _gradient);
            _rectType = (GradientRectType)stream.Load("recttype", (int)GradientRectType.Feature);
            _outlineSymbol = (ISymbol)stream.Load("outlinesymbol");

            return true;
        }

        async public Task<bool> Save(IPersistStream stream)
        {
            await base.Save(stream);

            stream.Save("gradient", _gradient);
            stream.Save("recttype", (int)_rectType);
            if (_outlineSymbol != null) stream.Save("outlinesymbol", _outlineSymbol);

            return true;
        }

        #endregion

        #region IClone2
        public object Clone(IDisplay display)
        {
            if (display == null) return Clone();
            float fac = 1;
            if (display.refScale > 1)
                fac = (float)(display.refScale / display.mapScale);
            if (display.dpi != 96.0)
                fac *= (float)(display.dpi / 96.0);

            GradientFillSymbol fSym = new GradientFillSymbol(_gradient);
            if (_outlineSymbol != null)
                fSym._outlineSymbol = (ISymbol)_outlineSymbol.Clone(display);
            fSym._rectType = _rectType;
            fSym.LegendLabel = _legendLabel;
            return fSym;
        }
        #endregion

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set
            {
                if (_outlineSymbol != null)
                    _outlineSymbol.SymbolSmothingMode = value;
            }
        }

        #endregion
    }
}

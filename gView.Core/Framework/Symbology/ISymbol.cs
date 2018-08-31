using System;
using System.Collections.Generic;
using System.Drawing;
using gView.Framework.Geometry;
using gView.Framework.Carto;
using gView.Framework.IO;
using gView.Framework.system;

namespace gView.Framework.Symbology
{
    public enum SymbolSmoothing
    {
        None = global::System.Drawing.Drawing2D.SmoothingMode.None,
        AntiAlias = global::System.Drawing.Drawing2D.SmoothingMode.AntiAlias
    }

    /// <summary>
    /// Zusammenfassung für ISymbol.
    /// </summary>
    public interface ISymbol : IPersistable, IClone, IClone2, ILegendItem
    {
        void Draw(IDisplay display, IGeometry geometry);
        void Release();

        string Name { get; }

        SymbolSmoothing SymbolSmothingMode { set; }
    }

    public interface INullSymbol : ISymbol
    {
        geometryType GeomtryType { get; set; }
    }

    public interface ILegendItem
    {
        string LegendLabel { get; set; }
        bool ShowInTOC { get; set; }
        int IconHeight { get; }
    }

    public interface ILegendGroup
    {
        int LegendItemCount { get; }
        ILegendItem LegendItem(int index);
        void SetSymbol(ILegendItem item, ISymbol symbol);
    }

    public interface ISymbolTransformation
    {
        float HorizontalOffset { get; set; }
        float VerticalOffset { get; set; }
        float Angle { get; set; }
    }

    public interface ISymbolPositioningUI
    {
        void HorizontalMove(float x);
        void VertiacalMove(float y);
    }

    public interface ISymbolCollection
    {
        List<ISymbolCollectionItem> Symbols { get; }
        void AddSymbol(ISymbol symbol);
        void AddSymbol(ISymbol symbol, bool visible);
        void RemoveSymbol(ISymbol symbol);
        void InsertBefore(ISymbol symbol, ISymbol before, bool visible);
        int IndexOf(ISymbol symbol);
        void ReplaceSymbol(ISymbol oldSymbol, ISymbol newSymbol);
        bool IsVisible(ISymbol symbol);
    }

    public interface ISymbolCollectionItem
    {
        bool Visible { get; }
        ISymbol Symbol { get; }
    }

    public interface IPointSymbol : ISymbol, ISymbolTransformation
    {
        void DrawPoint(IDisplay display, IPoint point);
    }

    public interface ILineSymbol : ISymbol
    {
        void DrawPath(IDisplay display, global::System.Drawing.Drawing2D.GraphicsPath path);
    }

    public interface IFillSymbol : ISymbol
    {
        void FillPath(IDisplay display, global::System.Drawing.Drawing2D.GraphicsPath path);
    }

    public enum TextSymbolAlignment { rightAlignOver, Over, leftAlignOver, rightAlignCenter, Center, leftAlignCenter, rightAlignUnder, Under, leftAlignUnder }

    public class AnnotationPolygonEnvelope
    {
        private float _minx, _miny, _maxx, _maxy;

        internal AnnotationPolygonEnvelope(float minx, float miny, float maxx, float maxy)
        {
            _minx = Math.Min(minx, maxx);
            _miny = Math.Min(miny, maxy);
            _maxx = Math.Max(minx, maxx);
            _maxy = Math.Max(miny, maxy);
        }
        internal AnnotationPolygonEnvelope(AnnotationPolygonEnvelope env)
        {
            _minx = env._minx;
            _miny = env._miny;
            _maxx = env._maxx;
            _maxy = env._maxy;
        }
        public float MinX { get { return _minx; } }
        public float MinY { get { return _miny; } }
        public float MaxX { get { return _maxx; } }
        public float MaxY { get { return _maxy; } }

        internal void Append(float x, float y)
        {
            _minx = Math.Min(_minx, x);
            _miny = Math.Min(_miny, y);
            _maxx = Math.Max(_maxx, x);
            _maxy = Math.Max(_maxy, y);
        }

        internal void Append(AnnotationPolygonEnvelope env)
        {
            _minx = Math.Min(_minx, env._minx);
            _miny = Math.Min(_miny, env._miny);
            _maxx = Math.Max(_maxx, env._maxx);
            _maxy = Math.Max(_maxy, env._maxy);
        }
    }

    public interface IAnnotationPolygonCollision
    {
        bool CheckCollision(IAnnotationPolygonCollision poly);
        bool Contains(float x, float y);
        AnnotationPolygonEnvelope Envelope { get; }
    }

    public class AnnotationPolygon : IAnnotationPolygonCollision
    {
        private float _x1, _y1, _width, _height;
        private double _angle = 0.0;
        private double cos_a = 1.0, sin_a = 0.0;
        private PointF[] _points = null;

        public AnnotationPolygon(float x1, float y1, float width, float height)
        {
            _x1 = x1;
            _y1 = y1;
            _width = width;
            _height = height;
        }

        public void Rotate(float x0, float y0, double angle)
        {
            _angle = angle;

            if (_angle != 0.0)
            {
                cos_a = Math.Cos(_angle * Math.PI / 180.0);
                sin_a = Math.Sin(_angle * Math.PI / 180.0);

                _x1 -= x0;
                _y1 -= y0;

                float x = (float)(_x1 * cos_a - _y1 * sin_a);
                float y = (float)(_x1 * sin_a + _y1 * cos_a);

                _x1 = x + x0;
                _y1 = y + y0;
            }
            else
            {
                cos_a = 1.0;
                sin_a = 0.0;
            }
        }

        public AnnotationPolygonEnvelope Envelope
        {
            get
            {
                float x2 = _x1 + (float)(cos_a * _width - sin_a * _height);
                float y2 = _y1 + (float)(sin_a * _width + cos_a * _height);

                AnnotationPolygonEnvelope poly = new AnnotationPolygonEnvelope(_x1, _y1, x2, y2);

                x2 = _x1 + (float)(cos_a * _width);
                y2 = _y1 + (float)(sin_a * _width);
                poly.Append(x2, y2);

                x2 = _x1 + (float)(-sin_a * _height);
                y2 = _y1 + (float)(cos_a * _height);
                poly.Append(x2, y2);

                return poly;
            }
        }

        public bool Contains(float x, float y)
        {
            float lx = x - _x1;
            float ly = y - _y1;

            float wx = (float)(cos_a * lx + sin_a * ly);
            float wy = (float)(-sin_a * lx + cos_a * ly);

            if (wx >= 0 && wx <= _width &&
                wy >= 0 && wy <= _height) return true;

            return false;
        }

        public PointF[] ToCoords()
        {
            PointF[] points = new PointF[4];
            points[0] = new PointF((float)_x1, (float)_y1);
            points[1] = new PointF(points[0].X + (float)(cos_a * _width), points[0].Y + (float)(sin_a * _width));
            points[2] = new PointF(points[1].X + (float)(-sin_a * _height), points[1].Y + (float)(cos_a * _height));
            points[3] = new PointF(points[0].X + (float)(-sin_a * _height), points[0].Y + (float)(cos_a * _height));

            return points;
        }

        public bool CheckCollision(IAnnotationPolygonCollision cand)
        {
            if (cand is AnnotationPolygon)
            {
                AnnotationPolygon lp = (AnnotationPolygon)cand;

                if (this._points == null)
                    this._points = this.ToCoords();
                if (lp._points == null)
                    lp._points = lp.ToCoords();

                if (HasSeperateLine(this, lp))
                    return false;
                if (HasSeperateLine(lp, this))
                    return false;
                return true;
            }
            else if (cand is AnnotationPolygonCollection)
            {
                foreach (IAnnotationPolygonCollision child in ((AnnotationPolygonCollection)cand))
                {
                    if (this.CheckCollision(child))
                        return true;
                }
            }
            return false;

        }

        private static bool HasSeperateLine(AnnotationPolygon tester, AnnotationPolygon cand)
        {
            for (int i = 1; i <= tester._points.Length; i++)
            {
                PointF p1 = tester[i];
                Vector2dF ortho = new Vector2dF(p1, tester._points[i - 1]);
                ortho.ToOrtho();
                ortho.Normalize();

                float t_min = 0f, t_max = 0f, c_min = 0f, c_max = 0f;
                MinMaxAreaForOrhtoSepLine(p1, ortho, tester, ref t_min, ref t_max);
                MinMaxAreaForOrhtoSepLine(p1, ortho, cand, ref c_min, ref c_max);

                if ((t_min <= c_max && t_max <= c_min) ||
                    (c_min <= t_max && c_max <= t_min))
                    return true;
            }

            return false;
        }

        private static void MinMaxAreaForOrhtoSepLine(PointF p1, Vector2dF ortho, AnnotationPolygon lp, ref float min, ref float max)
        {
            for (int j = 0; j < lp._points.Length; j++)
            {
                Vector2dF rc = new Vector2dF(lp[j], p1);
                float prod = ortho.DotProduct(rc);
                if (j == 0)
                {
                    min = max = prod;
                }
                else
                {
                    min = Math.Min(min, prod);
                    max = Math.Max(max, prod);
                }
            }
        }

        public PointF this[int index]
        {
            get
            {
                if (_points == null) _points = ToCoords();

                if (index < 0 || index >= _points.Length)
                    return _points[0];
                return _points[index];
            }
        }

        public float X1 { get { return _x1; } set { _x1 = value; } }
        public float Y1 { get { return _y1; } set { _y1 = value; } }
        public double Angle { get { return _angle; } set { _angle = value; } }

        public PointF CenterPoint
        {
            get
            {
                float cx = 0f, cy = 0f;
                foreach (PointF point in ToCoords())
                {
                    cx += point.X / 4f;
                    cy += point.Y / 4f;
                }
                return new PointF(cx, cy);
            }
        }

        #region Vector Helper Classes
        private class Vector2dF
        {
            float _x, _y;

            public Vector2dF(PointF p1, PointF p0)
            {
                _x = p1.X - p0.X;
                _y = p1.Y - p0.Y;
            }

            public void ToOrtho()
            {
                float x = _x;
                _x = -_y;
                _y = x;
            }

            public void Normalize()
            {
                float l = (float)Math.Sqrt(_x * _x + _y * _y);
                _x /= l;
                _y /= l;
            }

            public float DotProduct(Vector2dF v)
            {
                return _x * v._x + _y * v._y;
            }
        }
        #endregion
    }

    public class AnnotationPolygonCollection : List<IAnnotationPolygonCollision>, IAnnotationPolygonCollision
    {
        #region IAnnotationPolygonCollision Member

        public bool CheckCollision(IAnnotationPolygonCollision poly)
        {
            foreach (IAnnotationPolygonCollision child in this)
            {
                if (child.CheckCollision(poly))
                    return true;
            }
            return false;
        }

        public bool Contains(float x, float y)
        {
            foreach (IAnnotationPolygonCollision child in this)
            {
                if (child.Contains(x, y))
                    return true;
            }
            return false;
        }

        public AnnotationPolygonEnvelope Envelope
        {
            get
            {
                if (this.Count == 0)
                    return new AnnotationPolygonEnvelope(0, 0, 0, 0);

                AnnotationPolygonEnvelope env = this[0].Envelope;
                for (int i = 1; i < this.Count; i++)
                {
                    env.Append(this[i].Envelope);
                }
                return env;
            }
        }

        #endregion
    }

    public interface IDisplayCharacterRanges
    {
        float Width { get; }
        RectangleF this[int i] { get; }
    }

    public interface ILabel
    {
        string Text { get; set; }
        TextSymbolAlignment TextSymbolAlignment { get; set; }

        IDisplayCharacterRanges MeasureCharacterWidth(IDisplay display);

        List<IAnnotationPolygonCollision> AnnotationPolygon(IDisplay display, IGeometry geometry);
    }

    public interface ITextSymbol : ISymbol, ILabel, ISymbolTransformation, ISymbolRotation
    {
        Font Font { get; set; }

        float MaxFontSize { get; set; }
        float MinFontSize { get; set; }
    }

    public enum RotationType { geographic, aritmetic }
    public enum RotationUnit { rad, deg, gon }

    public interface ISymbolCreator
    {
        ISymbol CreateStandardSymbol(geometryType type);
        ISymbol CreateStandardSelectionSymbol(geometryType type);
        ISymbol CreateStandardHighlightSymbol(geometryType type);
    }

    public interface ISymbolRotation
    {
        float Rotation { get; set; }
    }

    public class SymbolRotation : IPersistable, IClone
    {
        private RotationType _rotType = RotationType.aritmetic;
        private RotationUnit _rotUnit = RotationUnit.deg;
        private string _rotationFieldName = "";

        public RotationType RotationType
        {
            get { return _rotType; }
            set { _rotType = value; }
        }
        public RotationUnit RotationUnit
        {
            get { return _rotUnit; }
            set { _rotUnit = value; }
        }
        public string RotationFieldName
        {
            get { return _rotationFieldName; }
            set { _rotationFieldName = value; }
        }

        public double Convert2DEGAritmetic(double rotation)
        {
            if (_rotType == RotationType.aritmetic && _rotUnit == RotationUnit.deg) return rotation;

            switch (_rotUnit)
            {
                case RotationUnit.rad:
                    rotation *= 180.0 / Math.PI;
                    break;
                case RotationUnit.gon:
                    rotation /= 0.9;
                    break;
            }

            switch (_rotType)
            {
                case RotationType.geographic:
                    rotation = 90 - rotation;
                    if (rotation < 0.0) rotation += 360.0;
                    break;
            }

            return rotation;
        }

        #region IPersistable Members

        public string PersistID
        {
            get { return ""; }
        }

        public void Load(IPersistStream stream)
        {
            _rotationFieldName = (string)stream.Load("RotationFieldname", "");
            _rotType = (RotationType)stream.Load("RotationType", RotationType.aritmetic);
            _rotUnit = (RotationUnit)stream.Load("RotationUnit", RotationUnit.deg);
        }

        public void Save(IPersistStream stream)
        {
            if (_rotationFieldName == "") return;

            stream.Save("RotationFieldname", _rotationFieldName);
            stream.Save("RotationType", (int)_rotType);
            stream.Save("RotationUnit", (int)_rotUnit);
        }

        #endregion

        #region IClone Members

        public object Clone()
        {
            SymbolRotation rot = new SymbolRotation();
            rot._rotationFieldName = _rotationFieldName;
            rot._rotType = _rotType;
            rot._rotUnit = _rotUnit;
            return rot;
        }

        #endregion
    }

    public interface IBrushColor
    {
        Color FillColor { get; set; }
    }
    public interface IPenColor
    {
        Color PenColor { get; set; }
    }
    public interface IPenWidth
    {
        float PenWidth { get; set; }
        float MaxPenWidth { get; set; }
        float MinPenWidth { get; set; }
    }
    public interface ISymbolSize
    {
        float SymbolSize { get; set; }

        float MaxSymbolSize { get; set; }
        float MinSymbolSize { get; set; }
    }
    public interface ISymbolWidth
    {
        float SymbolWidth { get; set; }
    }
    public interface IPenDashStyle
    {
        global::System.Drawing.Drawing2D.DashStyle PenDashStyle { get; set; }
    }
    public interface IFontColor
    {
        Color FontColor { get; set; }
    }
    public interface IFont
    {
        Font Font { get; set; }
    }

    /*
    public interface ISymbolPreview 
    {
        void DrawPreview(System.Drawing.Graphics graphics,System.Drawing.Rectangle rectangle);
    }
    */

    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Property)]
    public class UseColorPicker : global::System.Attribute
    {
        public UseColorPicker()
        {
        }
    }
    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Property)]
    public class UseWidthPicker : global::System.Attribute
    {
        public UseWidthPicker()
        {
        }
    }
    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Property)]
    public class UseDashStylePicker : global::System.Attribute
    {
        public UseDashStylePicker()
        {
        }
    }
    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Property)]
    public class UseHatchStylePicker : global::System.Attribute
    {
        public UseHatchStylePicker()
        {
        }
    }
    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Property)]
    public class UseLineSymbolPicker : global::System.Attribute
    {
        public UseLineSymbolPicker()
        {
        }
    }
    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Property)]
    public class UseCharacterPicker : global::System.Attribute
    {
        public UseCharacterPicker()
        {
        }
    }
    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Property)]
    public class UseFilePicker : global::System.Attribute
    {
        public UseFilePicker()
        {
        }
    }
    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Property)]
    public class UseColorGradientPicker : global::System.Attribute
    {
        public UseColorGradientPicker()
        {
        }
    }
}

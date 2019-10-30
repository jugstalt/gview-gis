using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Symbology.UI;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Symbology.Framework.Symbology.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Xml;

namespace gView.Framework.Symbology
{
    [gView.Framework.system.RegisterPlugIn("A5DA4D8D-879F-41a5-9795-F22BE5B85877")]
    public class SimpleTextSymbol : Symbol, ITextSymbol, IPropertyPage, IFontColor, IFont
    {
        protected string _text;
        protected Font _font;
        protected SolidBrush _brush;
        protected float _xOffset = 0, _yOffset = 0, _angle = 0, _hOffset = 0, _vOffset = 0, _rotation = 0;
        protected TextSymbolAlignment _align = TextSymbolAlignment.Center;

        public SimpleTextSymbol()
        {
            _font = new Font("Arial", 10);
            _brush = new SolidBrush(Color.Black);

            this.ShowInTOC = false;
        }

        protected SimpleTextSymbol(Font font, Color color)
        {
            _font = font;
            _brush = new SolidBrush(color);

            this.ShowInTOC = false;
        }

        [Browsable(true)]
        public Font Font
        {
            get { return _font; }
            set
            {
                if (_font == value)
                {
                    return;
                }

                if (_font != null)
                {
                    _font.Dispose();
                }

                _font = value;
                _measureTextWidth = null;
            }
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        public System.Drawing.Color Color
        {
            get { return _brush.Color; }
            set { _brush.Color = value; }
        }

        virtual protected int DrawingLevels { get { return 1; } }
        private void DrawAtPoint(IDisplay display, IPoint point, string text, float angle, StringFormat format)
        {
            DrawAtPoint(display, point, text, angle, format, -1);
        }
        virtual protected void DrawAtPoint(IDisplay display, IPoint point, string text, float angle, StringFormat format, int level)
        {
            if (_font != null)
            {
                //point.X+=_xOffset;
                //point.Y+=_yOffset;

                try
                {
                    display.GraphicsContext.TextRenderingHint = ((this.Smoothingmode == SymbolSmoothing.None) ? System.Drawing.Text.TextRenderingHint.SystemDefault : System.Drawing.Text.TextRenderingHint.AntiAlias);

                    display.GraphicsContext.TranslateTransform((float)point.X, (float)point.Y);
                    if (angle != 0 || _angle != 0 || _rotation != 0)
                    {
                        display.GraphicsContext.RotateTransform(angle + _angle + _rotation);
                    }
                    DrawString(display.GraphicsContext, text, _font, _brush, (float)_xOffset, (float)_yOffset, format);
                }
                finally
                {
                    display.GraphicsContext.ResetTransform();
                    display.GraphicsContext.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                }
            }
        }

        private StringFormat stringFormatFromAlignment
        {
            get
            {
                StringFormat format = new StringFormat();
                switch (_align)
                {
                    case TextSymbolAlignment.rightAlignOver:
                        format.Alignment = StringAlignment.Far;
                        format.LineAlignment = StringAlignment.Far;
                        break;
                    case TextSymbolAlignment.rightAlignCenter:
                        format.Alignment = StringAlignment.Far;
                        format.LineAlignment = StringAlignment.Center;
                        break;
                    case TextSymbolAlignment.rightAlignUnder:
                        format.Alignment = StringAlignment.Far;
                        format.LineAlignment = StringAlignment.Near;
                        break;
                    case TextSymbolAlignment.Over:
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Far;
                        break;
                    case TextSymbolAlignment.Center:
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Center;
                        break;
                    case TextSymbolAlignment.Under:
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Near;
                        break;
                    case TextSymbolAlignment.leftAlignOver:
                        format.Alignment = StringAlignment.Near;
                        format.LineAlignment = StringAlignment.Far;
                        break;
                    case TextSymbolAlignment.leftAlignCenter:
                        format.Alignment = StringAlignment.Near;
                        format.LineAlignment = StringAlignment.Center;
                        break;
                    case TextSymbolAlignment.leftAlignUnder:
                        format.Alignment = StringAlignment.Near;
                        format.LineAlignment = StringAlignment.Near;
                        break;
                }
                return format;
            }
        }

        #region ILabel Members

        [Browsable(false)]
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != value)
                {
                    _measureTextWidth = null;
                    _text = value;
                }
            }
        }
        [Browsable(false)]
        public TextSymbolAlignment TextSymbolAlignment
        {
            get { return _align; }
            set { _align = value; }
        }

        private IDisplayCharacterRanges _measureTextWidth = null;
        public IDisplayCharacterRanges MeasureCharacterWidth(IDisplay display)
        {
            if (_measureTextWidth != null)
            {
                return _measureTextWidth;
            }

            StringFormat format = stringFormatFromAlignment;
            IDisplayCharacterRanges ranges = new DisplayCharacterRanges(display.GraphicsContext, _font, format, _text);

            return ranges;
        }

        public List<IAnnotationPolygonCollision> AnnotationPolygon(IDisplay display, IGeometry geometry)
        {
            if (_font == null || _text == null || display.GraphicsContext == null)
            {
                return null;
            }

            List<IAnnotationPolygonCollision> polygons = new List<IAnnotationPolygonCollision>();
            if (geometry is IPoint)
            {
                #region IPoint
                double x = ((IPoint)geometry).X;
                double y = ((IPoint)geometry).Y;
                display.World2Image(ref x, ref y);

                AnnotationPolygon polygon = AnnotationPolygon(display, (float)x, (float)y);
                polygon.Rotate((float)x, (float)y, Angle);

                polygons.Add(polygon);
                #endregion
            }
            else if (geometry is IMultiPoint)
            {
                #region IMultipoint
                IMultiPoint pColl = (IMultiPoint)geometry;
                for (int i = 0; i < pColl.PointCount; i++)
                {
                    double x = pColl[i].X;
                    double y = pColl[i].Y;

                    display.World2Image(ref x, ref y);

                    AnnotationPolygon polygon = AnnotationPolygon(display, (float)x, (float)y);
                    polygon.Rotate((float)x, (float)y, Angle);

                    polygons.Add(polygon);
                }
                #endregion
            }
            else if (geometry is IDisplayPath)
            {
                if (String.IsNullOrEmpty(_text))
                {
                    return null;
                }

                IDisplayPath path = (IDisplayPath)geometry;

                #region Text On Path
                StringFormat format = stringFormatFromAlignment;

                IDisplayCharacterRanges ranges = this.MeasureCharacterWidth(display);
                float sizeW = ranges.Width;
                float stat0 = path.Chainage, stat1 = stat0 + sizeW, stat = stat0;
                if (stat0 < 0)
                {
                    return null;
                }

                #region Richtung des Textes
                PointF? p1_ = path.PointAt(stat0); //SpatialAlgorithms.Algorithm.DisplayPathPoint(path, stat0);
                PointF? p2_ = path.PointAt(stat1); //SpatialAlgorithms.Algorithm.DisplayPathPoint(path, stat1);
                if (p1_ == null || p2_ == null)
                {
                    return null;
                }

                if (((PointF)p1_).X > ((PointF)p2_).X)
                {
                    #region Swap Path Direction
                    path.ChangeDirection();
                    stat = stat0 = path.Length - stat1;
                    stat1 = stat0 + sizeW;
                    #endregion
                }
                #endregion

                AnnotationPolygonCollection charPolygons = new AnnotationPolygonCollection();
                float x, y, angle;

                for (int i = 0; i < _text.Length; i++)
                {
                    RectangleF cSize = ranges[i];
                    PointF? p1, p2;
                    int counter = 0;
                    while (true)
                    {
                        p1 = path.PointAt(stat); //SpatialAlgorithms.Algorithm.DisplayPathPoint(path, stat);
                        p2 = path.PointAt(stat + cSize.Width);
                        if (p1 == null || p2 == null)
                        {
                            return null;
                        }

                        angle = (float)(Math.Atan2(((PointF)p2).Y - ((PointF)p1).Y, ((PointF)p2).X - ((PointF)p1).X) * 180.0 / Math.PI);

                        //float ccc = 0f;
                        //angle = 0f;
                        //for (float xxx = 0f; xxx <= cSize.Width; xxx += cSize.Width / 10f, ccc += 1f)
                        //{
                        //    p2 = path.PointAt(stat + xxx/*cSize.Width*/); //SpatialAlgorithms.Algorithm.DisplayPathPoint(path, stat + cSize.Width);
                        //    if (p1 == null || p2 == null)
                        //        return null;

                        //    angle += (float)(Math.Atan2(((PointF)p2).Y - ((PointF)p1).Y, ((PointF)p2).X - ((PointF)p1).X) * 180.0 / Math.PI);
                        //}
                        //angle /= ccc;

                        x = ((PointF)p1).X; y = ((PointF)p1).Y;
                        AnnotationPolygon polygon = null;
                        switch (format.LineAlignment)
                        {
                            case StringAlignment.Near:
                                polygon = new Symbology.AnnotationPolygon((float)x + .1f * cSize.Width, (float)y + .2f * cSize.Height,
                                                                           .8f * cSize.Width, .6f * cSize.Height);
                                break;
                            case StringAlignment.Far:
                                polygon = new Symbology.AnnotationPolygon((float)x + .1f * cSize.Width, (float)y - .8f * cSize.Height,
                                                                           .8f * cSize.Width, .6f * cSize.Height);
                                break;
                            default:
                                polygon = new Symbology.AnnotationPolygon((float)x + .1f * cSize.Width, (float)y - .6f * cSize.Height / 2f,
                                                                           .8f * cSize.Width, .6f * cSize.Height);
                                break;
                        }
                        polygon.Rotate((float)x, (float)y, Angle + angle);
                        if (charPolygons.CheckCollision(polygon))
                        {
                            stat += cSize.Width / 10.0f;
                            counter++;
                            if (counter > 7)
                            {
                                return null;
                            }

                            continue;
                        }
                        charPolygons.Add(polygon);

                        //using (System.Drawing.Drawing2D.GraphicsPath grpath = new System.Drawing.Drawing2D.GraphicsPath())
                        //{
                        //    grpath.StartFigure();
                        //    grpath.AddLine(polygon[0], polygon[1]);
                        //    grpath.AddLine(polygon[1], polygon[2]);
                        //    grpath.AddLine(polygon[2], polygon[3]);
                        //    grpath.CloseFigure();

                        //    display.GraphicsContext.FillPath(Brushes.Aqua, grpath);
                        //}

                        break;
                    }
                    stat += cSize.Width;// +_font.Height * 0.1f;
                }

                if (charPolygons.Count > 0)
                {
                    #region Glättung
                    //for (int i = 1; i < charPolygons.Count - 1; i++)
                    //{
                    //    double angle0 = ((AnnotationPolygon)charPolygons[i - 1]).Angle;
                    //    double angle2 = ((AnnotationPolygon)charPolygons[i + 1]).Angle;
                    //    ((AnnotationPolygon)charPolygons[i]).Angle = (angle0 + angle2) * 0.5;

                    //    float x0 = ((AnnotationPolygon)charPolygons[i - 1]).X1;
                    //    float y0 = ((AnnotationPolygon)charPolygons[i - 1]).Y1;
                    //    float x2 = ((AnnotationPolygon)charPolygons[i + 1]).X1;
                    //    float y2 = ((AnnotationPolygon)charPolygons[i + 1]).Y1;
                    //    ((AnnotationPolygon)charPolygons[i]).X1 = (x0 + x2) * 0.5f;
                    //    ((AnnotationPolygon)charPolygons[i]).Y1 = (y0 + y2) * 0.5f;
                    //}
                    #endregion

                    polygons.Add(charPolygons);
                    path.AnnotationPolygonCollision = charPolygons;
                }
                #endregion
            }
            else if (geometry is IPolyline)
            {
                IPolyline pLine = (IPolyline)geometry;
                for (int iPath = 0; iPath < pLine.PathCount; iPath++)
                {
                    IPath path = pLine[iPath];
                    if (path.PointCount == 0)
                    {
                        continue;
                    }

                    #region Simple Method
                    IPoint p1 = path[0], p2;
                    for (int iPoint = 1; iPoint < path.PointCount; iPoint++)
                    {
                        p2 = path[iPoint];
                        double angle = -Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180.0 / Math.PI;
                        if (display.DisplayTransformation.UseTransformation)
                        {
                            angle -= display.DisplayTransformation.DisplayRotation;
                        }

                        if (angle < 0)
                        {
                            angle += 360;
                        }

                        if (angle > 90 && angle < 270)
                        {
                            angle -= 180;
                        }

                        StringFormat format = stringFormatFromAlignment;
                        double x, y;
                        if (format.Alignment == StringAlignment.Center)
                        {
                            x = p1.X * 0.5 + p2.X * 0.5;
                            y = p1.Y * 0.5 + p2.Y * 0.5;
                        }
                        else if (format.Alignment == StringAlignment.Far)
                        {
                            x = p2.X;
                            y = p2.Y;
                        }
                        else
                        {
                            x = p1.X;
                            y = p1.Y;
                        }
                        display.World2Image(ref x, ref y);

                        AnnotationPolygon polygon = AnnotationPolygon(display, (float)x, (float)y);
                        polygon.Rotate((float)x, (float)y, Angle + angle);

                        polygons.Add(polygon);
                        p1 = p2;
                    }
                    #endregion
                }
            }

            return polygons.Count > 0 ? polygons : null;
        }
        #endregion

        private AnnotationPolygon AnnotationPolygon(IDisplay display, float x, float y)
        {
            return AnnotationPolygon(display, _text, x, y);
        }
        private AnnotationPolygon AnnotationPolygon(IDisplay display, string text, float x, float y)
        {
            SizeF size = display.GraphicsContext.MeasureString(text, _font);

            float x1 = 0, y1 = 0;
            switch (_align)
            {
                case TextSymbolAlignment.rightAlignOver:
                    x1 = x - size.Width;
                    y1 = y - size.Height;
                    break;
                case TextSymbolAlignment.rightAlignCenter:
                    x1 = x - size.Width;
                    y1 = y - size.Height / 2;
                    break;
                case TextSymbolAlignment.rightAlignUnder:
                    x1 = x - size.Width;
                    y1 = y;
                    break;
                case TextSymbolAlignment.Over:
                    x1 = x - size.Width / 2;
                    y1 = y - size.Height;
                    break;
                case TextSymbolAlignment.Center:
                    x1 = x - size.Width / 2;
                    y1 = y - size.Height / 2;
                    break;
                case TextSymbolAlignment.Under:
                    x1 = x - size.Width / 2;
                    y1 = y;
                    break;
                case TextSymbolAlignment.leftAlignOver:
                    x1 = x;
                    y1 = y - size.Height;
                    break;
                case TextSymbolAlignment.leftAlignCenter:
                    x1 = x;
                    y1 = y - size.Height / 2;
                    break;
                case TextSymbolAlignment.leftAlignUnder:
                    x1 = x;
                    y1 = y;
                    break;
            }

            return new AnnotationPolygon(x1, y1, size.Width, size.Height);
        }

        #region ITextSymbol

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

        [Browsable(true)]
        [Category("Reference Scaling")]
        public float MaxFontSize { get; set; }

        [Browsable(true)]
        [Category("Reference Scaling")]
        public float MinFontSize { get; set; }

        [Browsable(true)]
        public bool IncludesSuperScript { get; set; }

        #endregion

        #region ISymbol Members

        public void Draw(IDisplay display, IGeometry geometry)
        {
            if (_font == null)
            {
                return;
            }

            if (geometry is IPoint)
            {
                #region IPoint
                double x = ((IPoint)geometry).X;
                double y = ((IPoint)geometry).Y;
                display.World2Image(ref x, ref y);
                IPoint p = new gView.Framework.Geometry.Point(x, y);

                DrawAtPoint(display, p, _text, 0, stringFormatFromAlignment);
                #endregion
            }
            if (geometry is IMultiPoint)
            {
                #region IMultiPoint
                IMultiPoint pColl = (IMultiPoint)geometry;
                for (int i = 0; i < pColl.PointCount; i++)
                {
                    double x = pColl[i].X;
                    double y = pColl[i].Y;

                    display.World2Image(ref x, ref y);
                    IPoint p = new gView.Framework.Geometry.Point(x, y);

                    DrawAtPoint(display, p, _text, 0, stringFormatFromAlignment);
                }
                #endregion
            }
            else if (geometry is IDisplayPath && ((IDisplayPath)geometry).AnnotationPolygonCollision is AnnotationPolygonCollection)
            {
                if (String.IsNullOrEmpty(_text))
                {
                    return;
                }

                IDisplayPath path = (IDisplayPath)geometry;
                AnnotationPolygonCollection apc = path.AnnotationPolygonCollision as AnnotationPolygonCollection;
                StringFormat format = stringFormatFromAlignment;
                format.LineAlignment = StringAlignment.Center;
                format.Alignment = StringAlignment.Center;

                if (_text.Length == apc.Count)
                {
                    int drawingLevels = this.DrawingLevels; // Für Blockout und Glowing Text -> Zuerst den Blockout für alle Zeichen...
                    for (int level = 0; level < drawingLevels; level++)
                    {
                        for (int i = 0; i < _text.Length; i++)
                        {
                            AnnotationPolygon ap = apc[i] as AnnotationPolygon;
                            if (ap != null)
                            {
                                PointF centerPoint = ap.CenterPoint;
                                DrawAtPoint(display, new Geometry.Point(centerPoint.X, centerPoint.Y), _text[i].ToString(), (float)ap.Angle, format, level);
                            }
                        }
                    }
                }

                #region Old Method
                /*
                #region Text On Path
                StringFormat format = stringFormatFromAlignment;
                DisplayCharacterRanges ranges = new DisplayCharacterRanges(display.GraphicsContext, _font, format, _text);

                double sizeW = ranges.Width;
                double len = path.Length;
                double stat0 = len / 2 - sizeW / 2, stat1 = stat0 + sizeW;
                if (stat0 < 0) return;

                #region Richtung des Textes
                Geometry.Point p1_ = SpatialAlgorithms.Algorithm.DisplayPathPoint(path, stat0);
                Geometry.Point p2_ = SpatialAlgorithms.Algorithm.DisplayPathPoint(path, stat1);
                if (p1_ == null || p2_ == null)
                    return;
                if (p1_.X > p2_.X)
                {
                    #region Swap Path Direction
                    path.ChangeDirection();
                    #endregion
                }
                #endregion

                AnnotationPolygonCollection charPolygons = new AnnotationPolygonCollection();
                double x, y, angle;

                for (int i = 0; i < _text.Length; i++)
                {
                    RectangleF cSize = ranges[i];
                    Geometry.Point p1, p2;
                    while (true)
                    {
                        p1 = SpatialAlgorithms.Algorithm.DisplayPathPoint(path, stat0);
                        p2 = SpatialAlgorithms.Algorithm.DisplayPathPoint(path, stat0 + cSize.Width);
                        if (p1 == null || p2 == null)
                            return;

                        angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180.0 / Math.PI;

                        x = p1.X; y = p1.Y;
                        AnnotationPolygon polygon = null;
                        switch (format.LineAlignment)
                        {
                            case StringAlignment.Near:
                                polygon = new Symbology.AnnotationPolygon((float)x + .1f * cSize.Width, (float)y + .2f * cSize.Height,
                                                                           .8f * cSize.Width, .6f * cSize.Height);
                                break;
                            case StringAlignment.Far:
                                polygon = new Symbology.AnnotationPolygon((float)x + .1f * cSize.Width, (float)y - .8f * cSize.Height,
                                                                           .8f * cSize.Width, .6f * cSize.Height);
                                break;
                            default:
                                polygon = new Symbology.AnnotationPolygon((float)x + .1f * cSize.Width, (float)y - .6f * cSize.Height / 2f,
                                                                           .8f * cSize.Width, .6f * cSize.Height);
                                break;
                        }
                        polygon.Rotate((float)x, (float)y, Angle + angle);
                        if (charPolygons.CheckCollision(polygon))
                        {
                            stat0 += cSize.Width / 10.0;
                            continue;
                        }
                        charPolygons.Add(polygon);

                        //using (System.Drawing.Drawing2D.GraphicsPath grpath = new System.Drawing.Drawing2D.GraphicsPath())
                        //{
                        //    grpath.StartFigure();
                        //    grpath.AddLine(polygon[0], polygon[1]);
                        //    grpath.AddLine(polygon[1], polygon[2]);
                        //    grpath.AddLine(polygon[2], polygon[3]);
                        //    grpath.CloseFigure();

                        //    display.GraphicsContext.FillPath(Brushes.Aqua, grpath);
                        //}

                        break;
                    }

                    if (format.Alignment == StringAlignment.Center)
                    {
                        x = p1.X * 0.5 + p2.X * 0.5;
                        y = p1.Y * 0.5 + p2.Y * 0.5;
                    }
                    else if (format.Alignment == StringAlignment.Far)
                    {
                        x = p2.X;
                        y = p2.Y;
                    }
                    else
                    {
                        x = p1.X;
                        y = p1.Y;
                    }
                    DrawAtPoint(display, new Geometry.Point(x, y), _text[i].ToString(), (float)angle, format);

                    stat0 += (double)cSize.Width;
                }
                //display.GraphicsContext.FillEllipse(Brushes.Green, (float)p1_.X - 3, (float)p1_.Y - 3, 6f, 6f);
                //display.GraphicsContext.FillEllipse(Brushes.Blue, (float)p2_.X - 3, (float)p2_.Y - 3, 6f, 6f);
                #endregion
                 * */
                #endregion
            }
            else if (geometry is IPolyline)
            {
                IPolyline pLine = (IPolyline)geometry;
                for (int iPath = 0; iPath < pLine.PathCount; iPath++)
                {
                    IPath path = pLine[iPath];
                    if (path.PointCount == 0)
                    {
                        continue;
                    }

                    #region Parallel Text
                    IPoint p1 = path[0], p2;
                    for (int iPoint = 1; iPoint < path.PointCount; iPoint++)
                    {
                        p2 = path[iPoint];
                        double angle = -Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180.0 / Math.PI;
                        if (display.DisplayTransformation.UseTransformation)
                        {
                            angle -= display.DisplayTransformation.DisplayRotation;
                        }

                        if (angle < 0)
                        {
                            angle += 360;
                        }

                        if (angle > 90 && angle < 270)
                        {
                            angle -= 180;
                        }

                        StringFormat format = stringFormatFromAlignment;
                        double x, y;
                        if (format.Alignment == StringAlignment.Center)
                        {
                            x = p1.X * 0.5 + p2.X * 0.5;
                            y = p1.Y * 0.5 + p2.Y * 0.5;
                        }
                        else if (format.Alignment == StringAlignment.Far)
                        {
                            x = p2.X;
                            y = p2.Y;
                        }
                        else
                        {
                            x = p1.X;
                            y = p1.Y;
                        }
                        display.World2Image(ref x, ref y);
                        IPoint p = new gView.Framework.Geometry.Point(x, y);

                        //_text += "  " + angle.ToString();
                        DrawAtPoint(display, p, _text, (float)angle, format);
                        p1 = p2;
                    }
                    #endregion
                }
            }
            else if (geometry is IPolygon)
            {
                /*
                GraphicsPath path = DisplayOperations.Geometry2GraphicsPath(display, geometry);

                foreach (PointF point in gView.SpatialAlgorithms.Algorithm.LabelPoints(path))
                {
                    DrawAtPoint(display, new gView.Framework.Geometry.Point(point.X, point.Y), 0, stringFormatFromAlignment);
                }
                 * */
            }
            else if (geometry is IAggregateGeometry)
            {
                for (int i = 0; i < ((IAggregateGeometry)geometry).GeometryCount; i++)
                {
                    Draw(display, ((IAggregateGeometry)geometry)[i]);
                }
            }
        }

        virtual public void Release()
        {
            if (_font != null)
            {
                _font.Dispose();
            }

            _font = null;
            if (_brush != null)
            {
                _brush.Dispose();
            }

            _brush = null;
        }

        [Browsable(false)]
        virtual public string Name
        {
            get { return "Simple Text Symbol"; }
        }

        #endregion

        #region IPersistable Members

        virtual public void Load(IPersistStream stream)
        {
            base.Load(stream);

            string err = "";
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
                if (sizeNode != null)
                {
                    sizeNode.InnerText = sizeNode.InnerText.Replace(".", ",");
                }

                soap = doc.OuterXml;
                //
                //
                //

                ms.Write(encoder.GetBytes(soap), 0, soap.Length);
                ms.Position = 0;
                SoapFormatter formatter = new SoapFormatter();
                _font = (Font)formatter.Deserialize<Font>(ms);

                this.MaxFontSize = (float)stream.Load("maxfontsize", 0f);
                this.MinFontSize = (float)stream.Load("minfontsize", 0f);
            }
            catch (Exception ex)
            {
                err = ex.Message;
                ex = null;
            }
            this.Color = Color.FromArgb((int)stream.Load("color", Color.Red.ToArgb()));

            HorizontalOffset = (float)stream.Load("xOffset", (float)0);
            VerticalOffset = (float)stream.Load("yOffset", (float)0);
            Angle = (float)stream.Load("Angle", (float)0);
            _align = (TextSymbolAlignment)stream.Load("Alignment", (int)TextSymbolAlignment.Center);
            this.IncludesSuperScript = (bool)stream.Load("includessuperscript", false);
        }

        virtual public void Save(IPersistStream stream)
        {
            base.Save(stream);

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
            stream.Save("color", this.Color.ToArgb());

            stream.Save("xOffset", HorizontalOffset);
            stream.Save("yOffset", VerticalOffset);
            stream.Save("Angle", Angle);
            stream.Save("Alignment", (int)_align);

            stream.Save("maxfontsize", this.MaxFontSize);
            stream.Save("minfontsize", this.MinFontSize);
            stream.Save("includessuperscript", this.IncludesSuperScript);
        }

        #endregion

        #region IClone2 Members

        virtual public object Clone(CloneOptions options)
        {
            var display = options?.Display;

            if (display == null)
            {
                return this.Clone();
            }

            float fac = 1;
            if (display.refScale > 1)
            {
                fac = ReferenceScaleHelper.RefscaleFactor(
                    (float)(display.refScale / display.mapScale),
                    _font.Size, MinFontSize, MaxFontSize);
                fac = options.LabelRefScaleFactor(fac);
            }
            if (display.dpi != 96.0)
            {
                fac *= (float)(display.dpi / 96.0);
            }

            SimpleTextSymbol tSym = new SimpleTextSymbol(new Font(_font.Name, Math.Max(_font.Size * fac, 2), _font.Style), _brush.Color);
            tSym.HorizontalOffset = HorizontalOffset * fac;
            tSym.VerticalOffset = VerticalOffset * fac;
            tSym.Angle = Angle;
            tSym._align = _align;
            tSym.Smoothingmode = this.Smoothingmode;
            tSym.IncludesSuperScript = this.IncludesSuperScript;

            return tSym;
        }

        #endregion

        #region IPropertyPage Members

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"/gView.Win.Symbology.UI.dll");

            IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Symbology.UI.PropertyForm_SimpleTextSymbol") as IPropertyPanel;
            if (p != null)
            {
                return p.PropertyPanel(this);
            }

            return null;
        }

        public object PropertyPageObject()
        {
            return null;
        }

        #endregion

        #region ISymbolRotation Member

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
                return this.Color;
            }
            set
            {
                this.Color = value;
            }
        }

        #endregion

        #region ISymbol Member

        [Browsable(false)]
        virtual public SymbolSmoothing SymbolSmothingMode
        {
            set { this.Smoothingmode = value; }
        }

        #endregion

        #region Helper

        protected void DrawString(System.Drawing.Graphics/*IGraphicsEngine*/ gr, string text, Font font, SolidBrush brush, float xOffset, float yOffset, StringFormat format)
        {
            if (IncludesSuperScript == true)
            {
                if (!text.Contains("^"))
                {
                    gr.DrawString(text, font, brush, xOffset, yOffset, format);
                }
                else
                {
                    float fontSize = font.Size;
                    while (!String.IsNullOrWhiteSpace(text))
                    {
                        var pos1 = text.IndexOf("^");
                        var pos2 = text.IndexOf("~");
                        int pos;
                        bool superScript = true;

                        if (pos1 >= 0 && pos2 >= 0)
                        {
                            if (pos1 < pos2)
                            {
                                pos = pos1;
                            }
                            else
                            {
                                pos = pos2;
                                superScript = false;
                            }
                        }
                        else if (pos1 >= 0 && pos2 < 0)
                        {
                            pos = pos1;
                        }
                        else if (pos2 >= 0 && pos1 < 0)
                        {
                            pos = pos2;
                            superScript = false;
                        }
                        else
                        {
                            pos = -1;
                        }

                        string subText = String.Empty;
                        if (pos < 0)
                        {
                            subText = text;
                        }
                        if (pos > 0)
                        {
                            subText = text.Substring(0, text.IndexOf(superScript ? "^" : "~"));
                        }

                        using (var subFont = new Font(font.Name, fontSize))
                        {
                            gr.DrawString(subText, subFont, brush, xOffset, yOffset, format);
                            var size = gr.MeasureString(subText, subFont);
                            if (!String.IsNullOrWhiteSpace(subText))
                            {
                                xOffset += size.Width - subFont.Size * .2f;
                            }

                            if (superScript)
                            {
                                yOffset -= subFont.Size * .4f;
                            }
                            else
                            {
                                yOffset += (subFont.Size / .9f) * .4f;
                            }
                        }

                        if (pos >= 0)
                        {
                            text = text.Substring(pos + 1);
                            if (superScript)
                            {
                                fontSize *= .9f;
                            }
                            else
                            {
                                fontSize /= .9f;
                            }
                        }
                        else
                        {
                            text = String.Empty;
                        }
                    }
                }
            }
            else
            {
                gr.DrawString(text, font, brush, xOffset, yOffset, format);
            }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("A5C2BB98-2B2D-4353-9262-4CDF4C7EB845")]
    public class GlowingTextSymbol : SimpleTextSymbol
    {
        protected SolidBrush _outlinebrush;
        private SymbolSmoothing _outlineSmothingMode = SymbolSmoothing.None;
        private int _outlineWidth = 1;

        public GlowingTextSymbol()
            : base()
        {
            _outlinebrush = new SolidBrush(Color.Yellow);
        }
        protected GlowingTextSymbol(Font font, Color color, Color outlinecolor)
            : base(font, color)
        {
            _outlinebrush = new SolidBrush(outlinecolor);
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        [Category("Glowing")]
        public System.Drawing.Color GlowingColor
        {
            get { return _outlinebrush.Color; }
            set { _outlinebrush.Color = value; }
        }

        [Browsable(true)]
        [Category("Glowing")]
        public SymbolSmoothing GlowingSmoothingmode
        {
            get { return _outlineSmothingMode; }
            set { _outlineSmothingMode = value; }
        }

        [Browsable(true)]
        [Category("Glowing")]
        public int GlowingWidth
        {
            get { return _outlineWidth; }
            set { _outlineWidth = value; }
        }

        #region ISymbol Members

        override public void Release()
        {
            base.Release();
            if (_outlinebrush != null)
            {
                _outlinebrush.Dispose();
            }

            _outlinebrush = null;
        }

        [Browsable(false)]
        override public string Name
        {
            get { return "Glowing Text Symbol"; }
        }

        #endregion

        #region IClone2 Members

        override public object Clone(CloneOptions options)
        {
            var display = options?.Display;

            if (display == null)
            {
                return this.Clone();
            }

            float fac = 1;
            if (display.refScale > 1)
            {
                fac = (float)(display.refScale / display.mapScale);
            }

            if (display.dpi != 96.0)
            {
                fac *= (float)(display.dpi / 96.0);
            }

            GlowingTextSymbol tSym = new GlowingTextSymbol(new Font(_font.Name, Math.Max(_font.Size * fac, 2f), _font.Style), _brush.Color, _outlinebrush.Color);
            tSym.HorizontalOffset = HorizontalOffset * fac;
            tSym.VerticalOffset = VerticalOffset * fac;
            tSym.Angle = Angle;
            tSym._align = _align;
            tSym.Smoothingmode = this.Smoothingmode;
            tSym.GlowingSmoothingmode = this.GlowingSmoothingmode;
            tSym.GlowingWidth = this.GlowingWidth;
            tSym.IncludesSuperScript = this.IncludesSuperScript;

            return tSym;
        }
        #endregion

        #region IPersistable Members

        override public void Load(IPersistStream stream)
        {
            base.Load(stream);

            this.GlowingColor = Color.FromArgb((int)stream.Load("outlinecolor", Color.Yellow.ToArgb()));
            this.GlowingSmoothingmode = (SymbolSmoothing)stream.Load("outlinesmoothing", (int)this.Smoothingmode);
            this.GlowingWidth = (int)stream.Load("outlinewidth", 1);
        }

        override public void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("outlinecolor", this.GlowingColor.ToArgb());
            stream.Save("outlinesmoothing", (int)this.GlowingSmoothingmode);
            stream.Save("outlinewidth", this.GlowingWidth);
        }

        #endregion

        #region ISymbol Member

        [Browsable(false)]
        override public SymbolSmoothing SymbolSmothingMode
        {
            set
            {
                this.Smoothingmode = _outlineSmothingMode = value;
            }
        }

        #endregion

        override protected int DrawingLevels { get { return 2; } }
        override protected void DrawAtPoint(IDisplay display, IPoint point, string text, float angle, StringFormat format, int level)
        {
            if (_font != null)
            {
                //point.X+=_xOffset;
                //point.Y+=_yOffset;

                try
                {
                    display.GraphicsContext.TranslateTransform((float)point.X, (float)point.Y);
                    if (angle != 0 || _angle != 0 || _rotation != 0)
                    {
                        display.GraphicsContext.RotateTransform(angle + _angle + _rotation);
                    }

                    display.GraphicsContext.TextRenderingHint = ((this.GlowingSmoothingmode == SymbolSmoothing.None) ? System.Drawing.Text.TextRenderingHint.SystemDefault : System.Drawing.Text.TextRenderingHint.AntiAlias);

                    if (level < 0 || level == 0)
                    {
                        if (_outlineWidth > 0)
                        {
                            for (int x = _outlineWidth; x >= -_outlineWidth; x--)
                            {
                                for (int y = _outlineWidth; y >= -_outlineWidth; y--)
                                {
                                    if (x == 0 && y == 0)
                                    {
                                        continue;
                                    }

                                    DrawString(display.GraphicsContext, text, _font, _outlinebrush, (float)_xOffset + x, (float)_yOffset + y, format);
                                }
                            }
                        }
                    }

                    if (level < 0 || level == 1)
                    {
                        display.GraphicsContext.TextRenderingHint = ((this.Smoothingmode == SymbolSmoothing.None) ? System.Drawing.Text.TextRenderingHint.SystemDefault : System.Drawing.Text.TextRenderingHint.AntiAlias);

                        DrawString(display.GraphicsContext, text, _font, _brush, (float)_xOffset, (float)_yOffset, format);
                    }

                }
                finally
                {
                    display.GraphicsContext.ResetTransform();
                    display.GraphicsContext.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                }
            }
        }
    }

    [RegisterPlugInAttribute("A06F8B12-394E-4F8E-8B9A-6025F96F6F4F")]
    public class BlockoutTextSymbol : SimpleTextSymbol
    {
        protected SolidBrush _outlinebrush;

        public BlockoutTextSymbol()
            : base()
        {
            _outlinebrush = new SolidBrush(Color.Yellow);
        }
        protected BlockoutTextSymbol(Font font, Color color, Color outlinecolor)
            : base(font, color)
        {
            _outlinebrush = new SolidBrush(outlinecolor);
        }

        [Browsable(true)]
        [UseColorPicker()]
        public System.Drawing.Color ColorOutline
        {
            get { return _outlinebrush.Color; }
            set { _outlinebrush.Color = value; }
        }

        #region ISymbol Members

        override public void Release()
        {
            base.Release();
            if (_outlinebrush != null)
            {
                _outlinebrush.Dispose();
            }

            _outlinebrush = null;
        }

        [Browsable(false)]
        override public string Name
        {
            get { return "Blockout Text Symbol"; }
        }

        #endregion

        #region IClone2 Members

        override public object Clone(CloneOptions options)
        {
            var display = options?.Display;

            if (display == null)
            {
                return this.Clone();
            }

            float fac = 1;
            if (display.refScale > 1)
            {
                fac = (float)(display.refScale / display.mapScale);
            }

            if (display.dpi != 96.0)
            {
                fac *= (float)(display.dpi / 96.0);
            }

            BlockoutTextSymbol tSym = new BlockoutTextSymbol(new Font(_font.Name, Math.Max(_font.Size * fac, 2f), _font.Style), _brush.Color, _outlinebrush.Color);
            tSym.HorizontalOffset = HorizontalOffset * fac;
            tSym.VerticalOffset = VerticalOffset * fac;
            tSym.Angle = Angle;
            tSym._align = _align;
            tSym.Smoothingmode = this.Smoothingmode;
            tSym.IncludesSuperScript = this.IncludesSuperScript;

            return tSym;
        }
        #endregion

        #region IPersistable Members

        override public void Load(IPersistStream stream)
        {
            base.Load(stream);

            this.ColorOutline = Color.FromArgb((int)stream.Load("outlinecolor", Color.Yellow.ToArgb()));
        }

        override public void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("outlinecolor", this.ColorOutline.ToArgb());
        }

        #endregion

        override protected int DrawingLevels { get { return 2; } }
        override protected void DrawAtPoint(IDisplay display, IPoint point, string text, float angle, StringFormat format, int level)
        {
            if (_font != null)
            {
                //point.X+=_xOffset;
                //point.Y+=_yOffset;

                try
                {
                    display.GraphicsContext.TranslateTransform((float)point.X, (float)point.Y);
                    if (angle != 0 || _angle != 0 || _rotation != 0)
                    {
                        display.GraphicsContext.RotateTransform(angle + _angle + _rotation);
                    }

                    if (level < 0 || level == 0)
                    {
                        SizeF size = display.GraphicsContext.MeasureString(text, _font);
                        RectangleF rect = new RectangleF(0f, 0f, size.Width, size.Height);
                        switch (format.Alignment)
                        {
                            case StringAlignment.Center:
                                rect.Offset(-size.Width / 2, 0f);
                                break;
                            case StringAlignment.Far:
                                rect.Offset(-size.Width, 0f);
                                break;
                        }
                        switch (format.LineAlignment)
                        {
                            case StringAlignment.Center:
                                rect.Offset(0f, -size.Height / 2);
                                break;
                            case StringAlignment.Far:
                                rect.Offset(0f, -size.Height);
                                break;
                        }

                        display.GraphicsContext.SmoothingMode = (System.Drawing.Drawing2D.SmoothingMode)this.Smoothingmode;
                        display.GraphicsContext.FillRectangle(_outlinebrush, rect);
                        display.GraphicsContext.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    }

                    if (level < 0 || level == 1)
                    {
                        display.GraphicsContext.TextRenderingHint = ((this.Smoothingmode == SymbolSmoothing.None) ? System.Drawing.Text.TextRenderingHint.SystemDefault : System.Drawing.Text.TextRenderingHint.AntiAlias);

                        DrawString(display.GraphicsContext, text, _font, _brush, (float)_xOffset, (float)_yOffset, format);

                        display.GraphicsContext.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                    }
                }
                finally
                {
                    display.GraphicsContext.ResetTransform();
                }
            }
        }
    }
}

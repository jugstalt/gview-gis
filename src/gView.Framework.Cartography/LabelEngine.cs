#nullable enable

using gView.Framework.Cartography.Extensions;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Data;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace gView.Framework.Cartography;

internal class LabelEngine2 : ILabelEngine, IDisposable
{
    private enum OverlapAlgorirthm { Pixel = 1, Geometry = 0 };

    private const int GlobalGridId = -1;
    private const OverlapAlgorirthm Algorithm = OverlapAlgorirthm.Geometry;

    private GraphicsEngine.Abstraction.IBitmap? _bitmap = null;
    //private Display _display;
    private GraphicsEngine.Abstraction.ICanvas? _canvas = null;
    private GraphicsEngine.ArgbColor _back;
    private bool _first = true, _directDraw = false;
    private Dictionary<int, GridArray<List<IAnnotationPolygonCollision>>> _gridArrayPolygonsDict = new();

    public LabelEngine2()
    {
        //_display = new Display(false);
    }

    public void Dispose()
    {
        if (_canvas != null)
        {
            _canvas.Dispose();
            _canvas = null;
        }
        if (_bitmap != null)
        {
            _bitmap.Dispose();
            _bitmap = null;
        }
        _gridArrayPolygonsDict.Clear();
    }

    #region ILabelEngine Member

    public void Init(IDisplay display, bool directDraw)
    {
        try
        {
            if (display == null)
            {
                return;
            }
            //if (_bm != null && (_bm.Width != display.iWidth || _bm.Height != display.iHeight))
            {
                Dispose();
            }

            if (_bitmap == null)
            {
                _bitmap = GraphicsEngine.Current.Engine.CreateBitmap(display.ImageWidth, display.ImageHeight, GraphicsEngine.PixelFormat.Rgba32);
            }

            _canvas = _bitmap.CreateCanvas();

            //using (var brush = GraphicsEngine.Current.Engine.CreateSolidBrush(GraphicsEngine.ArgbColor.Transparent))
            //{
            //    _canvas.FillRectangle(brush, new GraphicsEngine.CanvasRectangle(0, 0, _bitmap.Width, _bitmap.Height));
            //}
            _bitmap.MakeTransparent();

            _back = _bitmap.GetPixel(0, 0);
            _first = true;
            _directDraw = directDraw;
            //_bm.MakeTransparent(Color.White);

            CreateGrid(display);
        }
        catch
        {
            Dispose();
        }
    }

    public LabelAppendResult TryAppend(
            IDisplay display,
            IFeatureLayer layer,
            ITextSymbol symbol,
            IGeometry? geometry,
            bool checkForOverlap
        )
    {
        if (symbol == null || !(display is Display))
        {
            return LabelAppendResult.WrongArguments;
        }

        if (display.GeometricTransformer != null && !(geometry is IDisplayPath))
        {
            geometry = display.GeometricTransformer.Transform2D(geometry) as IGeometry;
            if (geometry == null)
            {
                return LabelAppendResult.WrongArguments;
            }
        }

        var labelAppendResult = LabelAppendResult.Succeeded;
        var labelAppendWithSpacingResult = LabelAppendResult.Succeeded;

        IAnnotationPolygonCollision? appendPolygon = null;
        IAnnotationPolygonCollision? appendPolygonWithSpacing = null;

        TextSymbolAlignment alignment = TextSymbolAlignment.Center;

        if (symbol is ILabel)
        {
            foreach (var symbolAlignment in LabelAlignments(symbol))
            {
                #region Reset values

                labelAppendResult = LabelAppendResult.Succeeded;
                labelAppendWithSpacingResult = LabelAppendResult.Succeeded;

                appendPolygon = null;
                appendPolygonWithSpacing = null;

                #endregion

                #region Check with all existing labels

                var labelPolygons = symbol.AnnotationPolygon(display, geometry, symbolAlignment) ?? [];
                
                foreach (var labelPolygon in labelPolygons)
                {
                    labelAppendResult = AppendResult(display, GlobalGridId, symbol, labelPolygon, checkForOverlap, symbolAlignment);
                    if(labelAppendResult == LabelAppendResult.Succeeded)
                    {
                        appendPolygon = labelPolygon;
                        break;
                    }
                }

                #endregion

                #region Check with other labels from layer, if there is a symbol spacing

                if (symbol is ISymbolSpacing spacing
                    && spacing.SymbolSpacingType != SymbolSpacingType.None
                    && (spacing.SymbolSpacingX > 0f || spacing.SymbolSpacingY > 0f))
                {
                    //
                    // Symbolspacing: Check if other features of this layers are close
                    //
                    var labelPolygonsWithSpacing = labelPolygons
                        .Select(p => p.WithSpacing(
                                            spacing.SymbolSpacingType,
                                            spacing.SymbolSpacingX, 
                                            spacing.SymbolSpacingY
                                        )
                        )
                        .ToList();

                    foreach (var labelPolygonWithSpacing in labelPolygonsWithSpacing)
                    {
                        labelAppendWithSpacingResult = AppendResult(display, layer.ID, symbol, labelPolygonWithSpacing, checkForOverlap, symbolAlignment);
                        if (labelAppendWithSpacingResult == LabelAppendResult.Succeeded)
                        {
                            appendPolygonWithSpacing = labelPolygonWithSpacing;
                            break;
                        }
                    }
                }

                #endregion


                if (labelAppendResult == LabelAppendResult.Succeeded
                    && labelAppendWithSpacingResult == LabelAppendResult.Succeeded)
                {
                    alignment = symbolAlignment;
                    break;  // if both succeeded => bingo  
                }
            }
        }

        if (labelAppendResult == LabelAppendResult.Succeeded
            && labelAppendWithSpacingResult == LabelAppendResult.Succeeded)
        {
            if (appendPolygon != null)
            {
                List<IAnnotationPolygonCollision> indexedPolygons = GetGrid(display, GlobalGridId)[appendPolygon.Envelope.ToEnvelope()];
                indexedPolygons.Add(appendPolygon);
            }

            if (appendPolygonWithSpacing != null)
            {
                List<IAnnotationPolygonCollision> indexedPolygons = GetGrid(display, layer.ID)[appendPolygonWithSpacing.Envelope.ToEnvelope()];
                indexedPolygons.Add(appendPolygonWithSpacing);
            }

            var originalCanvas = display.Canvas;
            ((Display)display).Canvas = _canvas;

            symbol.Draw(display, geometry, alignment);

            ((Display)display).Canvas = originalCanvas;

            if (_directDraw)
            {
                symbol.Draw(display, geometry, alignment);
            }
        }

        return labelAppendResult;
    }

    public LabelAppendResult TryAppend(
            IDisplay display,
            IFeatureLayer layer,
            List<IAnnotationPolygonCollision> aPolygons,
            IGeometry geometry,
            bool checkForOverlap
        )
    {
        if (_bitmap is null)
        {
            return LabelAppendResult.Outside;
        }
        bool outside = true;
        IAnnotationPolygonCollision? labelPolyon = null;
        IEnvelope? labelPolyonEnvelope = null;

        if (aPolygons != null)
        {
            foreach (IAnnotationPolygonCollision polyCollision in aPolygons)
            {
                AnnotationPolygonEnvelope env = polyCollision.Envelope;

                int minx = (int)Math.Max(0, env.MinX);
                int maxx = (int)Math.Min(_bitmap.Width - 1, env.MaxX);
                int miny = (int)Math.Max(0, env.MinY);
                int maxy = (int)Math.Min(_bitmap.Height, env.MaxY);

                if (minx > _bitmap.Width || maxx <= 0 || miny > _bitmap.Height || maxy <= 0)
                {
                    continue;  // liegt außerhalb!!
                }

                outside = false;

                if (!_first && checkForOverlap)
                {
                    //if (Algorithm == OverlapAlgorirthm.Pixel)
                    //{
                    //    #region Pixel Methode

                    //    for (int x = minx; x < maxx; x++)
                    //    {
                    //        for (int y = miny; y < maxy; y++)
                    //        {
                    //            //if (x < 0 || x >= _bm.Width || y < 0 || y >= _bm.Height) continue;

                    //            if (polyCollision.Contains(x, y))
                    //            {
                    //                //_bm.SetPixel(x, y, Color.Yellow);
                    //                if (!_back.Equals(_bitmap.GetPixel(x, y)))
                    //                {
                    //                    return LabelAppendResult.Overlap;
                    //                }
                    //            }
                    //        }
                    //    }

                    //    #endregion
                    //}
                    //else
                    {
                        #region Geometrie Methode
                        labelPolyon = polyCollision;
                        foreach (List<IAnnotationPolygonCollision> indexedPolygons in GetGrid(display).Collect(new Envelope(env.MinX, env.MinY, env.MaxX, env.MaxY)))
                        {
                            foreach (IAnnotationPolygonCollision lp in indexedPolygons)
                            {
                                if (lp.CheckCollision(polyCollision) == true)
                                {
                                    return LabelAppendResult.Overlap;
                                }
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    _first = false;

                    if (Algorithm == OverlapAlgorirthm.Geometry)
                    {
                        #region Geometrie Methode

                        labelPolyon = polyCollision;

                        #endregion
                    }
                }
                labelPolyonEnvelope = new Envelope(env.MinX, env.MinY, env.MaxX, env.MaxY);
            }
            if (outside)
            {
                return LabelAppendResult.Outside;
            }
        }


        if (labelPolyon != null)
        {
            List<IAnnotationPolygonCollision> indexedPolygons = GetGrid(display)[labelPolyonEnvelope];
            indexedPolygons.Add(labelPolyon);
        }

        return LabelAppendResult.Succeeded;
    }

    public void Draw(IDisplay display, ICancelTracker cancelTracker)
    {
        try
        {

            display.Canvas.DrawBitmap(_bitmap, new GraphicsEngine.CanvasPoint(0, 0));

            //_bm.Save(@"c:\temp\label.png", System.Drawing.Imaging.ImageFormat.Png);
        }
        catch { }
    }

    public void Release()
    {
        Dispose();
    }

    public GraphicsEngine.Abstraction.ICanvas? LabelCanvas
    {
        get { return _canvas; }
    }

    #endregion

    #region Helper

    private LabelAppendResult AppendResult(
                    IDisplay display, 
                    int id, 
                    ITextSymbol symbol,
                    IAnnotationPolygonCollision aPolygon, 
                    bool checkForOverlap, 
                    TextSymbolAlignment symbolAlignment
            )
    {
        bool outside = true;

        if (aPolygon != null && _bitmap != null)
        {
            AnnotationPolygonEnvelope env = aPolygon.Envelope;

            #region Check Outside

            if (id == GlobalGridId)
            {
                if (env.MinX < 0 || env.MinY < 0 || env.MaxX > _bitmap.Width || env.MaxY > _bitmap.Height)
                {
                    return LabelAppendResult.Outside;
                }
            }

            #endregion

            //int minx = (int)env.MinX, miny = (int)env.MinX, maxx = (int)env.MaxX, maxy = (int)env.MaxY;

            outside = false;

            if (!_first && checkForOverlap)
            {
                //if (Algorithm == OverlapAlgorirthm.Pixel)
                //{
                //    #region Pixel Methode

                //    for (int x = minx; x < maxx; x++)
                //    {
                //        for (int y = miny; y < maxy; y++)
                //        {
                //            //if (x < 0 || x >= _bm.Width || y < 0 || y >= _bm.Height) continue;

                //            if (polyCollision.Contains(x, y))
                //            {
                //                //_bm.SetPixel(x, y, Color.Yellow);
                //                if (!_back.Equals(_bitmap.GetPixel(x, y)))
                //                {
                //                    return LabelAppendResult.Overlap;
                //                }
                //            }
                //        }
                //    }

                //    #endregion
                //}
                //else
                {
                    #region Geometrie Methode

                    foreach (List<IAnnotationPolygonCollision> indexedPolygons in GetGrid(display, id).Collect(new Envelope(env.MinX, env.MinY, env.MaxX, env.MaxY)))
                    {
                        foreach (IAnnotationPolygonCollision lp in indexedPolygons)
                        {
                            if (lp.CheckCollision(aPolygon) == true)
                            {
                                return LabelAppendResult.Overlap;
                            }
                        }
                    }

                    #endregion
                }
            }
            else
            {
                _first = false;
            }
        }


        if (outside)
        {
            return LabelAppendResult.Outside;
        }

        return LabelAppendResult.Succeeded;
    }

    private TextSymbolAlignment[] LabelAlignments(ILabel label)
    {
        return label.SecondaryTextSymbolAlignments != null && label.SecondaryTextSymbolAlignments.Length > 0 ?
            label.SecondaryTextSymbolAlignments :
            new TextSymbolAlignment[] { label.TextSymbolAlignment };
    }

    private GridArray<List<IAnnotationPolygonCollision>> CreateGrid(IDisplay display, int id = GlobalGridId)
    {
        if (_gridArrayPolygonsDict.ContainsKey(id)) return _gridArrayPolygonsDict[id];

        var annotationPolygons = new GridArray<List<IAnnotationPolygonCollision>>(
                                    new Envelope(0.0, 0.0, display.ImageWidth, display.ImageHeight),
                                    new int[] { 50, 25, 18, 10, 5, 2 },
                                    new int[] { 50, 25, 18, 10, 5, 2 }
                                 );

        _gridArrayPolygonsDict[id] = annotationPolygons;

        return annotationPolygons;
    }

    private GridArray<List<IAnnotationPolygonCollision>> GetGrid(IDisplay display, int id = GlobalGridId)
    {
        return _gridArrayPolygonsDict.ContainsKey(id)
            ? _gridArrayPolygonsDict[id]
            : CreateGrid(display, id);
    }

    #endregion

    #region Helper Classes

    private class LabelPolygon
    {
        PointF[] _points;

        public LabelPolygon(PointF[] points)
        {
            _points = points;
        }

        public bool CheckCollision(LabelPolygon lp)
        {
            if (HasSeperateLine(this, lp))
            {
                return false;
            }

            if (HasSeperateLine(lp, this))
            {
                return false;
            }

            return true;
        }

        public PointF this[int index]
        {
            get
            {
                if (index < 0 || index >= _points.Length)
                {
                    return _points[0];
                }

                return _points[index];
            }
        }
        private static bool HasSeperateLine(LabelPolygon tester, LabelPolygon cand)
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

                if (t_min <= c_max && t_max <= c_min ||
                    c_min <= t_max && c_max <= t_min)
                {
                    return true;
                }
            }

            return false;
        }

        private static void MinMaxAreaForOrhtoSepLine(PointF p1, Vector2dF ortho, LabelPolygon lp, ref float min, ref float max)
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

        public IEnvelope Envelope
        {
            get
            {
                double
                    minx = _points[0].X,
                    miny = _points[0].Y,
                    maxx = _points[0].X,
                    maxy = _points[0].Y;
                for (int i = 1; i < _points.Length; i++)
                {
                    minx = Math.Min(minx, _points[i].X);
                    miny = Math.Min(miny, _points[i].Y);
                    maxx = Math.Max(maxx, _points[i].X);
                    maxy = Math.Max(maxy, _points[i].Y);
                }

                return new Envelope(minx, miny, maxx, maxy);
            }
        }

        #region Helper Classes
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

    #endregion
}

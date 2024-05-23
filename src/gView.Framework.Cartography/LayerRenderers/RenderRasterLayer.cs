using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using gView.Framework.Geometry;
using gView.GraphicsEngine.Filters;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Cartography.LayerRenderers
{
    public sealed class RenderRasterLayer
    {
        private Map _map;
        private IMapRenderer _mapRenderer;
        private IRasterLayer _layer;
        private ICancelTracker _cancelTracker;
        private InterpolationMethod _interpolMethod = InterpolationMethod.Fast;
        private FilterImplementations _filter = FilterImplementations.Default;
        private float _transparency = 0.0f;
        private GraphicsEngine.ArgbColor _transColor = GraphicsEngine.ArgbColor.Transparent;

        public RenderRasterLayer(Map map, 
            IRasterLayer layer, 
            IRasterLayer rLayer, 
            ICancelTracker cancelTracker,
            IMapRenderer mapRenderer = null)
        {
            _map = map;
            _layer = layer;
            _cancelTracker = cancelTracker;
            if (rLayer != null)
            {
                _interpolMethod = rLayer.InterpolationMethod;
                _transparency = rLayer.Opacity;
                _transColor = rLayer.TransparentColor;
                _filter = rLayer.FilterImplementation;
            }
            _mapRenderer = mapRenderer;
        }

        // Thread
        async public Task Render()
        {
            GraphicsEngine.Abstraction.IBitmap _filteredBitmap = null, _nodataFilteredBitmap = null;

            try
            {
                if (_layer == null || _map == null || _cancelTracker == null)
                {
                    return;
                }

                if (_layer.RasterClass.Polygon == null)
                {
                    return;
                }

                IEnvelope env = _layer.RasterClass.Polygon.Envelope;
                double minx = env.MinX, miny = env.MinY, maxx = env.MaxX, maxy = env.MaxY;

                _map.World2Image(ref minx, ref miny);
                _map.World2Image(ref maxx, ref maxy);

                int iWidth = 0, iHeight = 0;
                int min_x = Math.Max(0, (int)Math.Min(minx, maxx) - 1);
                int min_y = Math.Max(0, (int)Math.Min(miny, maxy) - 1);
                int max_x = Math.Min(iWidth = _map.ImageWidth, (int)Math.Max(minx, maxx) + 1);
                int max_y = Math.Min(iHeight = _map.ImageHeight, (int)Math.Max(miny, maxy) + 1);


                using (var paintContext = await _layer.RasterClass.BeginPaint(_map.Display, _cancelTracker))
                {
                    if(paintContext?.Bitmap == null)
                    {
                        return;  // No Data...eg. Tile not exists... 
                    }

                    if (_filter != FilterImplementations.Default && paintContext != null)
                    {
                        _filteredBitmap = BaseFilter.ApplyFilter(paintContext.Bitmap, _filter);
                    }
                    if (_transColor.ToArgb() != System.Drawing.Color.Transparent.ToArgb())
                    {
                        _nodataFilteredBitmap = BaseFilter.ApplyEraseColorFilter(
                            _filteredBitmap ?? paintContext.Bitmap, _transColor);
                    }
                    
                    if (paintContext?.Bitmap == null 
                        && _filteredBitmap == null)
                    {
                        return;
                    }

                    //System.Windows.Forms.MessageBox.Show("begin");

                    double W = _map.Envelope.MaxX - _map.Envelope.MinX;
                    double H = _map.Envelope.MaxY - _map.Envelope.MinY;
                    double MinX = _map.Envelope.MinX;
                    double MinY = _map.Envelope.MinY;

                    //_lastRasterLayer = _layer;

                    var canvas = _map.Display.Canvas;
                    if (canvas == null)
                    {
                        return;
                    }

                    canvas.InterpolationMode = (GraphicsEngine.InterpolationMode)_interpolMethod;

                    // Transformation berechnen
                    GraphicsEngine.CanvasRectangleF rect;
                    switch (canvas.InterpolationMode)
                    {
                        case GraphicsEngine.InterpolationMode.Bilinear:
                        case GraphicsEngine.InterpolationMode.Bicubic:
                            rect = new GraphicsEngine.CanvasRectangleF(0, 0, paintContext.Bitmap.Width - 1f, paintContext.Bitmap.Height - 1f);
                            break;
                        case GraphicsEngine.InterpolationMode.NearestNeighbor:
                            rect = new GraphicsEngine.CanvasRectangleF(-0.5f, -0.5f, paintContext.Bitmap.Width, paintContext.Bitmap.Height);
                            //rect = new GraphicsEngine.CanvasRectangleF(0f, 0f, _layer.RasterClass.Bitmap.Width, _layer.RasterClass.Bitmap.Height);
                            break;
                        default:
                            rect = new GraphicsEngine.CanvasRectangleF(0, 0, paintContext.Bitmap.Width, paintContext.Bitmap.Height);
                            break;
                    }

                    var points = new GraphicsEngine.CanvasPointF[3];

                    if (paintContext is IRasterPointContext2)
                    {
                        IPoint p1 = ((IRasterPointContext2)paintContext).PicPoint1;
                        IPoint p2 = ((IRasterPointContext2)paintContext).PicPoint2;
                        IPoint p3 = ((IRasterPointContext2)paintContext).PicPoint3;

                        if (_map.Display.GeometricTransformer != null)
                        {
                            p1 = (IPoint)_map.Display.GeometricTransformer.Transform2D(p1);
                            p2 = (IPoint)_map.Display.GeometricTransformer.Transform2D(p2);
                            p3 = (IPoint)_map.Display.GeometricTransformer.Transform2D(p3);
                        }

                        double X = p1.X, Y = p1.Y;
                        _map.Display.World2Image(ref X, ref Y);
                        points[0] = new GraphicsEngine.CanvasPointF(ToPixelFloat(X), ToPixelFloat(Y));

                        X = p2.X; Y = p2.Y;
                        _map.Display.World2Image(ref X, ref Y);
                        points[1] = new GraphicsEngine.CanvasPointF(ToPixelFloat(X), ToPixelFloat(Y));

                        X = p3.X; Y = p3.Y;
                        _map.Display.World2Image(ref X, ref Y);
                        points[2] = new GraphicsEngine.CanvasPointF(ToPixelFloat(X), ToPixelFloat(Y));

                        RoundGraphicPixelPoints(points);
                    }
                    else
                    {
                        double X1 = _layer.RasterClass.oX - _layer.RasterClass.dx1 / 2.0 - _layer.RasterClass.dy1 / 2.0;
                        double Y1 = _layer.RasterClass.oY - _layer.RasterClass.dx2 / 2.0 - _layer.RasterClass.dy2 / 2.0;
                        double X = X1;
                        double Y = Y1;
                        if (_map.Display.GeometricTransformer != null)
                        {
                            IPoint p = (IPoint)_map.Display.GeometricTransformer.Transform2D(new Point(X, Y));
                            X = p.X; Y = p.Y;
                        }

                        _map.Display.World2Image(ref X, ref Y);
                        points[0] = new GraphicsEngine.CanvasPointF(ToPixelFloat(X), ToPixelFloat(Y));
                        X = X1 + paintContext.Bitmap.Width * _layer.RasterClass.dx1;
                        Y = Y1 + paintContext.Bitmap.Width * _layer.RasterClass.dx2;
                        if (_map.Display.GeometricTransformer != null)
                        {
                            IPoint p = (IPoint)_map.Display.GeometricTransformer.Transform2D(new Point(X, Y));
                            X = p.X; Y = p.Y;
                        }

                        _map.Display.World2Image(ref X, ref Y);
                        points[1] = new GraphicsEngine.CanvasPointF(ToPixelFloat(X), ToPixelFloat(Y));
                        X = X1 + paintContext.Bitmap.Height * _layer.RasterClass.dy1;
                        Y = Y1 + paintContext.Bitmap.Height * _layer.RasterClass.dy2;
                        if (_map.Display.GeometricTransformer != null)
                        {
                            IPoint p = (IPoint)_map.Display.GeometricTransformer.Transform2D(new Point(X, Y));
                            X = p.X; Y = p.Y;
                        }

                        _map.Display.World2Image(ref X, ref Y);
                        points[2] = new GraphicsEngine.CanvasPointF(ToPixelFloat(X), ToPixelFloat(Y));
                    }

                    //if (_transColor.ToArgb() != System.Drawing.Color.Transparent.ToArgb())
                    //{
                    //    try
                    //    {
                    //        // kann OutOfMemoryException auslösen...
                    //        paintContext.Bitmap.MakeTransparent(_transColor);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        if (_map is IServiceMap && ((IServiceMap)_map).MapServer != null)
                    //        {
                    //            await ((IServiceMap)_map).MapServer.LogAsync(
                    //                ((IServiceMap)_map).Name,
                    //                "RenderRasterLayerThread: " + (_layer != null ? _layer.Title : string.Empty),
                    //                loggingMethod.error,
                    //                ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                    //        }
                    //        if (_map != null)
                    //        {
                    //            if (_map != null)
                    //            {
                    //                _map.AddRequestException(new Exception("RenderRasterLayerThread: " + (_layer != null ? _layer.Title : string.Empty) + "\n" + ex.Message, ex));
                    //            }
                    //        }
                    //    }
                    //}

                    //var comQual = gr.CompositingQuality;
                    //gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    float opaque = 1.0f - _transparency;

                    canvas.DrawBitmap(
                                  _nodataFilteredBitmap ?? _filteredBitmap ?? paintContext.Bitmap,
                                  points,
                                  rect,
                                  opacity: opaque);

                    _mapRenderer?.FireRefreshMapView(DrawPhase.Geography);
                }
            }
            catch (Exception ex)
            {
                if (_map is IServiceMap && ((IServiceMap)_map).MapServer != null)
                {
                    await ((IServiceMap)_map).MapServer.LogAsync(
                        ((IServiceMap)_map).Name,
                        "RenderRasterLayerThread:" + (_layer != null ? _layer.Title : string.Empty), loggingMethod.error,
                        ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                }
                if (_map != null)
                {
                    if (_map != null)
                    {
                        _map.AddRequestException(new Exception("RenderRasterLayerThread: " + (_layer != null ? _layer.Title : string.Empty) + "\n" + ex.Message, ex));
                    }
                }
            }
            finally
            {
                if(_nodataFilteredBitmap != null)
                {
                    _nodataFilteredBitmap.Dispose();
                }
                if (_filteredBitmap != null)
                {
                    _filteredBitmap.Dispose();
                }
            }
        }

        private float ToPixelFloat(double d)
        {
            return (float)d;
            //return (float)Math.Round(d, 2);
        }

        private void RoundGraphicPixelPoints(GraphicsEngine.CanvasPointF[] points)
        {
            float espsi = .1f;
            if (points.Length == 3)
            {
                points[0].X -= espsi;
                points[0].Y -= espsi;

                points[1].X += espsi;
                points[1].Y -= espsi;

                points[2].X -= espsi;
                points[2].Y += espsi;
            }
        }
    }
}

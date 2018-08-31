using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.Carto.UI;

[assembly: InternalsVisibleTo("gView.UI, PublicKey=002400000480000094000000060200000024000052534131000400000100010027d158e4f3030aadf57ddda2411853dec8352ea7e7c0149397393c35543c045619e02342cf7d9dff459c3a830ec2507659a8a058ef166ed1d06e679a7ded94a5b8454475e6330728784ffa18eb70beb35d92b736e0adca8124f61b6c232c222cd9efdb65e3bb407be4b568a1b1539e93cb73111dd95b9edc19227cb81fe63fd3")]
[assembly: InternalsVisibleTo("gView.Plugins.Tools, PublicKey=0024000004800000940000000602000000240000525341310004000001000100cd0164638c075bd9b7732f3f3fe1ee7e2c8bde2602441fc5cb85782a15ce73f69b46d6551e233d6d9001faa181d158a80c4f04508f445c74ee51210313d434c2183ab2244921c10840fef30b4462a0ef79e079279098c304451c870d880858d4e89a71e8c8c67f636b15d7f827315286fb7410bae9fe7c54a030fd0de24605ee")]
[assembly: InternalsVisibleTo("gView.MapServer.Instance, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a7ec0d3d800f81ef46df179314e9ed3b541b4ed1af5cb902907cdf1cbbe2bc7fc1b38af6058d48fd79c5c914b4d4f015b81d727d33bfd1147cc4c3bab99f86a1549cdaba0ea5d32c2539f7c3ea9892e86700cf851eaaf09c1a64a89869df8a2f1014d28c88493223db354ff23571234f1fa1cb5c4e9a51d172257b0cc0d965d0")]

namespace gView.Framework.Carto
{
    internal sealed class PrinterMap : Map
    {
        public override event DoRefreshMapViewEvent DoRefreshMapView;
        public override event DrawingLayerEvent DrawingLayer;
        private PrinterScreen _screen;

        public PrinterMap(Map original)
        {
            _screen = new PrinterScreen();

            _datasets = original._datasets;
            m_imageMerger = new ImageMerger2();

            m_name = original.Name;
            _toc = (TOC)original.TOC;

            this.Limit = original.Limit;

            this.Display.MapUnits = original.Display.MapUnits;
            this.Display.DisplayUnits = original.Display.DisplayUnits;
            
            this.Display.mapScale = original.Display.mapScale;
            this.Display.refScale = original.refScale;
            this.Display.SpatialReference = original.SpatialReference !=null ? original.SpatialReference.Clone() as ISpatialReference : null;

            foreach (IGraphicElement grElement in original.Display.GraphicsContainer.Elements)
            {
                this.Display.GraphicsContainer.Elements.Add(grElement);
            }
        }

        public void SetOrigin(float OX, float OY)
        {
            _OX = OX;
            _OY = OY;
        }

        /*
        public override bool RefreshMap(DrawPhase phase, ICancelTracker cancelTracker)
        {
            this.ZoomTo(m_actMinX, m_actMinY, m_actMaxX, m_actMaxY);

            if (cancelTracker == null) cancelTracker = new CancelTracker();

            if (phase == DrawPhase.All || phase == DrawPhase.Geography)
            {
                GeometricTransformer geoTransformer = new GeometricTransformer();
                geoTransformer.ToSpatialReference = this.SpatialReference;
                this.GeometricTransformer = geoTransformer;

                // Thread für MapServer Datasets starten...
                foreach (IDatasetElement element in this.MapElements)
                {
                    if (element is IWebServiceLayer)
                    {
                        if (!((ILayer)element).Visible) continue;
                        ServiceRequestThread srt = new ServiceRequestThread(this, element as IWebServiceLayer);
                        srt.finish += new ServiceRequestThread.RequestThreadFinished(MapRequestThread_finished);
                        Thread thread = new Thread(new ThreadStart(srt.ImageRequest));
                        m_imageMerger.max++;
                        thread.Start();
                    }
                }

                List<ILayer> layers=new List<ILayer>();
                if (this.TOC != null)
                {
                    if (this.ToString() == "gView.MapServer.Instance.ServiceMap")
                        layers = ListOperations<ILayer>.Swap(this.TOC.Layers);
                    else
                        layers = ListOperations<ILayer>.Swap(this.TOC.VisibleLayers);
                }
                else
                {
                    layers = new List<ILayer>();
                    foreach (IDatasetElement layer in this.MapElements)
                    {
                        if (!(layer is ILayer)) continue;
                        if (((ILayer)layer).Visible)
                            layers.Add((ILayer)layer);
                    }
                }
                
                foreach (ILayer element in layers)
                {
                    if (!(element is ILayer)) continue;
                    ILayer layer = (ILayer)element;

                    if (layer.MinimumScale > 1 && layer.MinimumScale > this.mapScale) continue;
                    if (layer.MaximumScale > 1 && layer.MaximumScale < this.mapScale) continue;

                    Thread thread = null;
                    if (layer is IFeatureLayer)
                    {
                        IFeatureLayer fLayer = (IFeatureLayer)layer;
                        if (fLayer.FeatureRenderer == null) continue;

                        RenderFeatureLayerThread rlt = new RenderFeatureLayerThread(this, fLayer, cancelTracker);
                        //rlt.Render();

                        thread = new Thread(new ThreadStart(rlt.Render));
                        thread.Start();

                        if (DrawingLayer != null && cancelTracker.Continue) DrawingLayer(layer.Title);
                    }
                    else if (layer is IRasterLayer && ((IRasterLayer)layer).RasterClass!=null)
                    {
                        IRasterLayer rLayer = (IRasterLayer)layer;
                        if (rLayer.RasterClass.Polygon == null) continue;

                        if (gView.SpatialAlgorithms.Algorithm.IntersectBox(rLayer.RasterClass.Polygon, this.Envelope))
                        {
                            if (rLayer is IParentRasterLayer)
                            {
                                DrawRasterParentLayer((IParentRasterLayer)rLayer, cancelTracker, rLayer);
                                thread = null;
                            }
                            else
                            {
                                RenderRasterLayerThreadPrint rlt = new RenderRasterLayerThreadPrint(this, rLayer, rLayer, cancelTracker);

                                thread = new Thread(new ThreadStart(rlt.Render));
                                thread.Start();

                                if (DrawingLayer != null && cancelTracker.Continue) DrawingLayer(layer.Title);
                            }
                        }
                    }
                    // Andere Layer (zB IRasterLayer)

                    if (thread == null) continue;
                    int count = 0;

                    while (thread.IsAlive)
                    {
                        Thread.Sleep(10);
                        if (DoRefreshMapView != null && (count % 100) == 0 && cancelTracker.Continue) DoRefreshMapView();
                        count++;
                    }
                    if (DoRefreshMapView != null && cancelTracker.Continue) DoRefreshMapView();
                }

                while (m_imageMerger.Count < m_imageMerger.max)
                {
                    Thread.Sleep(100);
                }
                if (_drawScaleBar)
                {
                    m_imageMerger.mapScale = this.mapScale;
                    m_imageMerger.dpi = this.dpi;
                }
                m_imageMerger.Merge(_image);
                m_imageMerger.clear();

                if (DoRefreshMapView != null && cancelTracker.Continue) DoRefreshMapView();
                //System.Windows.Forms.MessageBox.Show(mapScale.ToString());
            }
            if (phase == DrawPhase.All || phase == DrawPhase.Selection)
            {
                foreach (IDatasetElement layer in this.MapElements)
                {
                    Thread thread = null;
                    if (layer is IFeatureLayer && layer is IFeatureSelection)
                    {
                        IFeatureLayer fLayer = (IFeatureLayer)layer;
                        if (fLayer.SelectionRenderer == null) continue;
                        IFeatureSelection fSelection = (IFeatureSelection)layer;
                        if (fSelection.SelectionSet == null) continue;

                        RenderFeatureLayerSelectionThread rlt = new RenderFeatureLayerSelectionThread(this, fLayer, cancelTracker);
                        //rlt.Render();

                        thread = new Thread(new ThreadStart(rlt.Render));
                        thread.Start();

                        if (DrawingLayer != null && cancelTracker.Continue) DrawingLayer(layer.Title);
                    } // Andere Layer (zB IRasterLayer)

                    if (thread == null) continue;
                    int count = 0;

                    while (thread.IsAlive)
                    {
                        Thread.Sleep(10);
                        if (DoRefreshMapView != null && (count % 100) == 0 && cancelTracker.Continue)
                        {
                            DoRefreshMapView();
                        }
                        count++;
                    }
                    if (DoRefreshMapView != null && cancelTracker.Continue) DoRefreshMapView();
                }
            }

            if (this.GeometricTransformer != null)
            {
                this.GeometricTransformer.Release();
                this.GeometricTransformer = null;
            }
            return true;
        }
        */

        protected override void DrawStream(System.IO.Stream stream)
        {
            
        }
        protected override void StreamImage(ref System.IO.MemoryStream stream, System.Drawing.Image image)
        {
           
        }

        /*
        protected override void DrawRasterParentLayer(IParentRasterLayer rLayer, ICancelTracker cancelTracker,IRasterLayer rootLayer)
        {
            double dpi = this.Display.dpi;
            this.dpi = Math.Max(this.Display.GraphicsContext.DpiX, this.Display.GraphicsContext.DpiY);
            
            using(IRasterLayerCursor cursor=rLayer.ChildLayers(this,String.Empty))
            
            foreach (ILayer child in ((IParentRasterLayer)rLayer).ChildLayers(this, ""))
            {
                if (!cancelTracker.Continue) break;
                if (child is IParentRasterLayer)
                {
                    DrawRasterParentLayer((IParentRasterLayer)child, cancelTracker, rootLayer);
                    continue;
                }
                if (!(child is IRasterLayer)) continue;
                IRasterLayer cLayer = (IRasterLayer)child;

                RenderRasterLayerThreadPrint rlt = new RenderRasterLayerThreadPrint(this, cLayer, rootLayer, cancelTracker);

                Thread thread = new Thread(new ThreadStart(rlt.Render));
                thread.Start();

                if (DrawingLayer != null && cancelTracker.Continue)
                {
                    if (rLayer is ILayer) DrawingLayer(((ILayer)rLayer).Title);
                }
                // WarteSchleife
                int counter = 0;

                while (thread.IsAlive)
                {
                    Thread.Sleep(100);
                    if (DoRefreshMapView != null && (counter % 10) == 0 && cancelTracker.Continue) DoRefreshMapView();
                    counter++;
                }
                if (DoRefreshMapView != null && cancelTracker.Continue) DoRefreshMapView();
            }
            this.dpi = dpi;
        }
        */
        public override IScreen Screen
        {
            get
            {
                return _screen;
            }
        }

        private class PrinterScreen : IScreen
        {
            #region IScreen Member

            public float LargeFontsFactor
            {
                get { return 1f; }
            }

            #endregion
        }
    }

    internal class RenderRasterLayerThreadPrint
    {
        protected Map _map;
        private IRasterLayer _layer;
        private ICancelTracker _cancelTracker;
        private InterpolationMethod _interpolMethod=InterpolationMethod.Fast;
        private float _transparency = 0.0f;
        private System.Drawing.Color _transColor = System.Drawing.Color.Transparent;
        static private IRasterLayer _lastRasterLayer = null;

        public RenderRasterLayerThreadPrint(Map map, IRasterLayer layer, IRasterLayer rootLayer, ICancelTracker cancelTracker)
        {
            _map = map;
            _layer = layer;
            _cancelTracker = cancelTracker;
            if (rootLayer != null)
            {
                _interpolMethod = rootLayer.InterpolationMethod;
                _transparency = rootLayer.Transparency;
            }
        }

        public void Render()
        {
            try
            {
                if (_layer == null || _map == null || _cancelTracker == null) return;
                if (_layer.RasterClass.Polygon == null) return;

                IEnvelope env = _layer.RasterClass.Polygon.Envelope;
                double minx = env.minx, miny = env.miny, maxx = env.maxx, maxy = env.maxy;
                _map.World2Image(ref minx, ref miny);
                _map.World2Image(ref maxx, ref maxy);
                int iWidth = 0, iHeight = 0;
                int min_x = Math.Max(0, (int)Math.Min(minx, maxx) - 1);
                int min_y = Math.Max(0, (int)Math.Min(miny, maxy) - 1);
                int max_x = Math.Min(iWidth = _map.iWidth, (int)Math.Max(minx, maxx) + 1);
                int max_y = Math.Min(iHeight = _map.iHeight, (int)Math.Max(miny, maxy) + 1);

                // _lastRasterLayer bei ImageServer vermeiden,
                // weil sonst das Bild gelöscht wird bevor es gezeichnet wurde
                // nicht Threadsicher!!!!!!!
                /*
                if (_lastRasterLayer != null && _lastRasterLayer != _layer)
                {
                    _lastRasterLayer.EndPaint();
                }
                */

                _layer.RasterClass.BeginPaint(_map.Display, _cancelTracker);
                if (_layer.RasterClass.Bitmap == null) return;

                //System.Windows.Forms.MessageBox.Show("begin");

                double W = (_map.Envelope.maxx - _map.Envelope.minx);
                double H = (_map.Envelope.maxy - _map.Envelope.miny);
                double MinX = _map.Envelope.minx;
                double MinY = _map.Envelope.miny;

                //_lastRasterLayer = _layer;

                System.Drawing.Graphics gr = _map.Display.GraphicsContext;
                if (gr == null) return;
                gr.InterpolationMode = (System.Drawing.Drawing2D.InterpolationMode)_interpolMethod;

                // Transformation berechnen
                System.Drawing.RectangleF rect;
                switch (gr.InterpolationMode)
                {
                    case System.Drawing.Drawing2D.InterpolationMode.Bilinear:
                    case System.Drawing.Drawing2D.InterpolationMode.Bicubic:
                        rect = new System.Drawing.RectangleF(0, 0, _layer.RasterClass.Bitmap.Width - 1f, _layer.RasterClass.Bitmap.Height - 1f);
                        break;
                    case System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor:
                        rect = new System.Drawing.RectangleF(-0.5f, -0.5f, _layer.RasterClass.Bitmap.Width, _layer.RasterClass.Bitmap.Height);
                        break;
                    default:
                        rect = new System.Drawing.RectangleF(0, 0, _layer.RasterClass.Bitmap.Width, _layer.RasterClass.Bitmap.Height);
                        break;
                }
                System.Drawing.PointF[] points = new System.Drawing.PointF[3];

                if (_layer.RasterClass is IRasterClass2)
                {
                    IPoint p1 = ((IRasterClass2)_layer.RasterClass).PicPoint1;
                    IPoint p2 = ((IRasterClass2)_layer.RasterClass).PicPoint2;
                    IPoint p3 = ((IRasterClass2)_layer.RasterClass).PicPoint3;
                    if (_map.Display.GeometricTransformer != null)
                    {
                        p1 = (IPoint)_map.Display.GeometricTransformer.Transform2D(p1);
                        p2 = (IPoint)_map.Display.GeometricTransformer.Transform2D(p2);
                        p3 = (IPoint)_map.Display.GeometricTransformer.Transform2D(p3);
                    }

                    double X = p1.X, Y = p1.Y;
                    _map.Display.World2Image(ref X, ref Y);
                    points[0] = new System.Drawing.PointF((float)X, (float)Y);

                    X = p2.X; Y = p2.Y;
                    _map.Display.World2Image(ref X, ref Y);
                    points[1] = new System.Drawing.PointF((float)X, (float)Y);

                    X = p3.X; Y = p3.Y;
                    _map.Display.World2Image(ref X, ref Y);
                    points[2] = new System.Drawing.PointF((float)X, (float)Y);
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
                    points[0] = new System.Drawing.PointF((float)X, (float)Y);
                    X = X1 + (_layer.RasterClass.Bitmap.Width) * _layer.RasterClass.dx1;
                    Y = Y1 + (_layer.RasterClass.Bitmap.Width) * _layer.RasterClass.dx2;
                    if (_map.Display.GeometricTransformer != null)
                    {
                        IPoint p = (IPoint)_map.Display.GeometricTransformer.Transform2D(new Point(X, Y));
                        X = p.X; Y = p.Y;
                    }
                    _map.Display.World2Image(ref X, ref Y);
                    points[1] = new System.Drawing.PointF((float)X, (float)Y);
                    X = X1 + (_layer.RasterClass.Bitmap.Height) * _layer.RasterClass.dy1;
                    Y = Y1 + (_layer.RasterClass.Bitmap.Height) * _layer.RasterClass.dy2;
                    if (_map.Display.GeometricTransformer != null)
                    {
                        IPoint p = (IPoint)_map.Display.GeometricTransformer.Transform2D(new Point(X, Y));
                        X = p.X; Y = p.Y;
                    }
                    _map.Display.World2Image(ref X, ref Y);
                    points[2] = new System.Drawing.PointF((float)X, (float)Y);
                }

                if (_transColor != System.Drawing.Color.Transparent)
                {
                    try
                    {
                        // kann OutOfMemoryException auslösen...
                        _layer.RasterClass.Bitmap.MakeTransparent(_transColor);
                    }
                    catch { }
                }

                float opaque = 1.0f - _transparency;
                if (opaque > 0f && opaque < 1f)
                {
                    float[][] ptsArray ={ 
                            new float[] {1, 0, 0, 0, 0},
                            new float[] {0, 1, 0, 0, 0},
                            new float[] {0, 0, 1, 0, 0},
                            new float[] {0, 0, 0, opaque, 0}, 
                            new float[] {0, 0, 0, 0, 1}};

                    System.Drawing.Imaging.ColorMatrix clrMatrix = new System.Drawing.Imaging.ColorMatrix(ptsArray);
                    System.Drawing.Imaging.ImageAttributes imgAttributes = new System.Drawing.Imaging.ImageAttributes();
                    imgAttributes.SetColorMatrix(clrMatrix,
                                                 System.Drawing.Imaging.ColorMatrixFlag.Default,
                                                 System.Drawing.Imaging.ColorAdjustType.Bitmap);

                    gr.DrawImage(_layer.RasterClass.Bitmap, points, rect, System.Drawing.GraphicsUnit.Pixel, imgAttributes);
                }
                else
                {
                    gr.DrawImage(_layer.RasterClass.Bitmap, points, rect, System.Drawing.GraphicsUnit.Pixel);
                }
                _layer.RasterClass.EndPaint(_cancelTracker);
            }
            catch (Exception ex)
            {
            }
        }
        public void Render3()
        {
            if (_layer == null || _layer.RasterClass==null || _map == null || _cancelTracker == null)  return;
            if (_layer.RasterClass.Polygon == null) return;

            IEnvelope env = _layer.RasterClass.Polygon.Envelope;
            double minx = env.minx, miny = env.miny, maxx = env.maxx, maxy = env.maxy;
            _map.World2Image(ref minx, ref miny);
            _map.World2Image(ref maxx, ref maxy);
            int iWidth = 0, iHeight = 0;
            int min_x = Math.Max(0, (int)Math.Min(minx, maxx) - 1);
            int min_y = Math.Max(0, (int)Math.Min(miny, maxy) - 1);
            int max_x = Math.Min(iWidth = _map.iWidth, (int)Math.Max(minx, maxx) + 1);
            int max_y = Math.Min(iHeight = _map.iHeight, (int)Math.Max(miny, maxy) + 1);

            
            _layer.RasterClass.BeginPaint(_map.Display,_cancelTracker);
            //System.Windows.Forms.MessageBox.Show("begin");

            double W = (_map.Envelope.maxx - _map.Envelope.minx);
            double H = (_map.Envelope.maxy - _map.Envelope.miny);
            double MinX = _map.Envelope.minx;
            double MinY = _map.Envelope.miny;

            System.Drawing.Graphics gr = _map.Display.GraphicsContext;
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            
            System.Drawing.RectangleF rect;
            switch (gr.InterpolationMode)
            {
                case System.Drawing.Drawing2D.InterpolationMode.Bilinear:
                case System.Drawing.Drawing2D.InterpolationMode.Bicubic:
                    rect = new System.Drawing.RectangleF(0, 0, _layer.RasterClass.Bitmap.Width - 1f, _layer.RasterClass.Bitmap.Height - 1f);
                    break;
                case System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor:
                    rect = new System.Drawing.RectangleF(-0.5f, -0.5f, _layer.RasterClass.Bitmap.Width, _layer.RasterClass.Bitmap.Height);
                    break;
                default:
                    rect = new System.Drawing.RectangleF(0, 0, _layer.RasterClass.Bitmap.Width, _layer.RasterClass.Bitmap.Height);
                    break;
            }

            System.Drawing.PointF[] points = new System.Drawing.PointF[3];

            double X1 = _layer.RasterClass.oX - _layer.RasterClass.dx1 / 2.0 - _layer.RasterClass.dy1 / 2.0;
            double Y1 = _layer.RasterClass.oY - _layer.RasterClass.dx2 / 2.0 - _layer.RasterClass.dy2 / 2.0;
            double X = X1;
            double Y = Y1;
            _map.Display.World2Image(ref X, ref Y);
            points[0] = new System.Drawing.PointF((float)X, (float)Y);
            X = X1 + (rect.Width) * _layer.RasterClass.dx1;
            Y = Y1 + (rect.Width) * _layer.RasterClass.dx2;
            _map.Display.World2Image(ref X, ref Y);
            points[1] = new System.Drawing.PointF((float)X, (float)Y);
            X = X1 + (rect.Height) * _layer.RasterClass.dy1;
            Y = Y1 + (rect.Height) * _layer.RasterClass.dy2;
            _map.Display.World2Image(ref X, ref Y);
            points[2] = new System.Drawing.PointF((float)X, (float)Y);

            //gr.ResetTransform();
            //gr.Transform = new System.Drawing.Drawing2D.Matrix(rect, points);
            _map.Display.GraphicsContext.DrawImage(
                _layer.RasterClass.Bitmap, points, rect, System.Drawing.GraphicsUnit.Pixel);

            /*
            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black))
            {
                gr.DrawRectangle(pen,rect.X,rect.Y,rect.Width,rect.Height);
            }
             * */
            //gr.ResetTransform();

            _layer.RasterClass.EndPaint(_cancelTracker);
        }

        public void Render2()
        {
            /*
            if (_layer == null || _layer.RasterClass == null || _map == null || _cancelTracker == null) return;
            if (_layer.RasterClass.Polygon == null) return;

            IEnvelope env = _layer.RasterClass.Polygon.Envelope;
            double minx = env.minx, miny = env.miny, maxx = env.maxx, maxy = env.maxy;
            _map.World2Image(ref minx, ref miny);
            _map.World2Image(ref maxx, ref maxy);
            int iWidth = 0, iHeight = 0;
            int min_x = Math.Max(0, (int)Math.Min(minx, maxx) - 1);
            int min_y = Math.Max(0, (int)Math.Min(miny, maxy) - 1);
            int max_x = Math.Min(iWidth = _map.iWidth, (int)Math.Max(minx, maxx) + 1);
            int max_y = Math.Min(iHeight = _map.iHeight, (int)Math.Max(miny, maxy) + 1);

            if (_lastRasterLayer != null && _lastRasterLayer != _layer)
            {
                _lastRasterLayer.RasterClass.EndPaint();
            }
            _layer.RasterClass.BeginPaint();
            //System.Windows.Forms.MessageBox.Show("begin");

            double W = (_map.Envelope.maxx - _map.Envelope.minx);
            double H = (_map.Envelope.maxy - _map.Envelope.miny);
            double MinX = _map.Envelope.minx;
            double MinY = _map.Envelope.miny;

            float dpiX = (float)96.0 / (float)Math.Min(_map.GraphicsContext.DpiX, 300);
            float dpiY = (float)96.0 / (float)Math.Min(_map.GraphicsContext.DpiY, 300);
            System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            System.Drawing.RectangleF pixel = new System.Drawing.RectangleF(0, 0, dpiX, dpiY);
            System.Drawing.RectangleF bounds = _map.GraphicsContext.ClipBounds;

            //dpiX = dpiY = 1;
            IntPtr hdc = (IntPtr)0;

            if (dpiX == 1 || dpiY == 1)
            {
                hdc = _map.GraphicsContext.GetHdc();
            }


            float aX = 0, aY = 0;
            if (bounds.X > 0) aX = bounds.X;
            if (bounds.Y > 0) aY = bounds.Y;

            int Y = min_y + (int)aX;
            for (float y = min_y; y <= max_y; y += dpiY, Y++)
            {
                int X = min_x + (int)aY;
                for (float x = min_x; x <= max_x; x += dpiX, X++)
                {
                    double xx = x * W / iWidth + MinX;
                    double yy = (iHeight - y) * H / iHeight + MinY;

                    System.Drawing.Color pixCol = _layer.RasterClass.GetPixel(xx, yy);
                    if (pixCol.A == 0) continue;
                    long col = pixCol.R + (pixCol.G << 8) + (pixCol.B << 16);

                    if (hdc == (IntPtr)0)
                    {
                        brush.Color = pixCol;
                        pixel.X = x + aX;
                        pixel.Y = y + aY;
                        _map.GraphicsContext.FillRectangle(brush, pixel);
                    }
                    else
                    {
                        gView.GDI.SetPixel(hdc, X, Y, col);
                    }
                }
                if (!_cancelTracker.Continue) break;
            }
            if (hdc != (IntPtr)0) _map.GraphicsContext.ReleaseHdc();
            brush.Dispose();

            _lastRasterLayer = _layer;
            //_layer.EndPaint();
             * */
        }
    }
}

﻿using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.MapServer;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.DataSources.Raster.File
{
    public class MrSidFileClass : IRasterClass, IRasterFileBitmap, IRasterFile, IDisposable
    {
        private enum RasterType { sid, jp2, unknown }

        private IRasterDataset _dataset = null;
        private string _filename = "";
        private IPolygon _polygon = null;
        private ISpatialReference _sRef = null;
        private RasterType _type;
        private bool _isValid = false;

        public MrSidFileClass()
        {
        }

        public MrSidFileClass(IRasterDataset dataset, string filename)
            : this(dataset, filename, null)
        {
        }

        public MrSidFileClass(IRasterDataset dataset, string filename, IPolygon polygon)
        {
            _dataset = dataset;
            _filename = filename;

            FileInfo fi = new FileInfo(filename);

            switch (fi.Extension.ToLower())
            {
                case ".sid":
                    _type = RasterType.sid;
                    //_reader = MrSidWrapper.LoadMrSIDReader(filename, ref _geoCoord);
                    break;
                case ".jp2":
                    _type = RasterType.jp2;
                    //_reader = MrSidWrapper.LoadJP2Reader(filename, ref _geoCoord);
                    break;
                default:
                    _type = RasterType.unknown;
                    break;
            }

            _sRef = fi.FileSpatialReference();

            if (polygon == null)
            {
                if (!CalcPolygon())
                {
                    return;
                }
            }
            else
            {
                _polygon = polygon;
            }
            _isValid = true;
        }

        ~MrSidFileClass()
        {
            CleanUp();
        }

        //private IntPtr memBuffer;
        private IntPtr InitReader(out MrSidGeoCoord geoCoord)
        {
            IntPtr reader = IntPtr.Zero;
            geoCoord = new MrSidGeoCoord();

            switch (_type)
            {
                case RasterType.sid:
                    reader = MrSidWrapper.LoadMrSIDReader(_filename, ref geoCoord);
                    break;
                case RasterType.jp2:
                    reader = MrSidWrapper.LoadJP2Reader(_filename, ref geoCoord);
                    //FileInfo finfo = new FileInfo(_filename);

                    //unsafe
                    //{
                    //    byte[] buffer = new byte[finfo.Length];
                    //    StreamReader s = new StreamReader(_filename);
                    //    s.BaseStream.Read(buffer, 0, (int)finfo.Length);
                    //    s.Close();

                    //    IntPtr memBuffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(buffer.Length);
                    //    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, memBuffer, buffer.Length);
                    //    _reader = MrSidWrapper.LoadJP2MemReader(memBuffer, (int)finfo.Length, ref _geoCoord);

                    //}
                    break;
            }

            this.oX = geoCoord.X;
            this.oY = geoCoord.Y;
            this.dx1 = geoCoord.xRes;
            this.dx2 = geoCoord.xRot;
            this.dy1 = geoCoord.yRot;
            this.dy2 = geoCoord.yRes;

            string wf = _filename.Substring(0, _filename.Length - 4) + ((_type == RasterType.jp2) ? ".j2w" : ".sdw");
            FileInfo fi = new FileInfo(wf);
            if (fi.Exists)
            {
                //geoCoord.X -= geoCoord.xRes / 2.0 + geoCoord.xRot / 2.0;
                //geoCoord.Y -= geoCoord.yRes / 2.0 + geoCoord.yRot / 2.0;
            }

            return reader;
        }

        private void ReleaseReader(IntPtr reader)
        {
            try
            {
                if (reader != IntPtr.Zero)
                {
                    MrSidWrapper.FreeReader(reader);
                    reader = IntPtr.Zero;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CleanUp()
        {
            //ReleaseReader();
        }

        internal bool isValid
        {
            get { return _isValid; }
        }

        private bool CalcPolygon()
        {
            IntPtr reader = InitReader(out MrSidGeoCoord geoCoord);
            try
            {
                if (reader == IntPtr.Zero)
                {
                    return false;
                }

                TFWFile tfw = this.GeoCoord as TFWFile;
                if (tfw == null)
                {
                    return false;
                }

                int iWidth = geoCoord.iWidth;
                int iHeight = geoCoord.iHeight;

                _polygon = new Polygon();
                Ring ring = new Ring();
                gView.Framework.Geometry.Point p1 = new gView.Framework.Geometry.Point(
                    tfw.X - tfw.dx_X / 2.0 - tfw.dy_X / 2.0,
                    tfw.Y - tfw.dx_Y / 2.0 - tfw.dy_Y / 2.0);

                ring.AddPoint(p1);
                ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + tfw.dx_X * iWidth, p1.Y + tfw.dx_Y * iWidth));
                ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + tfw.dx_X * iWidth + tfw.dy_X * iHeight, p1.Y + tfw.dx_Y * iWidth + tfw.dy_Y * iHeight));
                ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + tfw.dy_X * iHeight, p1.Y + tfw.dy_Y * iHeight));
                _polygon.AddRing(ring);

                return true;
            }
            finally
            {
                ReleaseReader(reader);
            }
        }

        #region IRasterClass Member

        public IPolygon Polygon
        {
            get { return _polygon; }
        }

        public double oX { get; private set; }

        public double oY { get; private set; }

        public double dx1 { get; private set; }

        public double dx2 { get; private set; }

        public double dy1 { get; private set; }

        public double dy2 { get; private set; }

        public ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
            set
            {
                _sRef = value;
            }
        }

        async public Task<IRasterPaintContext> BeginPaint(IDisplay display, ICancelTracker cancelTracker)
        {
            IntPtr bufferData = IntPtr.Zero;
            GraphicsEngine.BitmapPixelData bitmapData = null;
            double mag = 1f; // mag immer als float, läuft stabiler!!!

            int x = 0;
            int y = 0;
            int iWidth = 0;
            int iHeight = 0;

            GraphicsEngine.Abstraction.IBitmap bitmap = null;
            IntPtr reader = IntPtr.Zero;

            try
            {
                reader = InitReader(out MrSidGeoCoord geoCoord);
                if (reader == IntPtr.Zero || !(_polygon is ITopologicalOperation))
                {
                    return null;
                }

                TFWFile tfw = this.GeoCoord as TFWFile;
                if (tfw == null)
                {
                    return null;
                }

                IEnvelope dispEnvelope = display.DisplayTransformation.RotatedBounds(); //display.Envelope;
                if (display.GeometricTransformer != null)
                {
                    dispEnvelope = ((IGeometry)display.GeometricTransformer.InvTransform2D(dispEnvelope)).Envelope;
                }

                IGeometry clipped;
                ((ITopologicalOperation)_polygon).Clip(dispEnvelope, out clipped);
                if (!(clipped is IPolygon))
                {
                    return null;
                }

                IPolygon cPolygon = (IPolygon)clipped;

                if (cPolygon.RingCount == 0 || cPolygon[0].Area == 0D)
                {
                    return null;
                }

                // geclipptes Polygon transformieren -> Bild
                vector2[] vecs = new vector2[cPolygon[0].PointCount];
                for (int i = 0; i < cPolygon[0].PointCount; i++)
                {
                    vecs[i] = new vector2(cPolygon[0][i].X, cPolygon[0][i].Y);
                }
                if (!tfw.ProjectInv(vecs))
                {
                    return null;
                }

                IEnvelope picEnv = vector2.IntegerEnvelope(vecs);
                picEnv.MinX = Math.Max(0, picEnv.MinX);
                picEnv.MinY = Math.Max(0, picEnv.MinY);
                picEnv.MaxX = Math.Min(picEnv.MaxX, geoCoord.iWidth);
                picEnv.MaxY = Math.Min(picEnv.MaxY, geoCoord.iHeight);

                // Ecken zurücktransformieren -> Welt
                vecs = new vector2[3];
                vecs[0] = new vector2(picEnv.MinX, picEnv.MinY);
                vecs[1] = new vector2(picEnv.MaxX, picEnv.MinY);
                vecs[2] = new vector2(picEnv.MinX, picEnv.MaxY);
                tfw.Project(vecs);
                var p1 = new gView.Framework.Geometry.Point(vecs[0].x, vecs[0].y);
                var p2 = new gView.Framework.Geometry.Point(vecs[1].x, vecs[1].y);
                var p3 = new gView.Framework.Geometry.Point(vecs[2].x, vecs[2].y);

                double pix = display.MapScale / (display.Dpi / 0.0254);  // [m]
                double c1 = Math.Sqrt(geoCoord.xRes * geoCoord.xRes + geoCoord.xRot * geoCoord.xRot);
                double c2 = Math.Sqrt(geoCoord.yRes * geoCoord.yRes + geoCoord.yRot * geoCoord.yRot);
                mag = Math.Round((Math.Min(c1, c2) / pix), 8);

                // Immer in auf float runden! Läuft stabiler!!!
                //mag = (float)mag; //1.03;
                if (mag > 1f)
                {
                    mag = 1f;
                }

                if (mag < geoCoord.MinMagnification)
                {
                    mag = (float)geoCoord.MinMagnification;
                }

                x = (int)(picEnv.MinX * mag);
                y = (int)(picEnv.MinY * mag);
                iWidth = (int)((picEnv.Width - 1) * mag);
                iHeight = (int)((picEnv.Height - 1) * mag);

                bufferData = MrSidWrapper.Read(reader, x, y, iWidth, iHeight, mag);
                if (bufferData == IntPtr.Zero)
                {
                    return null;
                }

                int totalWidth = MrSidWrapper.GetTotalCols(bufferData);
                int totalHeight = MrSidWrapper.GetTotalRows(bufferData);

                bitmap = GraphicsEngine.Current.Engine.CreateBitmap(totalWidth, totalHeight, GraphicsEngine.PixelFormat.Rgb24);
                bitmapData = bitmap.LockBitmapPixelData(GraphicsEngine.BitmapLockMode.WriteOnly, GraphicsEngine.PixelFormat.Rgb24);

                MrSidWrapper.ReadBandData(bufferData, bitmapData.Scan0, 3, (uint)bitmapData.Stride);

                return new RasterPaintContext2(bitmap)
                {
                    PicPoint1 = p1,
                    PicPoint2 = p2,
                    PicPoint3 = p3
                };
            }
            catch (Exception ex)
            {
                //string errMsg = ex.Message;

                if (display is IServiceMap && ((IServiceMap)display).MapServer != null)
                {
                    IMapServer mapServer = ((IServiceMap)display).MapServer;
                    await mapServer.LogAsync(
                    ((IServiceMap)display).Name,
                    "RenderRasterLayerThread", loggingMethod.error,
                        ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace + "\n" +
                        "filename=" + _filename + "\n" +
                        "x=" + x.ToString() + "\n" +
                        "y=" + y.ToString() + "\n" +
                        "iWidth=" + iWidth.ToString() + "\n" +
                        "iHeight=" + iHeight.ToString() + "\n" +
                        "mag=" + mag.ToString() + "\n");
                }
                else
                {
                    throw;
                }

                return null;
            }
            finally
            {
                if (bitmapData != null)
                {
                    bitmap.UnlockBitmapPixelData(bitmapData);
                }

                MrSidWrapper.ReleaseBandData(bufferData);
                ReleaseReader(reader);
            }
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get
            {
                try
                {
                    FileInfo fi = new FileInfo(_filename);
                    return fi.Name;
                    //return fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                }
                catch
                {
                    return "???";
                }
            }
        }

        public string Aliasname
        {
            get { return Name; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion

        #region IBitmap Member

        public GraphicsEngine.Abstraction.IBitmap LoadBitmap()
        {
            return null;
        }

        #endregion

        #region IRasterFile Member

        public string Filename
        {
            get { return _filename; }
        }

        public IRasterWorldFile WorldFile
        {
            get
            {

                TFWFile tfw = new TFWFile(
                    this.oX,
                    this.oY,
                    this.dx1,
                    this.dx2,
                    this.dy1,
                    this.dy2);

                string wf = _filename.Substring(0, _filename.Length - 4) + ((_type == RasterType.jp2) ? ".j2w" : ".sdw");
                FileInfo fi = new FileInfo(wf);
                if (fi.Exists)
                {
                    tfw.Filename = wf;
                }
                else if (this.oX != 0.0 && this.oY != 0.0 &&
                    Math.Abs(this.dx1) != 1.0 && Math.Abs(this.dy2) != 1.0)
                {
                    // valid
                }
                else
                {
                    tfw.isValid = false;
                }
                return tfw;

            }
        }

        #endregion

        public IRasterWorldFile GeoCoord
        {
            get
            {
                return new TFWFile(
                        this.oX,
                        this.oY,
                        this.dx1,
                        this.dx2,
                        this.dy1,
                        this.dy2);
            }
        }

        #region IDisposable Member

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            CleanUp();
        }

        #endregion
    }
}

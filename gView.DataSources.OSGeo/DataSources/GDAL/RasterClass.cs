using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using System.IO;
using System.Drawing;
using gView.Framework.Geometry;
using System.Drawing.Imaging;
using gView.Framework.IO;
using gView.Framework.LinAlg;
using gView.Framework.system;

namespace gView.DataSources.GDAL
{
    public class RasterClass : IRasterClass2, IRasterFile, IBitmap, IPointIdentify, IGridIdentify, IGridClass, IPersistable, IDisposable
    {
        internal enum RasterType { image = 0, grid = 1, wavelet = 2 }

        private string _filename, _title;
        private bool _valid = true;
        private int _iWidth = 0, _iHeight = 0;
        internal TFWFile _tfw;
        private IPolygon _polygon;
        private ISpatialReference _sRef = null;
        private IRasterDataset _dataset;
        private OSGeo.GDAL.Dataset _gDS = null;
        private Bitmap _bitmap = null;
        private RasterType _type = RasterType.image;
        private double _min = 0, _max = 0;
        private double _nodata = 0;
        private int _hasNoDataVal = 0;
        private Color _minColor = Color.Black;
        private Color _maxColor = Color.White;
        private double[] _hillShade = { 0.0, 1.0, 1.0 };
        private bool _useHillShade = true;
        private GridColorClass[] _colorClasses = null;

        public RasterClass()
        {
        }

        public RasterClass(IRasterDataset dataset, string filename)
            : this(dataset, filename, null)
        {
        }
        public RasterClass(IRasterDataset dataset, string filename, IPolygon polygon)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                _title = fi.Name;
                _filename = filename;
                _dataset = dataset;

                OSGeo.GDAL.Gdal.AllRegister();
                _gDS = OSGeo.GDAL.Gdal.Open(fi.FullName, 0);
                if (_gDS == null && _gDS.RasterCount == 0)
                {
                    _valid = false;
                    return;
                }

                _iWidth = _gDS.RasterXSize;
                _iHeight = _gDS.RasterYSize;

                switch (fi.Extension.ToLower())
                {
                    case ".adf":
                    case ".gsd":
                        _type = RasterType.grid;
                        break;
                    //case ".jp2":
                    //    _type = RasterType.wavelet;
                    //    break;
                }

                using (OSGeo.GDAL.Band band = _gDS.GetRasterBand(1))
                {
                    if (_gDS.RasterCount == 1)
                    {
                        if (band.DataType != OSGeo.GDAL.DataType.GDT_Byte)
                            _type = RasterType.grid;
                    }
                    band.GetMinimum(out _min, out _hasNoDataVal);
                    band.GetMaximum(out _max, out _hasNoDataVal);
                    band.GetNoDataValue(out _nodata, out _hasNoDataVal);
                }
                OSGeo.GDAL.Driver driver = _gDS.GetDriver();

                double[] tfw = new double[6];
                _gDS.GetGeoTransform(tfw);

                string tfwFilename = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length);
                switch (fi.Extension.ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                        tfwFilename += ".jgw";
                        break;
                    case ".jp2":
                        tfwFilename += ".j2w";
                        break;
                    case ".tif":
                    case ".tiff":
                        tfwFilename += ".tfw";
                        break;
                    case ".ecw":
                        tfwFilename += ".eww";
                        break;
                    default:
                        break;
                }

                FileInfo tfwInfo = new FileInfo(tfwFilename);

                _tfw = new TFWFile(tfw[0], tfw[3], tfw[1], tfw[2], tfw[4], tfw[5]);
                if (tfwInfo.Exists) _tfw.Filename = tfwFilename;

                if (_tfw.X == 0.0 && _tfw.Y == 0.0 &&
                    Math.Abs(_tfw.dx_X) == 1.0 && _tfw.dx_Y == 0.0 &&
                    Math.Abs(_tfw.dy_Y) == 1.0 && _tfw.dy_X == 0.0 && driver != null)
                {
                    if (tfwInfo.Exists)
                        _tfw = new TFWFile(tfwFilename);
                    else
                        _tfw.isValid = false;
                }
                else
                {
                    // Bei dem Driver schein es nicht Pixelmitte sein, oder ist das bei GDAL generell
                    //if (driver.ShortName.ToLower() == "jp2openjpeg")  
                    {
                        _tfw.X += (_tfw.dx_X / 2.0D + _tfw.dx_Y / 2.0D);
                        _tfw.Y += (_tfw.dy_X / 2.0D + _tfw.dy_Y / 2.0D);
                    }
                }


                FileInfo fiPrj = new FileInfo(fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".prj");
                if (fiPrj.Exists)
                {
                    StreamReader sr = new StreamReader(fiPrj.FullName);
                    string wkt = sr.ReadToEnd();
                    sr.Close();

                    _sRef = gView.Framework.Geometry.SpatialReference.FromWKT(wkt);
                }
                else
                {
                    fiPrj = new FileInfo(fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".wkt");
                    if (fiPrj.Exists)
                    {
                        StreamReader sr = new StreamReader(fiPrj.FullName);
                        string wkt = sr.ReadToEnd();
                        sr.Close();

                        _sRef = gView.Framework.Geometry.SpatialReference.FromWKT(wkt);
                    }
                }
                if (polygon != null)
                {
                    _polygon = polygon;
                }
                else
                {
                    calcPolygon();
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                _valid = false;
            }
        }

        public bool isValid { get { return _valid; } }
        internal RasterType Type
        {
            get { return _type; }
        }

        private void calcPolygon()
        {
            _polygon = new Polygon();
            Ring ring = new Ring();
            gView.Framework.Geometry.Point p1 = new gView.Framework.Geometry.Point(
                _tfw.X - _tfw.dx_X / 2.0 - _tfw.dy_X / 2.0,
                _tfw.Y - _tfw.dx_Y / 2.0 - _tfw.dy_Y / 2.0);

            ring.AddPoint(p1);
            ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + _tfw.dx_X * _iWidth, p1.Y + _tfw.dx_Y * _iWidth));
            ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + _tfw.dx_X * _iWidth + _tfw.dy_X * _iHeight, p1.Y + _tfw.dx_Y * _iWidth + _tfw.dy_Y * _iHeight));
            ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + _tfw.dy_X * _iHeight, p1.Y + _tfw.dy_Y * _iHeight));
            _polygon.AddRing(ring);
        }

        #region IRasterClass Member

        public gView.Framework.Geometry.IPolygon Polygon
        {
            get { return _polygon; }
        }

        public System.Drawing.Bitmap Bitmap
        {
            get { return _bitmap; }
        }

        public double oX { get { return _tfw.X; } }
        public double oY { get { return _tfw.Y; } }
        public double dx1 { get { return _tfw.dx_X; } }
        public double dx2 { get { return _tfw.dx_Y; } }
        public double dy1 { get { return _tfw.dy_X; } }
        public double dy2 { get { return _tfw.dy_Y; } }

        public gView.Framework.Geometry.ISpatialReference SpatialReference
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

        public void BeginPaint(gView.Framework.Carto.IDisplay display, ICancelTracker cancelTracker)
        {
            EndPaint(cancelTracker);
            try
            {
                if (!(_polygon is ITopologicalOperation)) return;

                TFWFile tfw = this.WorldFile as TFWFile;
                if (tfw == null) return;

                IEnvelope dispEnvelope = display.DisplayTransformation.TransformedBounds(display); //display.Envelope;
                if (display.GeometricTransformer != null)
                {
                    dispEnvelope = (IEnvelope)((IGeometry)display.GeometricTransformer.InvTransform2D(dispEnvelope)).Envelope;
                }

                IGeometry clipped;
                ((ITopologicalOperation)_polygon).Clip(dispEnvelope, out clipped);
                if (!(clipped is IPolygon)) return;

                IPolygon cPolygon = (IPolygon)clipped;

                // geclipptes Polygon transformieren -> Bild
                vector2[] vecs = new vector2[cPolygon[0].PointCount];
                for (int i = 0; i < cPolygon[0].PointCount; i++)
                {
                    vecs[i] = new vector2(cPolygon[0][i].X, cPolygon[0][i].Y);
                }
                if (!tfw.ProjectInv(vecs)) return;
                IEnvelope picEnv = vector2.IntegerEnvelope(vecs);
                picEnv.minx = Math.Max(0, picEnv.minx);
                picEnv.miny = Math.Max(0, picEnv.miny);
                picEnv.maxx = Math.Min(picEnv.maxx, _iWidth);
                picEnv.maxy = Math.Min(picEnv.maxy, _iHeight);

                // Ecken zurücktransformieren -> Welt
                vecs = new vector2[3];
                vecs[0] = new vector2(picEnv.minx, picEnv.miny);
                vecs[1] = new vector2(picEnv.maxx, picEnv.miny);
                vecs[2] = new vector2(picEnv.minx, picEnv.maxy);
                tfw.Project(vecs);
                _p1 = new gView.Framework.Geometry.Point(vecs[0].x, vecs[0].y);
                _p2 = new gView.Framework.Geometry.Point(vecs[1].x, vecs[1].y);
                _p3 = new gView.Framework.Geometry.Point(vecs[2].x, vecs[2].y);

                double pix = display.mapScale / (display.dpi / 0.0254);  // [m]
                double c1 = Math.Sqrt(_tfw.dx_X * _tfw.dx_X + _tfw.dx_Y * _tfw.dx_Y);
                double c2 = Math.Sqrt(_tfw.dy_Y * _tfw.dy_Y + _tfw.dy_X * _tfw.dy_X);
                double mag = Math.Min(c1, c2) / pix;

                if (mag > 1.0) mag = 1.0;

                int x = (int)(picEnv.minx);
                int y = (int)(picEnv.miny);
                int wWidth = (int)(picEnv.Width);
                int wHeight = (int)(picEnv.Height);

                //if (wWidth + x > _iWidth) wWidth = _iWidth - x;
                //if (wHeight + y > _iHeight) wHeight = _iHeight - y;

                int iWidth = (int)(wWidth * mag);
                int iHeight = (int)(wHeight * mag);

                switch (_type)
                {
                    case RasterType.image:
                        PaintImage(x, y, wWidth, wHeight, iWidth, iHeight, cancelTracker);
                        break;
                    case RasterType.wavelet:
                        PaintWavelet(x, y, wWidth, wHeight, iWidth, iHeight, cancelTracker);
                        break;
                    case RasterType.grid:
                        if (_renderRawGridValues)
                            PaintGrid(x, y, wWidth, wHeight, iWidth, iHeight);
                        else
                            PaintHillShade(x, y, wWidth, wHeight, iWidth, iHeight, mag, cancelTracker);
                        break;

                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                EndPaint(cancelTracker);
            }
            finally
            {

            }
        }

        public void EndPaint(ICancelTracker cancelTracker)
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }

        #endregion

        private void PaintImage(int x, int y, int wWidth, int wHeight, int iWidth, int iHeight, ICancelTracker cancelTracker)
        {
            if (CancelTracker.Canceled(cancelTracker) || _gDS == null) return;
            int pixelSpace = 3;
            _bitmap = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);
            BitmapData bitmapData = _bitmap.LockBits(new Rectangle(0, 0, iWidth, iHeight), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            try
            {
                int stride = bitmapData.Stride;
                IntPtr buf = bitmapData.Scan0;

                List<Color> colors = new List<Color>();
                for (int i = 1; i <= (_gDS.RasterCount > 3 ? 3 : _gDS.RasterCount); ++i)
                {
                    using (OSGeo.GDAL.Band band = _gDS.GetRasterBand(i))
                    {

                        int ch = 0;
                        switch ((ColorInterp)band.GetRasterColorInterpretation())
                        {
                            case ColorInterp.BlueBand:
                                ch = 0;
                                break;
                            case ColorInterp.GreenBand:
                                ch = 1;
                                break;
                            case ColorInterp.RedBand:
                                ch = 2;
                                break;
                            case ColorInterp.GrayIndex:
                                for (int iColor = 0; iColor < 256; iColor++)
                                {
                                    colors.Add(Color.FromArgb(255, iColor, iColor, iColor));
                                }
                                break;
                            case ColorInterp.PaletteIndex:
                                OSGeo.GDAL.ColorTable colTable = band.GetRasterColorTable();
                                if (colTable == null) break;
                                int colCount = colTable.GetCount();
                                for (int iColor = 0; iColor < colCount; iColor++)
                                {
                                    OSGeo.GDAL.ColorEntry colEntry = colTable.GetColorEntry(iColor);
                                    colors.Add(Color.FromArgb(
                                        colEntry.c4, colEntry.c1, colEntry.c2, colEntry.c3));
                                }

                                break;
                        }
                        band.ReadRaster(x, y, wWidth, wHeight,
                            new IntPtr(buf.ToInt32() + ch),
                            iWidth, iHeight, OSGeo.GDAL.DataType.GDT_Byte, pixelSpace, stride);

                        band.Dispose();
                    }
                }
                if (colors.Count > 0)
                {
                    unsafe
                    {
                        byte* ptr = (byte*)(bitmapData.Scan0);
                        for (int i = 0; i < bitmapData.Height; i++)
                        {
                            if (CancelTracker.Canceled(cancelTracker)) return;
                            for (int j = 0; j < bitmapData.Width; j++)
                            {
                                // write the logic implementation here
                                byte c = ptr[0];
                                Color col = colors[(int)c];
                                ptr[0] = col.B;
                                ptr[1] = col.G;
                                ptr[2] = col.R;
                                ptr += pixelSpace;
                            }
                            ptr += bitmapData.Stride - bitmapData.Width * pixelSpace;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            finally
            {
                if (_bitmap != null)
                    _bitmap.UnlockBits(bitmapData);
            }
        }

        private void PaintWavelet(int x, int y, int wWidth, int wHeight, int iWidth, int iHeight, ICancelTracker cancelTracker)
        {
            if (CancelTracker.Canceled(cancelTracker) || _gDS == null) return;
            int pixelSpace = 3;
            _bitmap = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);
            BitmapData bitmapData = _bitmap.LockBits(new Rectangle(0, 0, iWidth, iHeight), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            try
            {
                int stride = bitmapData.Stride;
                IntPtr buf = bitmapData.Scan0;

                for (int i = 1; i <= (_gDS.RasterCount > 3 ? 3 : _gDS.RasterCount); ++i)
                {
                    using (OSGeo.GDAL.Band band = _gDS.GetRasterBand(i))
                    {

                        int ch = 0;
                        switch ((ColorInterp)band.GetRasterColorInterpretation())
                        {
                            case ColorInterp.BlueBand:
                                ch = 0;
                                break;
                            case ColorInterp.GreenBand:
                                ch = 1;
                                break;
                            case ColorInterp.RedBand:
                                ch = 2;
                                break;
                        }
                        band.ReadRaster(x, y, wWidth, wHeight,
                                new IntPtr(buf.ToInt32() + ch),
                                iWidth, iHeight, OSGeo.GDAL.DataType.GDT_Byte, pixelSpace, stride);

                        band.Dispose();
                    }
                }
            }
            finally
            {
                if (_bitmap != null)
                    _bitmap.UnlockBits(bitmapData);
            }
        }

        private void PaintGrid(int x, int y, int wWidth, int wHeight, int iWidth, int iHeight)
        {
            if (_gDS == null) return;

            int pixelSpace = 4;
            _bitmap = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = _bitmap.LockBits(new Rectangle(0, 0, iWidth, iHeight), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            OSGeo.GDAL.Band band = null;

            try
            {
                int stride = bitmapData.Stride;
                IntPtr buf = bitmapData.Scan0;

                List<Color> colors = new List<Color>();
                using (band = _gDS.GetRasterBand(1))
                {
                    band.ReadRaster(x, y, wWidth, wHeight,
                        buf,
                        iWidth, iHeight, OSGeo.GDAL.DataType.GDT_CFloat32, pixelSpace, stride);

                    band.Dispose();
                }

                unsafe
                {
                    byte* ptr = (byte*)(bitmapData.Scan0);
                    float* v = (float*)(bitmapData.Scan0);

                    for (int i = 0; i < bitmapData.Height; i++)
                    {
                        for (int j = 0; j < bitmapData.Width; j++)
                        {
                            if (_renderRawGridValues)
                            {
                                byte* vb = (byte*)v;
                                ptr[0] = 111; // *vb; vb++;
                                ptr[1] = 122; // *vb; vb++;
                                ptr[2] = 133; // *vb; vb++;
                                ptr[3] = 144; // *vb;
                            }
                            else
                            {
                                if (_hasNoDataVal == 1 && *v == _nodata)
                                {
                                    ptr[0] = ptr[1] = ptr[2] = ptr[3] = 0;
                                }
                                else
                                {
                                    double c = (*v - _min) / (_max - _min);
                                    double a = _minColor.A + c * (_maxColor.A - _minColor.A);
                                    double r = _minColor.R + c * (_maxColor.R - _minColor.R);
                                    double g = _minColor.G + c * (_maxColor.G - _minColor.G);
                                    double b = _minColor.B + c * (_maxColor.B - _minColor.B);
                                    ptr[0] = (byte)b; ptr[1] = (byte)g; ptr[2] = (byte)r;
                                    ptr[3] = (byte)a; // alpha
                                }
                            }
                            ptr += pixelSpace;
                            v++;
                        }
                        ptr += bitmapData.Stride - bitmapData.Width * pixelSpace;
                        v = (float*)ptr;
                    }
                }
            }
            finally
            {
                if (_bitmap != null)
                    _bitmap.UnlockBits(bitmapData);
            }
        }

        private void PaintHillShade(int x, int y, int wWidth, int wHeight, int iWidth, int iHeight, double mag, ICancelTracker cancelTracker)
        {
            if (CancelTracker.Canceled(cancelTracker) || _gDS == null) return;

            int pixelSpace = 4;
            Bitmap bitmap = new Bitmap(iWidth, iHeight + 100, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, iWidth, iHeight + 100), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            OSGeo.GDAL.Band band = null;

            try
            {
                int stride = bitmapData.Stride;
                IntPtr buf = bitmapData.Scan0;

                List<Color> colors = new List<Color>();
                using (band = _gDS.GetRasterBand(1))
                {

                    band.ReadRaster(x, y, wWidth, wHeight,
                        buf,
                        iWidth, iHeight, OSGeo.GDAL.DataType.GDT_CFloat32, pixelSpace, stride);

                    band.Dispose();
                }

                double cx = _tfw.cellX / mag;
                double cy = _tfw.cellY / mag;

                Vector3d sun = new Vector3d(_hillShade);
                sun.Normalize();
                int rowStride = stride / pixelSpace;

                Color col = Color.White;
                unsafe
                {
                    byte* ptr = (byte*)(bitmapData.Scan0);
                    float* v = (float*)(bitmapData.Scan0);

                    for (int i = 0; i < iHeight; i++)
                    {
                        if (CancelTracker.Canceled(cancelTracker)) return;
                        for (int j = 0; j < iWidth; j++)
                        {
                            if ((_hasNoDataVal == 1 && *v == _nodata) ||
                                (_useIgnoreValue && *v == _ignoreValue))
                            {
                                ptr[0] = ptr[1] = ptr[2] = ptr[3] = 0;
                            }
                            else
                            {
                                double c = *v;

                                col = GridColorClass.FindColor(c, _colorClasses);
                                if (!_useHillShade)
                                {
                                    ptr[0] = (byte)col.B; ptr[1] = (byte)col.G; ptr[2] = (byte)col.R;
                                    ptr[3] = (byte)col.A; // alpha
                                }
                                else
                                {
                                    double c1 = (j < iWidth - 1) ? (*(v + 1)) : c;
                                    double c2 = (i < iHeight - 1) ? (*(v + rowStride)) : c;
                                    c1 = ((_hasNoDataVal != 0 && c1 == _nodata) ||
                                          (_useIgnoreValue && c1 == _ignoreValue)) ? c : c1;
                                    c2 = ((_hasNoDataVal != 0 && c2 == _nodata) ||
                                          (_useIgnoreValue && c2 == _ignoreValue)) ? c : c2;

                                    Vector3d v1 = new Vector3d(cx, 0.0, c1 - c); v1.Normalize();
                                    Vector3d v2 = new Vector3d(0.0, -cy, c2 - c); v2.Normalize();
                                    Vector3d vs = v2 % v1; vs.Normalize();
                                    double h = Math.Min(Math.Max(0.0, sun * vs), 1.0);
                                    //double h = Math.Abs(sun * vs);

                                    double a = col.A;
                                    double r = col.R * h;
                                    double g = col.G * h;
                                    double b = col.B * h;
                                    ptr[0] = (byte)b; ptr[1] = (byte)g; ptr[2] = (byte)r;
                                    ptr[3] = (byte)a; // alpha
                                }
                            }
                            ptr += pixelSpace;
                            v++;
                        }
                        ptr += bitmapData.Stride - (bitmapData.Width) * pixelSpace;
                        v = (float*)ptr;
                    }
                }
                if (bitmap != null)
                    bitmap.UnlockBits(bitmapData);

                _bitmap = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(_bitmap))
                {
                    gr.DrawImage(bitmap, 0, 0);
                }
            }
            catch { }
            finally
            {
                if (bitmap != null)
                    bitmap.Dispose();
            }
        }

        #region IClass Member

        public string Name
        {
            get { return _title; }
        }

        public string Aliasname
        {
            get { return _title; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion

        #region IRasterClass2 Member

        private IPoint _p1, _p2, _p3;
        public IPoint PicPoint1
        {
            get { return _p1; }
        }

        public IPoint PicPoint2
        {
            get { return _p2; }
        }

        public IPoint PicPoint3
        {
            get { return _p3; }
        }

        #endregion

        #region IRasterFile Member

        public string Filename
        {
            get { return _filename; }
        }

        public IRasterWorldFile WorldFile
        {
            get { return _tfw; }
        }

        #endregion

        #region IBitmap Member

        public Bitmap LoadBitmap()
        {
            return null;
        }

        #endregion

        #region IPointIdentify Member

        public ICursor PointQuery(gView.Framework.Carto.IDisplay display, IPoint point, ISpatialReference sRef, IUserData userdata)
        {
            TFWFile tfw = this.WorldFile as TFWFile;
            if (tfw == null) return null;

            if (this.SpatialReference != null && sRef != null &&
                !sRef.Equals(this.SpatialReference))
            {
                point = GeometricTransformer.Transform2D(point, sRef, this.SpatialReference) as IPoint;
            }
            if (point == null) return null;

            // Punkt transformieren -> Bild
            vector2[] vecs = new vector2[1];
            vecs[0] = new vector2(point.X, point.Y);

            if (!tfw.ProjectInv(vecs)) return null;
            if (vecs[0].x < 0 || vecs[0].x >= _iWidth ||
                vecs[0].y < 0 || vecs[0].y >= _iHeight) return null;

            switch (_type)
            {
                case RasterType.image:
                    return QueryImage((int)Math.Floor(vecs[0].x), (int)Math.Floor(vecs[0].y));
                case RasterType.grid:
                    return QueryGrid((int)Math.Floor(vecs[0].x), (int)Math.Floor(vecs[0].y));
            }

            return null;
        }

        #endregion

        #region IGridIdentify Member

        private OSGeo.GDAL.Band _gridQueryBand = null;
        public void InitGridQuery()
        {
            if (_gridQueryBand == null)
                _gridQueryBand = _gDS.GetRasterBand(1);
        }

        public float GridQuery(gView.Framework.Carto.IDisplay display, IPoint point, ISpatialReference sRef)
        {
            TFWFile tfw = this.WorldFile as TFWFile;
            if (tfw == null) return (float)_nodata;

            if (this.SpatialReference != null && sRef != null &&
                !sRef.Equals(this.SpatialReference))
            {
                point = GeometricTransformer.Transform2D(point, sRef, this.SpatialReference) as IPoint;
            }
            if (point == null) return (float)_nodata;

            // Punkt transformieren -> Bild
            vector2[] vecs = new vector2[1];
            vecs[0] = new vector2(point.X, point.Y);

            if (!tfw.ProjectInv(vecs)) return (float)_nodata;
            if (vecs[0].x < 0 || vecs[0].x >= _iWidth ||
                vecs[0].y < 0 || vecs[0].y >= _iHeight) return (float)_nodata;

            unsafe
            {
                fixed (float* buf = new float[2])
                {
                    _gridQueryBand.ReadRaster((int)vecs[0].x, (int)vecs[0].y, 1, 1,
                        (IntPtr)buf,
                        1, 1, OSGeo.GDAL.DataType.GDT_CFloat32, 4, 0);

                    if ((_hasNoDataVal != 0 && buf[0] == _nodata) ||
                        (_useIgnoreValue && buf[0] == _ignoreValue))
                        return (float)_nodata;

                    return buf[0];
                }
            }
        }

        public void ReleaseGridQuery()
        {
            if (_gridQueryBand != null)
            {
                _gridQueryBand.Dispose();
                _gridQueryBand = null;
            }
        }

        #endregion

        private ICursor QueryImage(int x, int y)
        {
            unsafe
            {
                int bandCount = _gDS.RasterCount;
                string[] tags = new string[bandCount + 2];
                object[] values = new object[bandCount + 2];
                List<Color> colors = new List<Color>();

                for (int i = 1; i <= bandCount; ++i)
                {
                    OSGeo.GDAL.Band band = _gDS.GetRasterBand(i);

                    string bandName = "";
                    switch ((ColorInterp)band.GetRasterColorInterpretation())
                    {
                        case ColorInterp.BlueBand:
                            bandName = "(blue)";
                            break;
                        case ColorInterp.GreenBand:
                            bandName = "(green)";
                            break;
                        case ColorInterp.RedBand:
                            bandName = "(red)";
                            break;
                        case ColorInterp.GrayIndex:
                            for (int iColor = 0; iColor < 256; iColor++)
                            {
                                colors.Add(Color.FromArgb(255, iColor, iColor, iColor));
                            }
                            break;
                        case ColorInterp.PaletteIndex:
                            tags = new string[tags.Length + 4];
                            values = new object[values.Length + 4];

                            OSGeo.GDAL.ColorTable colTable = band.GetRasterColorTable();
                            if (colTable == null) break;
                            int colCount = colTable.GetCount();
                            for (int iColor = 0; iColor < colCount; iColor++)
                            {
                                OSGeo.GDAL.ColorEntry colEntry = colTable.GetColorEntry(iColor);
                                colors.Add(Color.FromArgb(
                                    colEntry.c4, colEntry.c1, colEntry.c2, colEntry.c3));
                            }

                            break;
                    }

                    int c = 0;

                    int* buf = &c;

                    band.ReadRaster(x, y, 1, 1,
                        (IntPtr)buf,
                        1, 1, OSGeo.GDAL.DataType.GDT_Int32, 4, 0);

                    band.Dispose();

                    tags[i + 1] = "Band " + i.ToString() + " " + bandName;
                    values[i + 1] = c;

                    if (colors.Count > 0 && c >= 0 && c < colors.Count)
                    {
                        Color col = colors[c];
                        tags[i + 2] = "Alpha";
                        values[i + 2] = col.A;
                        tags[i + 3] = "Red";
                        values[i + 3] = col.R;
                        tags[i + 4] = "Green";
                        values[i + 4] = col.G;
                        tags[i + 5] = "Blue";
                        values[i + 5] = col.B;
                    }
                }

                tags[0] = "ImageX"; values[0] = x;
                tags[1] = "ImageY"; values[1] = y;

                return new QueryCursor(tags, values);
            }
        }

        private ICursor QueryGrid(int x, int y)
        {
            try
            {
                unsafe
                {
                    fixed (float* buf = new float[2])
                    {
                        OSGeo.GDAL.Band band = _gDS.GetRasterBand(1);

                        band.ReadRaster(x, y, 1, 1,
                            (IntPtr)buf,
                            1, 1, OSGeo.GDAL.DataType.GDT_CFloat32, 4, 0);

                        if ((_hasNoDataVal != 0 && buf[0] == _nodata) ||
                            (_useIgnoreValue && buf[0] == _ignoreValue))
                            return null;

                        string[] tags = { "ImageX", "ImageY", "band1" };
                        object[] values = { x, y, buf[0] };

                        band.Dispose();

                        return new QueryCursor(tags, values);
                    }
                }
            }
            finally
            {
            }
        }

        private class QueryCursor : IRowCursor
        {
            private int _pos = 0;
            private string[] _tag;
            private object[] _values;

            public QueryCursor(string[] tag, object[] values)
            {
                _tag = tag;
                _values = values;
            }

            #region IDisposable Member

            public void Dispose()
            {

            }

            #endregion

            #region IRowCursor Member

            public IRow NextRow
            {
                get
                {
                    if (_pos > 0) return null;

                    Row row = new Row();
                    row.OID = 1;

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < _tag.Length; i++)
                    {
                        if (_tag[i] == "ImageX" || _tag[i] == "ImageY") continue;
                        if (_values[_pos] == null) continue;
                        if (sb.Length > 0) sb.Append(",");
                        sb.Append(_values[i].ToString());
                    }
                    row.Fields.Add(new FieldValue("Bands", "(" + sb.ToString() + ")"));

                    for (int i = 0; i < _tag.Length; i++)
                    {
                        if (_tag[_pos] == null) continue;
                        row.Fields.Add(new FieldValue(_tag[i], _values[i]));
                    }
                    _pos++;

                    return row;
                }
            }

            #endregion
        }

        #region IGridClass Member
        private double _ignoreValue = 0.0;
        private bool _useIgnoreValue = false;

        public GridRenderMethode ImplementsRenderMethods
        {
            get
            {
                if (_type == RasterType.image)
                    return GridRenderMethode.None;
                else
                    return GridRenderMethode.Colors | GridRenderMethode.HillShade | GridRenderMethode.NullValue;
            }
        }

        public GridColorClass[] ColorClasses
        {
            get
            {
                if (_colorClasses == null) return null;
                GridColorClass[] copy = new GridColorClass[_colorClasses.Length];
                _colorClasses.CopyTo(copy, 0);

                return copy;
            }
            set
            {
                if (value == null)
                {
                    _colorClasses = null;
                }
                else
                {
                    _colorClasses = new GridColorClass[value.Length];
                    value.CopyTo(_colorClasses, 0);
                }
            }
        }

        public bool UseHillShade
        {
            get { return _useHillShade; }
            set { _useHillShade = value; }
        }
        public double[] HillShadeVector
        {
            get
            {
                return new Vector3d(_hillShade).ToArray();
            }
            set
            {
                if (value == null || value.Length != 3) return;

                _hillShade[0] = value[0];
                _hillShade[1] = value[1];
                _hillShade[2] = value[2];
            }
        }

        public double MinValue
        {
            get { return _min; }
        }
        public double MaxValue
        {
            get { return _max; }
        }

        public double IgnoreDataValue
        {
            get
            {
                return _ignoreValue;
            }
            set
            {
                _ignoreValue = value;
            }
        }
        public bool UseIgnoreDataValue
        {
            get
            {
                return _useIgnoreValue;
            }
            set
            {
                _useIgnoreValue = value;
            }
        }

        bool _renderRawGridValues = false;
        public bool RenderRawGridValues
        {
            get { return _renderRawGridValues; }
            set { _renderRawGridValues = value; }
        }
        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_type == RasterType.grid)
            {
                _colorClasses = null;
                List<GridColorClass> classes = new List<GridColorClass>();
                GridColorClass cc;
                while ((cc = (GridColorClass)stream.Load("GridClass", null, new GridColorClass(0, 0, Color.White))) != null)
                {
                    classes.Add(cc);
                }
                if (classes.Count > 0)
                    _colorClasses = classes.ToArray();

                _useHillShade = (bool)stream.Load("UseHillShade", true);
                _hillShade[0] = (double)stream.Load("HillShadeDx", 0.0);
                _hillShade[1] = (double)stream.Load("HillShadeDy", 0.0);
                _hillShade[2] = (double)stream.Load("HillShadeDz", 0.0);
                _useIgnoreValue = (bool)stream.Load("UseIgnoreData", 0);
                _ignoreValue = (double)stream.Load("IgnoreData", 0.0);
                _renderRawGridValues = (bool)stream.Load("RenderRawGridValues", false);
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_type == RasterType.grid)
            {
                if (_colorClasses != null)
                {
                    foreach (GridColorClass cc in _colorClasses)
                    {
                        stream.Save("GridClass", cc);
                    }
                }

                stream.Save("UseHillShade", _useHillShade);
                stream.Save("HillShadeDx", _hillShade[0]);
                stream.Save("HillShadeDy", _hillShade[1]);
                stream.Save("HillShadeDz", _hillShade[2]);
                stream.Save("UseIgnoreData", _useIgnoreValue);
                stream.Save("IgnoreData", _ignoreValue);
                stream.Save("RenderRawGridValues", _renderRawGridValues);
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (_gDS != null)
            {
                _gDS.Dispose();
                _gDS = null;
            }
        }

        #endregion
    }

    public enum ColorInterp
    {
        Undefined = 0,
        GrayIndex = 1,
        PaletteIndex = 2,
        RedBand = 3,
        GreenBand = 4,
        BlueBand = 5,
        AlphaBand = 6,
        HueBand = 7,
        SaturationBand = 8,
        LightnessBand = 9,
        CyanBand = 10,
        MagentaBand = 11,
        YellowBand = 12,
        BlackBand = 13,
        YCbCr_YBand = 14,
        YCbCr_CbBand = 15,
        YCbCr_CrBand = 16,
        Max = 16
    }

    public enum GDALDataType
    {
        /*! Eight bit unsigned integer */
        GDT_Byte = 1,
        /*! Sixteen bit unsigned integer */
        GDT_UInt16 = 2,
        /*! Sixteen bit signed integer */
        GDT_Int16 = 3,
        /*! Thirty two bit unsigned integer */
        GDT_UInt32 = 4,
        /*! Thirty two bit signed integer */
        GDT_Int32 = 5,
        /*! Thirty two bit floating point */
        GDT_Float32 = 6,
        /*! Sixty four bit floating point */
        GDT_Float64 = 7,
        /*! Complex Int16 */
        GDT_CInt16 = 8,
        /*! Complex Int32 */
        GDT_CInt32 = 9,
        /*! Complex Float32 */
        GDT_CFloat32 = 10,
        /*! Complex Float64 */
        GDT_CFloat64 = 11
    }
}

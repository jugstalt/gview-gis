using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using System.IO;
using System.Threading.Tasks;

namespace gView.DataSources.Raster.File
{
    public class RasterFileClass : IRasterClass, IRasterFileBitmap, IRasterFile
    {
        private string _filename, _title;
        private bool _valid = true;
        private int _iWidth = 0, _iHeight = 0;
        internal TFWFile _tfw;
        private IPolygon _polygon;
        private ISpatialReference _sRef = null;
        private IRasterDataset _dataset;

        public RasterFileClass()
        {
        }

        public RasterFileClass(IRasterDataset dataset, string filename)
            : this(dataset, filename, null)
        {
        }
        public RasterFileClass(IRasterDataset dataset, string filename, IPolygon polygon)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                _title = fi.Name;
                _filename = filename;
                _dataset = dataset;

                string tfwfilename = filename.Substring(0, filename.Length - fi.Extension.Length);
                if (fi.Extension.ToLower() == ".jpg")
                {
                    tfwfilename += ".jgw";
                }
                else
                {
                    tfwfilename += ".tfw";
                }

                _tfw = new TFWFile(tfwfilename);
                //if (!_tfw.isValid)
                //{
                //    _valid = false;
                //    return;
                //}

                FileInfo fiPrj = new FileInfo(fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".prj");
                if (fiPrj.Exists)
                {
                    StreamReader sr = new StreamReader(fiPrj.FullName);
                    string wkt = sr.ReadToEnd();
                    sr.Close();

                    _sRef = gView.Framework.Geometry.SpatialReference.FromWKT(wkt);
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
            catch { _valid = false; }
        }

        public bool isValid { get { return _valid; } }

        private void calcPolygon()
        {
            try
            {
                if (_tfw == null)
                {
                    FileInfo fi = new FileInfo(_filename);

                    string tfwfilename = _filename.Substring(0, _filename.Length - fi.Extension.Length);
                    if (fi.Extension.ToLower() == ".jpg")
                    {
                        tfwfilename += ".jgw";
                    }
                    else
                    {
                        tfwfilename += ".tfw";
                    }

                    _tfw = new TFWFile(tfwfilename);
                }
                if (_iWidth == 0 || _iHeight == 0)
                {
                    using (var image = GraphicsEngine.Current.Engine.CreateBitmap(_filename))
                    {
                        SetBounds(image);
                    }
                }
            }
            catch
            {
                _polygon = null;
            }
        }
        private void SetBounds(GraphicsEngine.Abstraction.IBitmap bitmap)
        {
            if (bitmap != null)
            {
                _iWidth = bitmap.Width;
                _iHeight = bitmap.Height;
            }
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

        #region IRasterClass
        public IPolygon Polygon
        {
            get { return _polygon; }
        }

        GraphicsEngine.Abstraction.IBitmap _bitmap;
        GraphicsEngine.BitmapPixelData _bmData;

        async public Task BeginPaint(gView.Framework.Carto.IDisplay display, ICancelTracker cancelTracker)
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }

            _bitmap = GraphicsEngine.Current.Engine.CreateBitmap(_filename);

            /*
            _bmData = _bm.LockBits(new Rectangle(0, 0, _bm.Width, _bm.Height),
                 ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            */
        }
        public void EndPaint(ICancelTracker cancelTracker)
        {
            //return;
            if (_bitmap != null)
            {
                if (_bmData != null)
                {
                    /*
                    _bm.UnlockBits(_bmData);
                    _bmData = null;
                     */
                }
                _bitmap.Dispose();
                _bitmap = null;
            }
        }
        public GraphicsEngine.Abstraction.IBitmap Bitmap
        {
            get { return _bitmap; }
        }

        public GraphicsEngine.ArgbColor GetPixel(double X, double Y)
        {
            //return Color.Beige;
            if (_bitmap == null)
            {
                return GraphicsEngine.ArgbColor.Transparent;
            }

            int x, y;
            _tfw.World2Image(X, Y, out x, out y);
            if (x < 0 || y < 0 || x >= _iWidth || y >= _iHeight)
            {
                return GraphicsEngine.ArgbColor.Transparent;
            }

            if (_bmData != null)
            {
                unsafe
                {
                    int* p = (int*)(void*)_bmData.Scan0;
                    p += y * _iWidth + x;
                    return GraphicsEngine.ArgbColor.FromArgb(*p);
                }
            }
            else
            {
                return _bitmap.GetPixel(x, y);
            }
        }

        public double oX { get { return _tfw.X; } }
        public double oY { get { return _tfw.Y; } }
        public double dx1 { get { return _tfw.dx_X; } }
        public double dx2 { get { return _tfw.dx_Y; } }
        public double dy1 { get { return _tfw.dy_X; } }
        public double dy2 { get { return _tfw.dy_Y; } }

        public ISpatialReference SpatialReference
        {
            get { return _sRef; }
            set { _sRef = value; }
        }

        public IRasterDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion

        #region IBitmap Members

        public GraphicsEngine.Abstraction.IBitmap LoadBitmap()
        {
            try
            {
                if (_filename == "" || _filename == null)
                {
                    return null;
                }

                return GraphicsEngine.Current.Engine.CreateBitmap(_filename);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region IRasterFile Members

        public string Filename
        {
            get { return _filename; }
        }

        public IRasterWorldFile WorldFile
        {
            get { return _tfw; }
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get { return _title; }
        }

        public string Aliasname
        {
            get { return _title; }
        }

        IDataset IClass.Dataset
        {
            get { return _dataset; }
        }

        #endregion
    }
}

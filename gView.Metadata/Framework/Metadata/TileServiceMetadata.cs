using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using System.Reflection;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Tiling;

namespace gView.Framework.Metadata
{
    [RegisterPlugIn("D33D3DD2-DD63-4a47-9F84-F840FE0D01C0")]
    public class TileServiceMetadata : IMetadataProvider, IPropertyPage
    {
        private bool _use = false;
        private DoubleScales _scales = new DoubleScales();
        private List<int> _epsgs = new List<int>();
        private Dictionary<int, IEnvelope> _extents = new Dictionary<int, IEnvelope>();
        private Dictionary<int, IPoint> _originsUL = new Dictionary<int, IPoint>();
        private Dictionary<int, IPoint> _originsLL = new Dictionary<int, IPoint>();
        private int _tileWidth = 256, _tileHeight = 256;
        private bool _upperLeft = true, _lowerLeft = true;
        private bool _upperLeftCacheTiles = true;
        private bool _lowerLeftCacheTiles = true;
        private bool _formatPng = true;
        private bool _formatJpg = true;
        private double _dpi = Const.TilesDpi;

        #region Properties
        public bool Use
        {
            get { return _use; }
            set { _use = value; }
        }

        public List<int> EPSGCodes
        {
            get { return _epsgs; }
        }
        public IEnvelope GetEPSGEnvelope(int epsg)
        {
            if (!_extents.ContainsKey(epsg))
                return null;

            return _extents[epsg];
        }
        public void SetEPSGEnvelope(int epsg, IEnvelope envelope)
        {
            if (envelope == null)
                envelope = new Envelope();

            if (_extents.ContainsKey(epsg))
                _extents[epsg] = envelope;
            else
                _extents.Add(epsg, envelope);
        }

        public IPoint GetOriginUpperLeft(int epsg)
        {
            if (_originsUL.ContainsKey(epsg))
                return _originsUL[epsg];

            var envelope = GetEPSGEnvelope(epsg);
            if (envelope != null)
                return new Point(envelope.minx, envelope.maxy);

            return null;
        }
        public IPoint GetOriginLowerLeft(int epsg)
        {
            if (_originsLL.ContainsKey(epsg))
                return _originsLL[epsg];

            var envelope = GetEPSGEnvelope(epsg);
            if (envelope != null)
                return new Point(envelope.minx, envelope.miny);

            return null;
        }
        public void SetOriginUpperLeft(int epsg, IPoint upperLeft)
        {
            if (_originsUL.ContainsKey(epsg))
                _originsUL[epsg] = upperLeft;
            else
                _originsUL.Add(epsg, upperLeft);
        }
        public void SetOriginLowerLeft(int epsg, IPoint lowerLeft)
        {
            if (_originsLL.ContainsKey(epsg))
                _originsLL[epsg] = lowerLeft;
            else
                _originsLL.Add(epsg, lowerLeft);
        }

        public double Dpi
        {
            get
            {
                return _dpi;
            }
            set
            {
                _dpi = value;
            }
        }

        public DoubleScales Scales
        {
            get { return _scales; }
            set
            {
                if (value == null)
                {
                    _scales = new DoubleScales();
                }
                else
                {
                    _scales = value;
                    if (_scales != null)
                        _scales.Order();
                }

            }
        }
        public int TileWidth
        {
            get { return _tileWidth; }
            set { _tileWidth = value; }
        }
        public int TileHeight
        {
            get { return _tileHeight; }
            set { _tileHeight = value; }
        }

        public bool UpperLeft
        {
            get { return _upperLeft; }
            set { _upperLeft = value; }
        }
        public bool LowerLeft
        {
            get { return _lowerLeft; }
            set { _lowerLeft = value; }
        }

        public bool UpperLeftCacheTiles
        {
            get { return _upperLeftCacheTiles; }
            set { _upperLeftCacheTiles = value; }
        }
        public bool LowerLeftCacheTiles
        {
            get { return _lowerLeftCacheTiles; }
            set { _lowerLeftCacheTiles = value; }
        }

        public bool FormatPng
        {
            get { return _formatPng; }
            set { _formatPng = value; }
        }
        public bool FormatJpg
        {
            get { return _formatJpg; }
            set { _formatJpg = value; }
        }

        public bool RenderTilesOnTheFly { get; set; }

        #endregion

        #region IMetadataProvider Member

        public bool ApplyTo(object Object)
        {
            if (Object == null)
                return false;

            return Object.GetType().ToString() == "gView.MapServer.Instance.ServiceMap";
        }

        public string Name
        {
            get { return "Tile Service Properties"; }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _use = (bool)stream.Load("use", false);

            _tileWidth = (int)stream.Load("tile_width", 256);
            _tileHeight = (int)stream.Load("tile_height", 256);

            _upperLeft = (bool)stream.Load("upperleft", true);
            _lowerLeft = (bool)stream.Load("lowerleft", true);
            _upperLeftCacheTiles = (bool)stream.Load("upperleftcachetiles", true);
            _lowerLeftCacheTiles = (bool)stream.Load("lowerleftcachetiles", true);

            _formatPng = (bool)stream.Load("formatpng", true);
            _formatJpg = (bool)stream.Load("formatjpg", true);

            _dpi = (double)stream.Load("dpi", Const.TilesDpi);

            this.RenderTilesOnTheFly = (bool)stream.Load("rendertilesonthefly", false);

            _scales.Clear();
            int scales_count = (int)stream.Load("scales_count", 0);
            for (int i = 0; i < scales_count; i++)
            {
                double scale = Convert.ToDouble(stream.Load("scale" + i, 0.0));
                if (scale <= 0.0)
                    continue;
                _scales.Add(scale);
            }

            _extents.Clear();
            _epsgs.Clear();
            _originsUL.Clear();
            _originsLL.Clear();
            int epsg_count = (int)stream.Load("epsg_count", 0);
            for (int i = 0; i < epsg_count; i++)
            {
                int epsg = (int)stream.Load("epsg" + i, 0);
                if (epsg == 0 || _extents.ContainsKey(epsg))
                    continue;
                _epsgs.Add((int)stream.Load("epsg" + i, epsg));
                _extents.Add(epsg,
                             new Envelope(
                    (double)stream.Load("extent_minx" + i, 0.0),
                    (double)stream.Load("extent_miny" + i, 0.0),
                    (double)stream.Load("extent_maxx" + i, 0.0),
                    (double)stream.Load("extent_maxy" + i, 0.0)));

                _originsUL.Add(epsg, new Point(
                    (double)stream.Load("origin_ul_x" + i, GetOriginUpperLeft(epsg)?.X ?? 0D),
                    (double)stream.Load("origin_ul_y" + i, GetOriginUpperLeft(epsg)?.Y ?? 0D)
                    ));

                _originsLL.Add(epsg, new Point(
                    (double)stream.Load("origin_ll_x" + i, GetOriginLowerLeft(epsg)?.X ?? 0D),
                    (double)stream.Load("origin_ll_y" + i, GetOriginLowerLeft(epsg)?.Y ?? 0D)
                    ));
            }
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("use", _use);

            stream.Save("tile_width", _tileWidth);
            stream.Save("tile_height", _tileHeight);

            stream.Save("upperleft", _upperLeft);
            stream.Save("lowerleft", _lowerLeft);
            stream.Save("upperleftcachetiles", _upperLeftCacheTiles);
            stream.Save("lowerleftcachetiles", _lowerLeftCacheTiles);

            stream.Save("formatpng", _formatPng);
            stream.Save("formatjpg", _formatJpg);

            stream.Save("rendertilesonthefly", this.RenderTilesOnTheFly);

            stream.Save("dpi", _dpi);

            _scales.Order();
            stream.Save("scales_count", _scales.Count);
            int counter = 0;
            foreach (double scale in _scales)
            {
                if (scale <= 0.0)
                    continue;

                stream.Save("scale" + counter, scale);
                counter++;
            }

            stream.Save("epsg_count", _epsgs.Count);
            counter = 0;
            for (int i = 0; i < _epsgs.Count; i++)
            {
                int epsg = _epsgs[i];
                if (epsg <= 0)
                    continue;

                stream.Save("epsg" + counter, epsg);

                IEnvelope extent = _extents.ContainsKey(epsg) ? _extents[epsg] : new Envelope();
                stream.Save("extent_minx" + counter, extent.minx);
                stream.Save("extent_miny" + counter, extent.miny);
                stream.Save("extent_maxx" + counter, extent.maxx);
                stream.Save("extent_maxy" + counter, extent.maxy);

                IPoint ul = GetOriginUpperLeft(epsg)!=null ? GetOriginUpperLeft(epsg) : new Point();
                stream.Save("origin_ul_x" + counter, ul.X);
                stream.Save("origin_ul_y" + counter, ul.Y);

                IPoint ll = GetOriginLowerLeft(epsg) != null ? GetOriginLowerLeft(epsg) : new Point();
                stream.Save("origin_ll_x" + counter, ll.X);
                stream.Save("origin_ll_y" + counter, ll.Y);

                counter++;
            }
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Metadata.UI.dll");

            IPlugInParameter p = uiAssembly.CreateInstance("gView.Framework.Metadata.UI.TileServiceMetadataControl") as IPlugInParameter;
            if (p != null)
                p.Parameter = this;

            return p;
        }

        public object PropertyPageObject()
        {
            return this;
        }

        #endregion

        #region Static Members

        public static string TilePath(GridOrientation orientation, int epsg, double scale, int row, int col)
        {
            return
                (orientation == GridOrientation.UpperLeft ? "ul" : "ll") + @"\" +
                epsg + @"\" + (int)Math.Round(scale) + @"\" + row + @"\" + col;
        }

        public static string ScalePath(GridOrientation orientation, int epsg, double scale)
        {
            return
                (orientation == GridOrientation.UpperLeft ? "ul" : "ll") + @"\" +
                epsg + @"\" + (int)Math.Round(scale) + @"\";
        }

        public static string EpsgPath(GridOrientation orientation, int epsg)
        {
            return
                (orientation == GridOrientation.UpperLeft ? "ul" : "ll") + @"\" +
                epsg + @"\";
        }

        #endregion

        #region HelperClasses
        public class DoubleScales : List<double>
        {
            new public bool Contains(double scale)
            {
                double scaleR = Math.Round(scale);
                double scaleF = Math.Floor(scale);

                foreach (double s in this)
                {
                    if (Math.Round(s) == scaleR ||
                        Math.Floor(s) == scaleF)
                        return true;
                }
                return false;
            }

            public double GetScale(double scale)
            {
                double scaleR = Math.Round(scale);
                double scaleF = Math.Floor(scale);

                foreach (double s in this)
                {
                    if (Math.Round(s) == scaleR ||
                        Math.Floor(s) == scaleF)
                        return s;
                }
                return -1.0;
            }
            public void Order()
            {
                this.Sort();
                List<double> scales = ListOperations<double>.Swap((List<double>)this);

                this.Clear();
                foreach (double s in scales)
                {
                    this.Add(s);
                }
            }
        }
        #endregion
    }
}

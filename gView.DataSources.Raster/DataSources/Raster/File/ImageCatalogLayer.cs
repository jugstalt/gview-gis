using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using gView.Framework.IO;
using gView.Framework.Geometry;
using gView.Framework.Data;

namespace gView.DataSources.Raster.File
{
    /*
    internal class ImageCatalogLayer : gView.Framework.Data.IRasterLayer, gView.Framework.Data.IParentLayer, IPersistable
    {
        private bool _valid = false, _visible = true;
        private Polygon _polygon;
        private double _minScale = 0.0, _maxScale = 0.0;
        private AccessFDB _fdb = null;
        private string _title = "", _filename = "";
        private ISpatialReference _sRef = null;
        private IRasterDataset _dataset = null;
        private int _datasetID = -1;

        public IRasterClass RasterClass { get { return null; } }

        public ImageCatalogLayer()
        {
        }
        public ImageCatalogLayer(IRasterDataset dataset, string filename)
        {
            try
            {
                FileInfo fi = new FileInfo(_filename = filename);
                if (!fi.Exists) return;

                AccessFDBDataset ds = new AccessFDBDataset();
                ds.ConnectionString = "mdb=" + filename + ";dsname=RASTERCATALOG";
                ds.Open();
                
                _dataset = dataset;

                IEnvelope env = null;
                List<IDatasetElement> layers = dataset.Elements;
                if (layers == null) return;
                foreach (IDatasetElement element in layers)
                {
                    if (!(element is IFeatureLayer)) continue;
                    IFeatureLayer layer = (IFeatureLayer)element;
                    if (layer.FeatureClass == null) continue;
                    if (layer.FeatureClass.GeometryType == geometryType.Polygon &&
                        layer.FeatureClass.FindField("PATH")!=null &&
                        layer.FeatureClass.FindField("LAST_MODIFIED")!=null)
                    {
                        _title = layer.Title;
                        env = layer.FeatureClass.Envelope;
                        break;
                    }
                }
                dataset.Dispose();
                if (_title == "" || env == null) return;

                _fdb = new AccessFDB();
                if (!_fdb.Open(filename)) return;

                _sRef = _fdb.SpatialReference("RASTERCATALOG");

                calcPolygon(env);

                _valid = true;
            }
            catch
            {
                _valid = false;
            }
        }
        public void Dispose()
        {
            if (_fdb != null)
            {
                _fdb.Dispose();
                _fdb = null;
            }
        }

        private void calcPolygon(IEnvelope env)
        {
            _polygon = new Polygon();
            Ring ring = new Ring();
            ring.AddPoint(new Point(env.minx, env.miny));
            ring.AddPoint(new Point(env.maxx, env.miny));
            ring.AddPoint(new Point(env.maxx, env.maxy));
            ring.AddPoint(new Point(env.minx, env.maxy));
            _polygon.AddRing(ring);
        }

        public bool isValid { get { return _valid; } }

        #region IRasterLayer Members

        public gView.Framework.Geometry.IPolygon Polygon
        {
            get { return _polygon; }
        }

        public void BeginPaint()
        {
            
        }

        public void EndPaint()
        {
            
        }

        public System.Drawing.Color GetPixel(double X, double Y)
        {
            return System.Drawing.Color.Transparent;
        }

        public System.Drawing.Bitmap Bitmap
        {
            get { return null; }
        }

        public double oX
        {
            get { return 0.0; }
        }

        public double oY
        {
            get { return 0.0; }
        }

        public double dx1
        {
            get { return 0.0; }
        }

        public double dx2
        {
            get { return 0.0; }
        }

        public double dy1
        {
            get { return 0.0; }
        }

        public double dy2
        {
            get { return 0.0; }
        }

        public ISpatialReference SpatialReference
        {
            get { return _sRef; }
            set { _sRef = value; }
        }

        private InterpolationMethod _interpolMethod = InterpolationMethod.Fast;
        public InterpolationMethod InterpolationMethod
        {
            get { return _interpolMethod; }
            set { _interpolMethod = value; }
        }

        public IRasterDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion

        #region ILayer Members

        public int order
        {
            get
            {
                return 0;
            }
            set
            {
                
            }
        }

        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
            }
        }

        public double MinimumScale
        {
            get
            {
                return _minScale;
            }
            set
            {
                _minScale = value;
            }
        }

        public double MaximumScale
        {
            get
            {
                return _maxScale;
            }
            set
            {
                _maxScale = value;
            }
        }

        public int DatasetID
        {
            get { return _datasetID; }
            set { _datasetID = value; }
        }
        #endregion

        #region IDatasetElement Members

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        public IClass Class
        {
            get { return null; }
        }
        #endregion

        #region IParentLayer Members

        public List<gView.Framework.Data.ILayer> ChildLayers(gView.Framework.Carto.IDisplay display)
        {
            List<ILayer> layers = new List<ILayer>();

            if (!_valid || _fdb == null) return layers;


            SpatialFilter filter = new SpatialFilter();
            filter.AddField("PATH");
            filter.FuzzyQuery = false;
            filter.Geometry = display.Envelope;

            if (display.GeometricTransformer != null)
            {
                filter.Geometry = (IGeometry)display.GeometricTransformer.InvTransform2D(filter.Geometry);
            }

            lock (this)
            {
                IFeatureCursor cursor = _fdb.Query(_title, filter);

                IFeature feature;
                while ((feature = cursor.NextFeature) != null)
                {
                    if (feature.Shape is Polygon)
                    {
                        foreach (FieldValue fv in feature.Fields)
                        {
                            if (fv.Name != "PATH") continue;

                            string filename = fv.Value.ToString();
                            if (filename.IndexOf("jpg.mdb") != -1 ||
                                filename.IndexOf("png.mdb") != -1 ||
                                filename.IndexOf("tif.mdb") != -1)
                            {
                                PyramidFileLayer layer = new PyramidFileLayer(_dataset, filename, (Polygon)feature.Shape);
                                if (layer.isValid)
                                {
                                    layer.InterpolationMethod = this.InterpolationMethod;
                                    if (layer.SpatialReference == null) layer.SpatialReference = _sRef;
                                    layers.Add(layer);
                                }
                            }
                            else
                            {
                                RasterFileClass layer = new RasterFileClass(_dataset, filename, (Polygon)feature.Shape);
                                if (layer.isValid)
                                {
                                    layer.InterpolationMethod = this.InterpolationMethod;
                                    if (layer.SpatialReference == null) layer.SpatialReference = _sRef;
                                    layers.Add(layer);
                                }
                            }
                        }
                    }
                }
            }
            return layers;
        }

        #endregion

        #region IPersistable Members

        public string PersistID
        {
            get { return null; }
        }

        public void Load(IPersistStream stream)
        {
            _title = (string)stream.Load("title");
            _filename = (string)stream.Load("filename");
            _minScale = (double)stream.Load("minScale", 0.0);
            _maxScale = (double)stream.Load("maxScale", 0.0);
            InterpolationMethod = (InterpolationMethod)stream.Load("interpolation", InterpolationMethod.Fast);

            if (_fdb != null) _fdb.Dispose();
            _fdb = new AccessFDB();
            if (!_fdb.Open(_filename))
            {
                _fdb.Dispose();
                _fdb = null;
                _valid = false;
                return;
            }
            _sRef = _fdb.SpatialReference("RASTERCATALOG");
            IEnvelope env = _fdb.QueryExtent(_title);
            if (env != null)
            {
                calcPolygon(env);
                _valid = true;
            }
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("title", _title);
            stream.Save("filename", _filename);
            stream.Save("minScale", _minScale);
            stream.Save("maxScale", _maxScale);
            stream.Save("interpolation", (int)InterpolationMethod);
        }

        #endregion
    }
     * */
}

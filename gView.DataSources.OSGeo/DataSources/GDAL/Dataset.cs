using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using System.IO;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.IO;
using OSGeo;
using System.Threading.Tasks;

namespace gView.DataSources.GDAL
{
    [gView.Framework.system.RegisterPlugIn("43DFABF1-3D19-438c-84DA-F8BA0B266592")]
    public class Dataset : DatasetMetadata, IRasterFileDataset, IPersistable, IPlugInDependencies
    {
        private string _connectionString = "";
        private string _errMsg = "";
        //private V122::GDAL.Dataset _gdalDataset = null;
        private List<IDatasetElement> _layers = new List<IDatasetElement>();
        private string _directory = "";
        private DatasetState _state = DatasetState.opened;

        public Dataset() { }

        public IRasterLayer AddRasterFile(string filename)
        {
            return AddRasterFile(filename, null);
        }
        public IRasterLayer AddRasterFile(string filename, IPolygon polygon)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (_directory == "")
                {
                    _directory = fi.Directory.FullName;
                }

                RasterClass rasterClass = (polygon == null) ? new RasterClass(this, filename) : new RasterClass(this, filename, polygon);
                RasterLayer layer = new RasterLayer(rasterClass);
                if (rasterClass.isValid)
                {
                    _layers.Add(layer);
                }
                return layer;
            }
            catch { }
            return null;
        }

        #region IRasterFileDataset Member

        public string SupportedFileFilter
        {
            get { return "*.tif|*.tiff|*.jpg|*.jpeg|*.jp2|*.ecw|w001001.adf|*.gsd|*.sid"; }
        }

        public int SupportsFormat(string extension)
        {
            switch (extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                case ".tif":
                case ".tiff":
                case ".ecw":
                case ".adf":
                case ".gsd":
                case ".jp2":
                case ".sid":
                    return 200;
            }
            return -1;
        }

        public string FileName
        {
            get
            {
                return "";
            }
            set
            {
                AddRasterFile(value);
            }
        }
        #endregion

        #region IRasterDataset Member

        public Task<IEnvelope> Envelope()
        {
            IEnvelope env = null;
            foreach (IDatasetElement element in _layers)
            {
                if (element.Class is IRasterClass &&
                    ((IRasterClass)element.Class).Polygon != null)
                {
                    if (env == null)
                        env = ((IRasterClass)element.Class).Polygon.Envelope;
                    else
                        env.Union(((IRasterClass)element.Class).Polygon.Envelope);
                }
            }
            return Task.FromResult<IEnvelope>(env);
        }

        public Task<ISpatialReference> GetSpatialReference()
        {
            return Task.FromResult<ISpatialReference>(null);
        }
        public void SetSpatialReference(ISpatialReference sRef) { }

        #endregion

        #region IDataset Member

        public string ConnectionString
        {
            get
            {
                _connectionString = String.Empty;
                foreach (IDatasetElement layer in _layers)
                {
                    RasterClass rc = layer.Class as RasterClass;
                    if (rc == null) continue;

                    if (_connectionString != String.Empty) _connectionString += ";";
                    _connectionString += rc.Filename;
                }

                return _connectionString;
            }
        }
        public Task<bool> SetConnectionString(string value)
        {
            _layers.Clear();
            _connectionString = value;
            foreach (string filename in _connectionString.Split(';'))
            {
                if (filename.Trim() == String.Empty) continue;
                AddRasterFile(filename);
            }

            return Task.FromResult(true);
        }
        

        public string DatasetGroupName
        {
            get { return "GDAL Image Dataset"; }
        }

        public string DatasetName
        {
            get { return "GDAL"; }
        }

        public string ProviderName
        {
            get { return "gView"; }
        }

        public DatasetState State
        {
            get { return _state; }
        }

        public Task<bool> Open()
        {
            return Task.FromResult(true);
        }

        public string LastErrorMessage
        {
            get { return _errMsg; }
            set { _errMsg = value; }
        }

        public Task<List<IDatasetElement>> Elements()
        {
            return Task.FromResult(ListOperations<IDatasetElement>.Clone(_layers));
        }

        public string Query_FieldPrefix
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public string Query_FieldPostfix
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public gView.Framework.FDB.IDatabase Database
        {
            get { return null; }
        }

        public Task<IDatasetElement> Element(string title)
        {
            foreach (IDatasetElement element in _layers)
            {
                if (element.Title == title)
                    return Task.FromResult(element);
            }
            return Task.FromResult((IDatasetElement)null);
        }

        async public Task RefreshClasses()
        {
        }
        #endregion

        #region IDisposable Member

        public void Dispose()
        {
           
        }

        #endregion

        #region IPersistable Member

        public Task<bool> Load(IPersistStream stream)
        {
            SetConnectionString((string)stream.Load("filename", String.Empty));

            //if (_layers.Count == 1 && _layers[0].Class is IPersistable)
            //{
            //    stream.Load("RasterClass", null, _layers[0].Class);
            //}

            return Task.FromResult(true);
        }

        public Task<bool> Save(IPersistStream stream)
        {
            stream.Save("filename", ConnectionString);

            //if (_layers.Count == 1 && _layers[0].Class is IPersistable)
            //{
            //    stream.Save("RasterClass", _layers[0].Class);
            //}

            return Task.FromResult(true);
        }

        #endregion

        #region IPlugInDependencies Member

        public bool HasUnsolvedDependencies()
        {
            return Dataset.hasUnsolvedDependencies;
        }

        #endregion

        static public bool hasUnsolvedDependencies
        {
            get
            {
                try
                {
                    OSGeo.GDAL.Gdal.AllRegister();

                    return false;
                }
                catch (Exception ex)
                {
                    return true;
                }
            }
        }
    }
}

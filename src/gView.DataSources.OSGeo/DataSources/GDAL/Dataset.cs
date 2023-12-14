using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using gView.Framework.Data;
using gView.Framework.Data.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using gView.Framework.IO;

namespace gView.DataSources.GDAL
{
    [RegisterPlugIn("43DFABF1-3D19-438c-84DA-F8BA0B266592")]
    public class Dataset : DatasetMetadata, IRasterFileDataset, IPlugInDependencies
    {
        private string _connectionString = "";
        private string _errMsg = "";
        private List<IDatasetElement> _layers = new List<IDatasetElement>();
        private string _directory = "";
        private DatasetState _state = DatasetState.opened;

        public Dataset()
        {
            OSGeo.Initializer.RegisterAll();
        }

        public IRasterLayer AddRasterFile(string filename)
        {
            return AddRasterFile(filename, null);
        }

        public IRasterLayer AddRasterFile(string filename, IPolygon polygon)
        {
            try
            {
                FileInfo fi = FileInfoFactory.Create(filename);

                if (_directory == "")
                {
                    _directory = fi.Directory.FullName;
                }

                IRasterClass rasterClass = null;
                switch (OSGeo.Initializer.InstalledVersion)
                {
                    case OSGeo.GdalVersion.V3:
                        rasterClass = (polygon == null) ? new RasterClassV3(this, filename) : new RasterClassV3(this, filename, polygon);
                        if (((RasterClassV3)rasterClass).IsValid == false)
                        {
                            rasterClass = null;
                        }
                        break;

                    case OSGeo.GdalVersion.V1:
                        rasterClass = (polygon == null) ? new RasterClassV1(this, filename) : new RasterClassV1(this, filename, polygon);
                        if (((RasterClassV1)rasterClass).isValid == false)
                        {
                            rasterClass = null;
                        }
                        break;

                    default:
                        throw new Exception("No GDAL Version detected/installed");
                }

                if (rasterClass != null)
                {
                    RasterLayer layer = new RasterLayer(rasterClass);
                    _layers.Add(layer);

                    return layer;
                }
            }
            catch (Exception ex)
            {
                this.LastErrorMessage = ex.Message;
            }
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

        #endregion IRasterFileDataset Member

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
                    {
                        env = ((IRasterClass)element.Class).Polygon.Envelope;
                    }
                    else
                    {
                        env.Union(((IRasterClass)element.Class).Polygon.Envelope);
                    }
                }
            }
            return Task.FromResult<IEnvelope>(env);
        }

        public Task<ISpatialReference> GetSpatialReference()
        {
            return Task.FromResult<ISpatialReference>(null);
        }

        public void SetSpatialReference(ISpatialReference sRef)
        {
        }

        #endregion IRasterDataset Member

        #region IDataset Member

        public string ConnectionString
        {
            get
            {
                _connectionString = String.Empty;
                foreach (IDatasetElement layer in _layers)
                {
                    IRasterFile rc = null;
                    switch (OSGeo.Initializer.InstalledVersion)
                    {
                        case OSGeo.GdalVersion.V3:
                            rc = layer.Class as RasterClassV3;
                            if (rc == null)
                            {
                                continue;
                            }
                            break;

                        case OSGeo.GdalVersion.V1:
                            rc = layer.Class as RasterClassV1;
                            if (rc == null)
                            {
                                continue;
                            }
                            break;

                        default:
                            continue;
                    }

                    if (_connectionString != String.Empty)
                    {
                        _connectionString += ";";
                    }

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
                if (filename.Trim() == String.Empty)
                {
                    continue;
                }

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

        public IDatabase Database
        {
            get { return null; }
        }

        public Task<IDatasetElement> Element(string title)
        {
            foreach (IDatasetElement element in _layers)
            {
                if (element.Title == title)
                {
                    return Task.FromResult(element);
                }
            }
            return Task.FromResult((IDatasetElement)null);
        }

        public Task RefreshClasses()
        {
            return Task.CompletedTask;
        }

        #endregion IDataset Member

        #region IDisposable Member

        public void Dispose()
        {
        }

        #endregion IDisposable Member

        #region IPersistable Member

        async public Task<bool> LoadAsync(IPersistStream stream)
        {
            await SetConnectionString((string)stream.Load("filename", String.Empty));

            return true;
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("filename", ConnectionString);
        }

        #endregion IPersistable Member

        #region IPlugInDependencies Member

        public bool HasUnsolvedDependencies()
        {
            return Dataset.hasUnsolvedDependencies;
        }

        #endregion IPlugInDependencies Member

        static public bool hasUnsolvedDependencies
        {
            get
            {
                return OSGeo.Initializer.RegisterAll() != true;
            }
        }
    }
}
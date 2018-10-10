using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using System.IO;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.IO;
using OSGeo;

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

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get
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
                return env;
            }
        }

        public gView.Framework.Geometry.ISpatialReference SpatialReference
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }

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
            set
            {
                _layers.Clear();
                _connectionString = value;
                foreach (string filename in _connectionString.Split(';'))
                {
                    if (filename.Trim() == String.Empty) continue;
                    AddRasterFile(filename);
                }
            }
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

        public bool Open()
        {
            return true;
        }

        public string lastErrorMsg
        {
            get { return _errMsg; }
        }

        public List<IDatasetElement> Elements
        {
            get
            {
                return ListOperations<IDatasetElement>.Clone(_layers);
            }
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

        public IDatasetElement this[string title]
        {
            get
            {
                foreach (IDatasetElement element in _layers)
                {
                    if (element.Title == title) return element;
                }
                return null;
            }
        }

        public void RefreshClasses()
        {
        }
        #endregion

        #region IDisposable Member

        public void Dispose()
        {
           
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            ConnectionString = (string)stream.Load("filename", String.Empty);

            //if (_layers.Count == 1 && _layers[0].Class is IPersistable)
            //{
            //    stream.Load("RasterClass", null, _layers[0].Class);
            //}
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("filename", ConnectionString);

            //if (_layers.Count == 1 && _layers[0].Class is IPersistable)
            //{
            //    stream.Save("RasterClass", _layers[0].Class);
            //}
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

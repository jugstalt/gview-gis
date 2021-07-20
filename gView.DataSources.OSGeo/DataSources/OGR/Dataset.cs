using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataSources.OGR
{
    [gView.Framework.system.RegisterPlugIn("7a972c29-4955-4899-b142-98f6e05bb063")]
    public class Dataset : DatasetMetadata, IFeatureDataset, IDisposable, IDataset2, IPlugInDependencies
    {
        private string _connectionString, _lastErrMsg = "";
        private OSGeo_v1.OGR.DataSource _dataSourceV1 = null;
        private OSGeo_v3.OGR.DataSource _dataSourceV3 = null;
        private DatasetState _state = DatasetState.unknown;
        private List<IDatasetElement> _elements = null;

        public Dataset()
        {
            OSGeo.Initializer.RegisterAll();
        }

        #region IFeatureDataset Member

        public Task<IEnvelope> Envelope()
        {
            return Task.FromResult<IEnvelope>(new Envelope());
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
                return _connectionString;
            }
        }
        public Task<bool> SetConnectionString(string value)
        {
            _connectionString = value;

            return Task.FromResult(true);
        }

        public string DatasetGroupName
        {
            get { return "gView.GDAL"; }
        }

        public string DatasetName
        {
            get { return "gView.OGR"; }
        }

        public string ProviderName
        {
            get { return "gView.GIS"; }
        }

        public DatasetState State
        {
            get { return _state; }
        }

        public Task<bool> Open()
        {
            try
            {
                switch (OSGeo.Initializer.InstalledVersion)
                {
                    case OSGeo.GdalVersion.V1:
                        _dataSourceV1 = OSGeo_v1.OGR.Ogr.Open(_connectionString, 0);
                        if (_dataSourceV1 != null)
                        {
                            _state = DatasetState.opened;
                            return Task.FromResult(true);
                        }
                        break;
                    case OSGeo.GdalVersion.V3:
                        _dataSourceV3 = OSGeo_v3.OGR.Ogr.Open(_connectionString, 0);
                        if (_dataSourceV3 != null)
                        {
                            _state = DatasetState.opened;
                            return Task.FromResult(true);
                        }
                        break;
                }


                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _lastErrMsg = ex.Message;
                return Task.FromResult(false);
            }
        }

        public string LastErrorMessage
        {
            get { return _lastErrMsg; }
            set { _lastErrMsg = value; }
        }

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

        public IDatasetEnum DatasetEnum
        {
            get { return null; }
        }

        public Task<List<IDatasetElement>> Elements()
        {
            if (_elements != null && _elements.Count > 0)
            {
                return Task.FromResult(_elements);
            }

            _elements = new List<IDatasetElement>();

            if (_dataSourceV1 == null && _dataSourceV3 == null)
            {
                return Task.FromResult(_elements);
            }

            var layerCount = _dataSourceV1?.GetLayerCount() ??
                             _dataSourceV3?.GetLayerCount();

            for (int i = 0; i < layerCount.Value; i++)
            {
                IFeatureClass fc = null;
                switch (OSGeo.Initializer.InstalledVersion)
                {
                    case OSGeo.GdalVersion.V1:
                        OSGeo_v1.OGR.Layer ogrLayerV1 = _dataSourceV1?.GetLayerByIndex(i);

                        if (ogrLayerV1 == null)
                        {
                            continue;
                        }

                        fc = new FeatureClassV1(this, ogrLayerV1);
                        break;
                    case OSGeo.GdalVersion.V3:
                        using (OSGeo_v3.OGR.Layer ogrLayerV3 = _dataSourceV3?.GetLayerByIndex(i))
                        {
                            if (ogrLayerV3 == null)
                            {
                                continue;
                            }
                        }

                        fc = new FeatureClassV3(this, _dataSourceV3, i);
                        break;
                    default:
                        throw new Exception("No OGR Version detected/installed");
                }

                _elements.Add(new DatasetElement(fc));
            }

            return Task.FromResult(_elements);
        }

        public string Query_FieldPrefix
        {
            get { return String.Empty; }
        }

        public string Query_FieldPostfix
        {
            get { return String.Empty; }
        }

        public gView.Framework.FDB.IDatabase Database
        {
            get
            {
                return null;
            }
        }

        async public Task<IDatasetElement> Element(string title)
        {
            foreach (IDatasetElement element in await this.Elements())
            {
                if (element.Class != null &&
                    element.Class.Name == title)
                {
                    return element;
                }
            }

            return null;
        }

        async public Task RefreshClasses()
        {
        }
        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            if (_dataSourceV1 != null)
            {
                _dataSourceV1.Dispose();
                _dataSourceV1 = null;
            }
            if (_dataSourceV3 != null)
            {
                _dataSourceV3.Dispose();
                _dataSourceV3 = null;
            }
        }

        #endregion

        static public bool hasUnsolvedDependencies
        {
            get
            {
                return OSGeo.Initializer.RegisterAll() != true;
            }
        }

        #region IDataset2 Member

        async public Task<IDataset2> EmptyCopy()
        {
            Dataset dataset = new Dataset();
            await dataset.SetConnectionString(ConnectionString);
            await dataset.Open();

            return dataset;
        }

        async public Task AppendElement(string elementName)
        {
            if (_elements == null)
            {
                _elements = new List<IDatasetElement>();
            }

            foreach (IDatasetElement e in _elements)
            {
                if (e.Title == elementName)
                {
                    return;
                }
            }

            if (_dataSourceV1 != null || _dataSourceV3 != null)
            {
                var layerCount = _dataSourceV1?.GetLayerCount() ??
                                 _dataSourceV3?.GetLayerCount();

                for (int i = 0; i < layerCount.Value; i++)
                {

                    string ogrLayerName = _dataSourceV1?.GetLayerByIndex(i)?.GetName() ??
                                          _dataSourceV3?.GetLayerByIndex(i)?.GetName();

                    if (ogrLayerName == elementName)
                    {
                        IFeatureClass fc = null;
                        switch (OSGeo.Initializer.InstalledVersion)
                        {
                            case OSGeo.GdalVersion.V1:
                                OSGeo_v1.OGR.Layer ogrLayerV1 = _dataSourceV1.GetLayerByIndex(i);
                                if (ogrLayerV1 == null)
                                {
                                    continue;
                                }

                                fc = new FeatureClassV1(this, ogrLayerV1);
                                break;
                            case OSGeo.GdalVersion.V3:
                                using (OSGeo_v3.OGR.Layer ogrLayerV3 = _dataSourceV3.GetLayerByIndex(i))
                                {
                                    if (ogrLayerV3 == null)
                                    {
                                        continue;
                                    }
                                }
                                fc = new FeatureClassV3(this, _dataSourceV3, i);
                                break;
                            default:
                                throw new Exception("No OGR Version detected/installed");
                        }

                        _elements.Add(new DatasetElement(fc));
                        break;
                    }
                }
            }
        }

        #endregion

        #region IPlugInDependencies Member

        public bool HasUnsolvedDependencies()
        {
            return hasUnsolvedDependencies;
        }

        #endregion

        #region IPersistable Member

        public Task<bool> LoadAsync(IPersistStream stream)
        {
            _connectionString = (string)stream.Load("connectionstring", String.Empty);

            return Task.FromResult(true);
        }

        public void Save(IPersistStream stream)
        {
            stream.SaveEncrypted("connectionstring", _connectionString);
            //this.Open();
        }

        #endregion
    }
}

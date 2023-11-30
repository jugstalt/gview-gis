using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.system;
using gView.Framework.Data;
using gView.Framework.Data.Metadata;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataSources.OGR
{
    [RegisterPlugIn("7a972c29-4955-4899-b142-98f6e05bb063")]
    public class Dataset : DatasetMetadata, IFeatureDataset, IDisposable, IDataset2, IPlugInDependencies
    {
        private string _connectionString, _lastErrMsg = "";
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
                        using (var dataSourceV1 = OSGeo_v1.OGR.Ogr.Open(_connectionString, 0))
                        {
                            if (dataSourceV1 != null)
                            {
                                _state = DatasetState.opened;
                                return Task.FromResult(true);
                            }
                        }

                        break;
                    case OSGeo.GdalVersion.V3:
                        using (var dataSourceV3 = OSGeo_v3.OGR.Ogr.Open(_connectionString, 0))
                        {
                            if (dataSourceV3 != null)
                            {
                                _state = DatasetState.opened;
                                return Task.FromResult(true);
                            }
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

            if (_state != DatasetState.opened)
            {
                return Task.FromResult(_elements);
            }

            if (OSGeo.Initializer.InstalledVersion == OSGeo.GdalVersion.V1)
            {
                using (var dataSourceV1 = OSGeo_v1.OGR.Ogr.Open(_connectionString, 0))
                {
                    var layerCount = dataSourceV1.GetLayerCount();

                    for (int i = 0; i < layerCount; i++)
                    {
                        OSGeo_v1.OGR.Layer ogrLayerV1 = dataSourceV1?.GetLayerByIndex(i);

                        if (ogrLayerV1 != null)
                        {
                            _elements.Add(new DatasetElement(new FeatureClassV1(this, ogrLayerV1)));
                        }
                    }
                }
            }
            else if (OSGeo.Initializer.InstalledVersion == OSGeo.GdalVersion.V3)
            {
                using (var dataSourceV3 = OSGeo_v3.OGR.Ogr.Open(_connectionString, 0))
                {
                    var layerCount = dataSourceV3.GetLayerCount();

                    for (int i = 0; i < layerCount; i++)
                    {
                        using (var ogrLayerV3 = dataSourceV3.GetLayerByIndex(i))
                        {
                            if (ogrLayerV3 != null)
                            {
                                _elements.Add(new DatasetElement(new FeatureClassV3(this, ogrLayerV3.GetName())));
                            }
                        }
                    }
                }
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

        public IDatabase Database
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

        public Task RefreshClasses()
        {
            return Task.CompletedTask;
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

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

        public Task AppendElement(string elementName)
        {
            if (_elements == null)
            {
                _elements = new List<IDatasetElement>();
            }

            foreach (IDatasetElement e in _elements)
            {
                if (e.Title == elementName)
                {
                    return Task.CompletedTask;
                }
            }

            if (_state == DatasetState.opened)
            {
                if (OSGeo.Initializer.InstalledVersion == OSGeo.GdalVersion.V1)
                {
                    using (var dataSourceV1 = OSGeo_v1.OGR.Ogr.Open(_connectionString, 0))
                    {
                        var layerCount = dataSourceV1.GetLayerCount();

                        for (int i = 0; i < layerCount; i++)
                        {
                            string ogrLayerName = dataSourceV1.GetLayerByIndex(i)?.GetName();
                            if (ogrLayerName == elementName)
                            {
                                OSGeo_v1.OGR.Layer ogrLayerV1 = dataSourceV1.GetLayerByIndex(i);
                                if (ogrLayerV1 != null)
                                {
                                    _elements.Add(new DatasetElement(new FeatureClassV1(this, ogrLayerV1)));
                                }
                            }
                        }
                    }
                }
                else if (OSGeo.Initializer.InstalledVersion == OSGeo.GdalVersion.V3)
                {
                    using (var dataSourceV3 = OSGeo_v3.OGR.Ogr.Open(_connectionString, 0))
                    {
                        var layerCount = dataSourceV3.GetLayerCount();

                        for (int i = 0; i < layerCount; i++)
                        {
                            string ogrLayerName = dataSourceV3.GetLayerByIndex(i)?.GetName();
                            if (ogrLayerName == elementName)
                            {
                                using (var ogrLayerV3 = dataSourceV3.GetLayerByIndex(i))
                                {
                                    if (ogrLayerV3 != null)
                                    {
                                        _elements.Add(new DatasetElement(new FeatureClassV3(this, ogrLayerV3.GetName())));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return Task.CompletedTask;
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

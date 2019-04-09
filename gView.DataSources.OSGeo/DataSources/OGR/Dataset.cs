using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.system;
using gView.Framework.Geometry;
using gView.Framework.IO;
using System.Threading.Tasks;

namespace gView.DataSources.OGR
{
    [gView.Framework.system.RegisterPlugIn("7a972c29-4955-4899-b142-98f6e05bb063")]
    public class Dataset : DatasetMetadata, IFeatureDataset, IDisposable, IDataset2, IPersistable, IPlugInDependencies
    {
        private string _connectionString, _lastErrMsg = "";
        private OSGeo.OGR.DataSource _dataSource = null;
        private DatasetState _state = DatasetState.unknown;
        private List<IDatasetElement> _elements = null;

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
                OSGeo.OGR.Ogr.RegisterAll();

                _dataSource = OSGeo.OGR.Ogr.Open(_connectionString, 0);
                if (_dataSource != null)
                {
                    _state = DatasetState.opened;
                    return Task.FromResult(true);
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
                return Task.FromResult(_elements);

            _elements = new List<IDatasetElement>();

            if (_dataSource == null) return Task.FromResult(_elements);
            for (int i = 0; i < _dataSource.GetLayerCount(); i++)
            {
                OSGeo.OGR.Layer ogrLayer = _dataSource.GetLayerByIndex(i);
                if (ogrLayer == null) continue;

                _elements.Add(new DatasetElement(
                    new FeatureClass(this, ogrLayer)));
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
                    return element;
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
            //if (_dataSource != null)
            //{
            //    _dataSource.Dispose();
            //    _dataSource = null;
            //}
        }

        #endregion

        static public bool hasUnsolvedDependencies
        {
            get
            {
                try
                {
                    OSGeo.OGR.Ogr.RegisterAll();

                    return false;
                }
                catch (Exception ex)
                {
                    return true;
                }
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
            if (_elements == null) _elements = new List<IDatasetElement>();

            foreach (IDatasetElement e in _elements)
            {
                if (e.Title == elementName)
                    return;
            }

            if (_dataSource != null)
            {
                for (int i = 0; i < _dataSource.GetLayerCount(); i++)
                {
                    OSGeo.OGR.Layer ogrLayer = _dataSource.GetLayerByIndex(i);
                    if (ogrLayer == null) continue;

                    if (ogrLayer.GetName() == elementName)
                    {
                        _elements.Add(new DatasetElement(new FeatureClass(this, ogrLayer)));
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

        public Task<bool> Load(IPersistStream stream)
        {
            _connectionString = (string)stream.Load("connectionstring", String.Empty);

            return Task.FromResult(true);
        }

        public Task<bool> Save(IPersistStream stream)
        {
            stream.SaveEncrypted("connectionstring", _connectionString);
            //this.Open();

            return Task.FromResult(true);
        }

        #endregion
    }
}

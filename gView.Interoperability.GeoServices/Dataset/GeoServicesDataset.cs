using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Web;
using gView.Interoperability.GeoServices.Rest.Json;
using gView.MapServer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Interoperability.GeoServices.Dataset
{
    [gView.Framework.system.RegisterPlugIn("C4D644FE-8125-4214-99E1-0F0BC5884EDB")]
    public class GeoServicesDataset : DatasetMetadata, IFeatureDataset, IRequestDependentDataset
    {
        internal string _connection = "";
        internal string _name = "";
        internal List<IWebServiceTheme> _themes = new List<IWebServiceTheme>();
        private GeoServicesClass _class = null;
        private IEnvelope _envelope;
        private string _errMsg = "";
        //internal dotNETConnector _connector = null;
        private DatasetState _state = DatasetState.unknown;
        private ISpatialReference _sRef = null;

        public GeoServicesDataset() { }

        public GeoServicesDataset(string connection, string name)
        {
            _connection = connection;
            _name = name;

            _class = new GeoServicesClass(this);
        }

        internal GeoServicesClass WebServiceClass
        {
            get { return _class as GeoServicesClass; }
        }

        #region IFeatureDataset Member

        public Task<IEnvelope> Envelope()
        {
            return Task.FromResult<IEnvelope>(_envelope);
        }

        public Task<ISpatialReference> GetSpatialReference()
        {
            return Task.FromResult(_sRef);
        }
        public void SetSpatialReference(ISpatialReference sRef)
        {
            _sRef = sRef;
        }

        #endregion

        #region IDataset Member

        public void Dispose()
        {
            _state = DatasetState.unknown;
        }

        public string ConnectionString
        {
            get
            {
                return _connection + ";service=" + _name;
            }
        }
        public Task<bool> SetConnectionString(string value)
        {
            _connection = "server=" + ConfigTextStream.ExtractValue(value, "server") +
                            ";user=" + ConfigTextStream.ExtractValue(value, "user") +
                            ";pwd=" + ConfigTextStream.ExtractValue(value, "pwd");
            _name = ConfigTextStream.ExtractValue(value, "service");

            return Task.FromResult(true);
        }

        public string DatasetGroupName
        {
            get { return "GeoServices"; }
        }

        public string DatasetName
        {
            get { return "GeoServices Service"; }
        }

        public string ProviderName
        {
            get { return "gView"; }
        }

        public DatasetState State
        {
            get { return _state; }
        }

        async public Task<bool> Open()
        {
            return await Open(null);
        }

        public string LastErrorMessage
        {
            get { return _errMsg; }
            set { _errMsg = value; }
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
            List<IDatasetElement> elements = new List<IDatasetElement>();
            if (_class != null)
            {
                elements.Add(new DatasetElement(_class));
            }
            return Task.FromResult<List<IDatasetElement>>(elements);
        }

        public string Query_FieldPrefix
        {
            get { return ""; }
        }

        public string Query_FieldPostfix
        {
            get { return ""; }
        }

        public gView.Framework.FDB.IDatabase Database
        {
            get { return null; }
        }

        public Task<IDatasetElement> Element(string title)
        {
            if (_class != null && title == _class.Name)
            {
                return Task.FromResult<IDatasetElement>(new DatasetElement(_class));
            }

            return Task.FromResult<IDatasetElement>(null);
        }

        async public Task RefreshClasses()
        {
        }
        #endregion

        #region IRequestDependentDataset Member

        async public Task<bool> Open(IServiceRequestContext context)
        {
            if (_class == null)
            {
                _class = new GeoServicesClass(this);
            }
            _themes = new List<IWebServiceTheme>();

            string server = ConfigTextStream.ExtractValue(ConnectionString, "server");
            string service = ConfigTextStream.ExtractValue(ConnectionString, "service");
            string user = ConfigTextStream.ExtractValue(ConnectionString, "user");
            string pwd = ConfigTextStream.ExtractValue(ConnectionString, "pwd");

            var jsonMapService = await TryPostAsync<JsonMapService>($"{service}?f=json");
            var jsonLayers = await TryPostAsync<JsonLayers>($"{service}/layers?f=json");

            if (jsonMapService != null)
            {
                _class.Name = jsonMapService.MapName;

                if (jsonLayers?.Layers != null)
                {
                    foreach (var jsonLayer in jsonLayers.Layers)
                    {
                        IClass themeClass = null;
                        IWebServiceTheme theme = null;

                        if (jsonLayer.Type.ToLower() == "feature layer")
                        {
                            var featureClass = new GeoServicesFeatureClass(this, jsonLayer);

                        }

                        if (themeClass == null)
                        {
                            continue;
                        }

                        theme = LayerFactory.Create(themeClass, _class as IWebServiceClass) as IWebServiceTheme;
                        if (theme == null)
                        {
                            continue;
                        }

                        theme.Visible = false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region IPersistableAsync Member

        async public Task<bool> LoadAsync(IPersistStream stream)
        {
            await this.SetConnectionString((string)stream.Load("ConnectionString", ""));

            _class = new GeoServicesClass(this);
            return await Open();
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("ConnectionString", this.ConnectionString);
        }

        #endregion

        #region Helper

        async private Task<T> TryPostAsync<T>(string url)
        {
            try
            {
                var result = await WebFunctions.DownloadObjectAsync<T>(url);

                return result;
            }
            catch  // To Cache auth error
            {
            }

            return default(T);
        }

        #endregion
    }
}

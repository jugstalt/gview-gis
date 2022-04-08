using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Web;
using gView.Interoperability.OGC.Dataset.WFS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Interoperability.OGC.Dataset.WMS
{
    public enum SERVICE_TYPE { WMS = 0, WFS = 1, WMS_WFS = 2 };

    [gView.Framework.system.RegisterPlugIn("538F0731-31FE-493a-B063-10A2D37D6E6D")]
    public class WMSDataset : DatasetMetadata, IFeatureDataset, IRequestDependentDataset
    {
        internal string _connection = "", _connectionString;
        private IEnvelope _envelope = null;
        private string _errMsg = "";
        private WMSClass _class = null;
        private WFSDataset _wfsDataset = null;
        internal string _name = "";
        private bool _isOpened = false;
        private SERVICE_TYPE _type = SERVICE_TYPE.WMS;
        private DatasetState _state = DatasetState.unknown;

        public WMSDataset() { }

        async static public Task<WMSDataset> Create(string connectionString, string name)
        {
            var dataset = new WMSDataset();
            dataset._name = name;

            await dataset.SetConnectionString(connectionString);

            return dataset;
        }

        internal WMSClass WebServiceClass
        {
            get { return _class as WMSClass; }
        }

        internal bool IsOpened
        {
            get { return _isOpened; }
        }

        internal SERVICE_TYPE ServiceType
        {
            get { return _type; }
        }

        #region IFeatureDataset Member

        public Task<IEnvelope> Envelope()
        {
            return Task.FromResult(_envelope);
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
            _class = null;
            _wfsDataset = null;
            switch (ConfigTextStream.ExtractValue(value, "service").ToUpper())
            {
                case "WMS":
                    _type = SERVICE_TYPE.WMS;
                    _connection = ConfigTextStream.ExtractValue(value, "wms");
                    _class = new WMSClass(this);
                    break;
                case "WFS":
                    _type = SERVICE_TYPE.WFS;
                    _wfsDataset = new WFSDataset(this, ConfigTextStream.ExtractValue(value, "wfs"));
                    break;
                case "WMS_WFS":
                    _type = SERVICE_TYPE.WMS_WFS;
                    _connection = ConfigTextStream.ExtractValue(value, "wms");
                    _class = new WMSClass(this);
                    _wfsDataset = new WFSDataset(this, ConfigTextStream.ExtractValue(value, "wfs"));
                    break;
                default:
                    return Task.FromResult(false);
            }
            _connectionString = value;

            return Task.FromResult(true);
        }

        public string DatasetGroupName
        {
            get { return "OGC WMS"; }
        }

        public string DatasetName
        {
            get { return "OGC WMS Dataset"; }
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
            if (_class == null && _wfsDataset != null)
            {
                foreach (IDatasetElement element in _wfsDataset.Elements)
                {
                    elements.Add(element);
                }
            }

            return Task.FromResult(elements);
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
            if (_class != null && (
                title == _class.Name || title.ToLower() == ConnectionString.ToLower()))
            {
                return Task.FromResult<IDatasetElement>(new DatasetElement(_class));
            }

            if (_wfsDataset != null)
            {
                foreach (IDatasetElement element in _wfsDataset.Elements)
                {
                    if (element.Title == title)
                    {
                        return Task.FromResult(element);
                    }
                }
            }

            return Task.FromResult<IDatasetElement>(null);
        }

        async public Task RefreshClasses()
        {
        }
        #endregion

        #region IRequestDependentDataset Member

        async public Task<bool> Open(gView.MapServer.IServiceRequestContext context)
        {
            string url = String.Empty;
            try
            {
                _isOpened = true;
                bool ret = true;

                if (_wfsDataset != null)
                {
                    ret = _wfsDataset.Open();
                }
                if (_class != null)
                {
                    string param = "REQUEST=GetCapabilities&VERSION=1.1.1&SERVICE=WMS";

                    url = Append2Url(_connection, param);
                    string response = WebFunctions.HttpSendRequest(url, "GET", null,
                                ConfigTextStream.ExtractValue(_connectionString, "usr"),
                                ConfigTextStream.ExtractValue(_connectionString, "pwd"));

                    response = RemoveDOCTYPE(response);

                    _class.Init(response, _wfsDataset);
                }

                _state = (ret) ? DatasetState.opened : DatasetState.unknown;

                return ret;
            }
            catch (Exception ex)
            {
                await WMSClass.ErrorLogAsync(context, "GetCapabilities", url, ex);
                _class = null;
                _wfsDataset = null;

                return false;
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region IPersistableLoadAsync Member

        async public Task<bool> LoadAsync(IPersistStream stream)
        {
            await this.SetConnectionString((string)stream.Load("ConnectionString", ""));

            //switch (_type)
            //{
            //    case SERVICE_TYPE.WMS:
            //        _class = new WMSClass(this);
            //        break;
            //    case SERVICE_TYPE.WFS:
            //        _wfsDataset = new WFSDataset(this, _connection);
            //        break;
            //    case SERVICE_TYPE.WMS_WFS:
            //        _class = new WMSClass(this);
            //        _wfsDataset = new WFSDataset(this, _connection);
            //        break;
            //}

            if (await Open())
            {

                if (_class != null)
                {
                    _class.SRSCode = (string)stream.Load("WMS_SRS", String.Empty); ;
                    _class.GetMapFormat = (string)stream.Load("WMS_GetMapFormat", String.Empty);
                    _class.FeatureInfoFormat = (string)stream.Load("WMS_InfoFormat", String.Empty);
                    _class.UseSLD_BODY = (bool)stream.Load("UseSLD_BODY", _class.UseSLD_BODY);
                }

                return true;
            }

            return false;
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("ConnectionString", this.ConnectionString);
            stream.Save("Type", (int)_type);

            if (_class != null)
            {
                stream.Save("WMS_SRS", _class.SRSCode);
                stream.Save("WMS_GetMapFormat", _class.GetMapFormat);
                stream.Save("WMS_InfoFormat", _class.FeatureInfoFormat);
                stream.Save("UseSLD_BODY", _class.UseSLD_BODY);
            }
        }

        #endregion

        internal static string RemoveDOCTYPE(string xml)
        {
            int pos = xml.IndexOf("<!DOCTYPE");
            if (pos != -1)
            {
                int o = 1, i;
                for (i = pos + 1; i < xml.Length; i++)
                {
                    if (xml[i] == '<')
                    {
                        o++;
                    }
                    else if (xml[i] == '>')
                    {
                        o--;
                        if (o == 0)
                        {
                            break;
                        }
                    }
                }

                string s1 = xml.Substring(0, pos);
                string s2 = xml.Substring(i + 1, xml.Length - i - 1);

                return s1 + s2;
            }

            return xml;
        }

        public static string Append2Url(string url, string parameters)
        {
            string c = "?";
            if (url.EndsWith("?") || url.EndsWith("&"))
            {
                c = "";
            }
            else if (url.Contains("?"))
            {
                c = "&";
            }

            return url + c + parameters;
        }


    }
}

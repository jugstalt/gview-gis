using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.MapServer;
using gView.Framework.Core.Common;
using gView.Framework.Data;
using gView.Framework.Data.Metadata;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Web.Abstraction;
using gView.Framework.Web.Services;
using gView.Interoperability.GeoServices.Rest.DTOs;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using gView.Framework.Common.Extensions;
using gView.Framework.Common.Json;

namespace gView.Interoperability.GeoServices.Dataset
{
    [RegisterPlugIn("C4D644FE-8125-4214-99E1-0F0BC5884EDB")]
    public class GeoServicesDataset : DatasetMetadata, IFeatureDataset, IRequestDependentDataset
    {
        internal readonly IHttpService _http;

        internal string _connection = "";
        internal string _name = "";
        internal List<IWebServiceTheme> _themes = new List<IWebServiceTheme>();
        private GeoServicesClass _class = null;
        private IEnvelope _envelope = null;
        private string _errMsg = "";
        private DatasetState _state = DatasetState.unknown;
        private ISpatialReference _sRef = null;
        private string _token;

        public GeoServicesDataset()
        {
            _http = HttpService.CreateInstance();
        }

        public GeoServicesDataset(string connection, string name)
            : this()
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

        public IDatabase Database
        {
            get { return null; }
        }

        public Task<IDatasetElement> Element(string title)
        {
            if (_class != null && title == _class.Name)
            {
                return Task.FromResult<IDatasetElement>(new DatasetElement(_class));
            }

            if (_class != null && _class.Themes != null)
            {
                var theme = _class.Themes.Where(t => t.Title == title).FirstOrDefault();
                if (theme?.FeatureClass != null)
                {
                    return Task.FromResult<IDatasetElement>(new DatasetElement(theme.FeatureClass));
                }
            }

            return Task.FromResult<IDatasetElement>(null);
        }

        public Task RefreshClasses()
        {
            return Task.CompletedTask;
        }
        #endregion

        #region IRequestDependentDataset Member

        async public Task<bool> Open(IServiceRequestContext context)
        {
            if (_class == null)
            {
                _class = new GeoServicesClass(this);
            }
            _class.Themes.Clear();

            _themes = new List<IWebServiceTheme>();

            string serviceUrl = ServiceUrl();
            string user = ConfigTextStream.ExtractValue(ConnectionString, "user");
            string pwd = ConfigTextStream.ExtractValue(ConnectionString, "pwd");

            var jsonMapService = await TryPostAsync<JsonMapServiceDTO>($"{serviceUrl}?f=json");
            var jsonLayers = await TryPostAsync<JsonLayersDTO>($"{serviceUrl}/layers?f=json");

            if (jsonMapService != null)
            {
                _class.Name = jsonMapService.MapName;

                if (jsonMapService.FullExtent != null)
                {
                    _class.Envelope = new Envelope(
                        jsonMapService.FullExtent.XMin,
                        jsonMapService.FullExtent.YMin,
                        jsonMapService.FullExtent.XMax,
                        jsonMapService.FullExtent.YMax);
                }

                if (jsonMapService.SpatialReferenceInstance != null &&
                    jsonMapService.SpatialReferenceInstance.Wkid > 0)
                {
                    var sRef = gView.Framework.Geometry.SpatialReference.FromID("epsg:" + jsonMapService.SpatialReferenceInstance.Wkid);
                    this.SetSpatialReference(sRef);
                    _class.SpatialReference = sRef;
                }

                if (jsonLayers?.Layers != null)
                {
                    foreach (var jsonLayer in jsonLayers.Layers)
                    {
                        IClass themeClass = null;
                        IWebServiceTheme theme = null;

                        if (jsonLayer.Type.ToLower() == "feature layer")
                        {
                            themeClass = await GeoServicesFeatureClass.CreateAsync(this, jsonLayer);

                            theme = LayerFactory.Create(themeClass, _class as IWebServiceClass) as IWebServiceTheme;
                            if (theme == null)
                            {
                                continue;
                            }
                        }
                        // ToDo Raster classes

                        if (themeClass == null)
                        {
                            continue;
                        }

                        theme.Visible = true; //false;

                        _class.Themes.Add(theme);
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

        async internal Task<T> TryPostAsync<T>(string url, string postData = "")
        {
            int i = 0;
            while (true)
            {
                try
                {
                    string result, tokenParameter = String.Empty;

                    if (!String.IsNullOrWhiteSpace(_token))
                    {
                        tokenParameter = (String.IsNullOrWhiteSpace(postData) ? "" : "&") + "token=" + _token;
                    }

                    try
                    {
                        result = await _http.PostFormUrlEncodedStringAsync(url.UrlAppendParameters(tokenParameter),
                                                                              $"{postData}{tokenParameter}");

                        //result = Encoding.UTF8.GetString(await WebFunctions.DownloadRawAsync(url.UrlAppendParameters(tokenParameter), 
                        //                                                                     postData == null ? null : Encoding.UTF8.GetBytes(postData),
                        //                                                                     null, null, string.Empty, string.Empty));
                    }
                    catch (WebException ex)
                    {
                        if (ex.Message.Contains("(403)") ||
                                    ex.Message.Contains("(499)") ||
                                    ex.Message.Contains("(499)"))
                        {
                            throw new TokenRequiredException();
                        }
                        throw;
                    }

                    if (result.Contains("\"error\":"))
                    {
                        JsonErrorDTO error = JsonSerializer.Deserialize<JsonErrorDTO>(result);
                        if (error.Error == null)
                        {
                            throw new Exception("Unknown error");
                        }

                        if (error.Error.Code == 499 || error.Error.Code == 498 || error.Error.Code == 403) // Token Required (499), Invalid Token (498), No user Persmissions (403)
                        {
                            throw new TokenRequiredException();
                        }

                        throw new Exception("Error:" + error.Error.Code + "\n" + error.Error.Message);
                    }

                    return JSerializer.Deserialize<T>(result);
                }
                catch (TokenRequiredException ex)
                {
                    await HandleTokenExceptionAsync(i, ex);
                }
                i++;
            }
        }

        async private Task HandleTokenExceptionAsync(int i, TokenRequiredException ex)
        {
            if (i < 3)  // drei mal probieren lassen
            {
                string serviceUrl = ServiceUrl();
                string user = ConfigTextStream.ExtractValue(ConnectionString, "user");
                string pwd = ConfigTextStream.ExtractValue(ConnectionString, "pwd");

                _token = await RequestTokenCache.RefreshTokenAsync(serviceUrl, user, pwd, _token);
            }
            else
            {
                throw ex;
            }
        }

        public string ServiceUrl()
        {
            string server = ConfigTextStream.ExtractValue(ConnectionString, "server");
            string service = ConfigTextStream.ExtractValue(ConnectionString, "service");

            return $"{server.UrlRemoveEndingSlashes().UrlAppendPath(service)}/MapServer";
        }

        #endregion
    }
}

using gView.Data.Framework.Data;
using gView.Data.Framework.Data.Abstraction;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace gView.DataSources.VectorTileCache
{
    [gView.Framework.system.RegisterPlugIn("1B8F1096-8D3F-4D2B-9222-15F26BBEC550")]
    public class Dataset : DatasetMetadata, IDataset, IFeatureCacheDataset, IFeatureDataset
    {
        private string _connectionString;
        private string _dsName;
        private DatasetState _state = DatasetState.unknown;
        private IEnumerable<IDatasetElement> _dsElements = null;
        private Json.VectorTilesCapabilities _capabilities;
        
        internal static HttpClient _httpClient = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        #region IDataset

        public string ConnectionString => _connectionString;

        public Task<bool> SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;

            _dsName = ConfigTextStream.ExtractValue(_connectionString, "name");

            return Task.FromResult(true);
        }

        public string DatasetGroupName => "Vector Tile Cache";

        public string DatasetName => _dsName;

        public string ProviderName => "gView.GIS";

        public DatasetState State => _state;

        public string Query_FieldPrefix => String.Empty;

        public string Query_FieldPostfix => String.Empty;

        public IDatabase Database => null;

        public string LastErrorMessage { get; set; }

        public void Dispose()
        {
           
        }

        public Task<IDatasetElement> Element(string title)
        {
            if (_dsElements != null)
            {
                foreach (var element in _dsElements)
                {
                    if (element.Title == title)
                    {
                        return Task.FromResult(element);
                    }
                }
            }
            return Task.FromResult<IDatasetElement>(null);
        }

        public Task<List<IDatasetElement>> Elements()
        {
            var result = new List<IDatasetElement>();

            if (_dsElements != null)
            {
                result.AddRange(_dsElements);
            }

            return Task.FromResult(result);
        }

        async public Task<bool> Open()
        {
            SpatialReference = gView.Framework.Geometry.SpatialReference.FromID("epsg:4326");
            WebMercatorSpatialReference = gView.Framework.Geometry.SpatialReference.FromID("epsg:3857");

            await RefreshClasses();

            return true;
        }

        async public Task RefreshClasses()
        {
            string source = ConfigTextStream.ExtractValue(_connectionString, "source");

            using (var responseMesssage = await _httpClient.GetAsync(source))
            {
                var jsonString = await responseMesssage.Content.ReadAsStringAsync();

                _capabilities = JsonConvert.DeserializeObject<Json.VectorTilesCapabilities>(jsonString);

                if (String.IsNullOrEmpty(_dsName))
                {
                    _dsName = _capabilities.Name;
                }

                this.TileUrls = _capabilities.Tiles;

                var dsElements = new List<IDatasetElement>();
                if(_capabilities.VectorLayers!=null)
                {
                    foreach(var vectorLayer in _capabilities.VectorLayers)
                    {
                        dsElements.Add(new DatasetElement(new FeatureClass(this, vectorLayer.Id))
                        {
                            Title = vectorLayer.Id
                        });
                    }
                }

                _dsElements = dsElements.ToArray();
            }
        }

        #endregion

        #region IPersistable

        async public Task<bool> LoadAsync(IPersistStream stream)
        {
             await this.SetConnectionString((string)stream.Load("connectionstring", ""));

            return true;
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("connectionstring", _connectionString);
        }

        #endregion

        #region IFeatureCacheDataset

        async public Task<bool> InitFeatureCache(DatasetCachingContext cachingContext)
        {
            var map = cachingContext?.Map;

            var featureCache = cachingContext?.GetCache<FeatureCache>();
            if (featureCache?.Dataset == this)
            {
                return true;
            }

            if (map != null)
            {
                FeatureCache cache = new FeatureCache(this);
                await cache.LoadAsync(map.Display);
                cachingContext.AddCache(cache);
            }

            return true;
        }

        #endregion

        #region IFeatureDataset

        public Task<IEnvelope> Envelope()
        {
            if(_capabilities?.Bounds != null && _capabilities.Bounds.Length==4)
            {
                return Task.FromResult<IEnvelope>(new Envelope(
                    _capabilities.Bounds[0],
                    _capabilities.Bounds[1],
                    _capabilities.Bounds[2],
                    _capabilities.Bounds[3]));
            }

            return Task.FromResult<IEnvelope>(null);
        }

        public Task<ISpatialReference> GetSpatialReference()
        {
            return Task.FromResult(this.SpatialReference);
        }

        public void SetSpatialReference(ISpatialReference sRef)
        {
            
        }

        #endregion

        #region Properties

        public string[] TileUrls { get; private set; }

        internal Json.VectorTilesCapabilities Capabilities => _capabilities;

        internal ISpatialReference SpatialReference { get; private set; }

        internal ISpatialReference WebMercatorSpatialReference { get; private set; }

        #endregion
    }
}

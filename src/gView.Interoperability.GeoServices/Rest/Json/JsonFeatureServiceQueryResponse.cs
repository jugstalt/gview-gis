using gView.Interoperability.GeoServices.Rest.Json.Features;
using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonFeatureServiceQueryResponse
    {
        [JsonProperty("objectIdFieldName")]
        public string ObjectIdFieldName { get; set; }

        [JsonProperty("globalIdFieldName")]
        public string GlobalIdFieldName { get; set; }

        [JsonProperty("fieldAliases")]
        public dynamic FieldAliases { get; set; } // object

        [JsonProperty("geometryType")]
        public string GeometryType { get; set; }

        [JsonProperty("spatialReference")]
        public JsonSpatialReference SpatialReference { get; set; }

        [JsonProperty("fields")]
        public JsonFeatureResponse.Field[] Fields { get; set; }

        [JsonProperty("features")]
        public JsonFeature[] Features { get; set; }

        [JsonProperty("exceededTransferLimit")]
        public bool ExceededTransferLimit { get; set; }
    }
}

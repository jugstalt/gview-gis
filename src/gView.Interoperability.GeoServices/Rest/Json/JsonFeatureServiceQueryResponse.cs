using gView.Interoperability.GeoServices.Rest.Json.Features;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonFeatureServiceQueryResponse
    {
        [JsonPropertyName("objectIdFieldName")]
        public string ObjectIdFieldName { get; set; }

        [JsonPropertyName("globalIdFieldName")]
        public string GlobalIdFieldName { get; set; }

        [JsonPropertyName("fieldAliases")]
        public dynamic FieldAliases { get; set; } // object

        [JsonPropertyName("geometryType")]
        public string GeometryType { get; set; }

        [JsonPropertyName("spatialReference")]
        public JsonSpatialReference SpatialReference { get; set; }

        [JsonPropertyName("fields")]
        public JsonFeatureResponse.Field[] Fields { get; set; }

        [JsonPropertyName("features")]
        public JsonFeature[] Features { get; set; }

        [JsonPropertyName("exceededTransferLimit")]
        public bool ExceededTransferLimit { get; set; }
    }
}

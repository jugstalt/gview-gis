using gView.Interoperability.GeoServices.Rest.DTOs.Features;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    public class JsonFeatureServiceQueryResponseDTO
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
        public JsonSpatialReferenceDTO SpatialReference { get; set; }

        [JsonPropertyName("fields")]
        public JsonFeatureResponseDTO.Field[] Fields { get; set; }

        [JsonPropertyName("features")]
        public JsonFeatureDTO[] Features { get; set; }

        [JsonPropertyName("exceededTransferLimit")]
        public bool ExceededTransferLimit { get; set; }
    }
}

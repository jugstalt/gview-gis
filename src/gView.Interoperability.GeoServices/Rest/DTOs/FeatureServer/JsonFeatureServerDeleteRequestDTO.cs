using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.FeatureServer
{
    public class JsonFeatureServerDeleteRequestDTO : JsonFeatureServerEditRequesDTO
    {
        [JsonPropertyName("objectIds")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ObjectIds { get; set; }
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    internal class JsonExtentResponseDTO
    {
        [JsonPropertyName("extent")]
        public JsonExtentDTO Extend { get; set; }
    }
}

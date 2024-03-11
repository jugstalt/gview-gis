using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    internal class JsonExtentResponse
    {
        [JsonPropertyName("extent")]
        public JsonExtent Extend { get; set; }
    }
}

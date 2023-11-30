using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    internal class JsonExtentResponse
    {
        [JsonProperty("extent")]
        public JsonExtent Extend { get; set; }
    }
}

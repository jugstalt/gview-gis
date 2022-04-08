using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonError
    {
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public ErrorDef Error { get; set; }

        public class ErrorDef
        {
            [JsonProperty("code")]
            public int Code { get; set; }
            [JsonProperty("message")]
            public string Message { get; set; }
            [JsonProperty("details")]
            public object Details { get; set; }
        }
    }
}

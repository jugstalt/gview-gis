using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonError
    {
        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ErrorDef Error { get; set; }

        public class ErrorDef
        {
            [JsonPropertyName("code")]
            public int Code { get; set; }
            [JsonPropertyName("message")]
            public string Message { get; set; }
            [JsonPropertyName("details")]
            public object Details { get; set; }
        }
    }
}

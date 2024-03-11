using gView.Interoperability.GeoServices.Rest.Json.Features.Geometry;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Response
{
    public class JsonIdentifyResponse : JsonStopWatch
    {
        [JsonPropertyName("results")]
        public Result[] Results { get; set; }

        #region Classes

        public class Result
        {
            [JsonPropertyName("layerId")]
            public int LayerId { get; set; }

            [JsonPropertyName("layerName")]
            public string LayerName { get; set; }

            [JsonPropertyName("displayFieldName")]
            public string DisplayFieldName { get; set; }

            [JsonPropertyName("attributes")]
            public Dictionary<string, object> ResultAttributes { get; set; }

            [JsonPropertyName("geometry")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public JsonGeometry Geometry { get; set; }
        }

        #endregion
    }
}

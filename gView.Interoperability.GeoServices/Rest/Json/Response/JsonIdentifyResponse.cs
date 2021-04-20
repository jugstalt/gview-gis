using gView.Interoperability.GeoServices.Rest.Json.Features.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Response
{
    public class JsonIdentifyResponse : JsonStopWatch
    {
        [JsonProperty("results")]
        public Result[] Results { get; set; }

        #region Classes

        public class Result
        {
            [JsonProperty("layerId")]
            public int LayerId { get; set; }

            [JsonProperty("layerName")]
            public string LayerName { get; set; }

            [JsonProperty("displayFieldName")]
            public string DisplayFieldName { get; set; }

            [JsonProperty("attributes")]
            public Dictionary<string, object> ResultAttributes { get; set; }

            [JsonProperty("geometry", NullValueHandling = NullValueHandling.Ignore)]
            public JsonGeometry Geometry { get; set; }
        }

        #endregion
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Legend
{
    public class LegendResponse : JsonStopWatch
    {
        [JsonProperty("layers")]
        public Layer[] Layers { get; set; }
    }
}

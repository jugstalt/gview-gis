using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json.Legend
{
    public class LegendResponse
    {
        [JsonProperty("layers")]
        public Layer[] Layers { get; set; }
    }
}

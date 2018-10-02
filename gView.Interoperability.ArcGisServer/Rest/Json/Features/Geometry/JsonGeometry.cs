using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json.Features.Geometry
{
    public class JsonGeometry
    {
        // AREA
        [JsonProperty("rings", NullValueHandling = NullValueHandling.Ignore)]
        public double[][,] Rings { get; set; }

        // POLY LINE
        [JsonProperty("paths", NullValueHandling = NullValueHandling.Ignore)]
        public double[][,] Paths { get; set; } 

        // SINGLE POINT
        [JsonProperty("x", NullValueHandling = NullValueHandling.Ignore)]
        public double? X { get; set; } 
        [JsonProperty("y", NullValueHandling = NullValueHandling.Ignore)]
        public double? Y { get; set; } 

    }
}

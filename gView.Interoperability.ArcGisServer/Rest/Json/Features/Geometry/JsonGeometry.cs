using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json.Features.Geometry
{
    class JsonGeometry
    {
        // AREA
        [JsonProperty("rings")]
        public double[][,] Rings { get; set; } // JsonRings []

        // POLY LINE
         [JsonProperty("paths")]
        public double[][,] Paths { get; set; } // JsonPaths[]

        // SINGLE POINT
        [JsonProperty("x")]
         public dynamic X { get; set; } // JsonX
        [JsonProperty("y")]
        public dynamic Y { get; set; } // JsonY

    }
}

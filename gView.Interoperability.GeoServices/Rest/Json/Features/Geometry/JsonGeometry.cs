using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Features.Geometry
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

        // Envelope
        [JsonProperty("xmin", NullValueHandling = NullValueHandling.Ignore)]
        public double? XMin { get; set; }
        [JsonProperty("ymin", NullValueHandling = NullValueHandling.Ignore)]
        public double? YMin { get; set; }
        [JsonProperty("xmax", NullValueHandling = NullValueHandling.Ignore)]
        public double? XMax { get; set; }
        [JsonProperty("ymax", NullValueHandling = NullValueHandling.Ignore)]
        public double? YMax { get; set; }

        [JsonProperty("spatialReference", NullValueHandling = NullValueHandling.Ignore)]
        public JsonSpatialReference SpatialReference { get; set; }
    }
}

using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json.Features.Geometry
{
    public class JsonGeometry
    {
        // AREA
        [JsonProperty("rings", NullValueHandling = NullValueHandling.Ignore)]
        public double?[][,] Rings { get; set; }

        // POLY LINE
        [JsonProperty("paths", NullValueHandling = NullValueHandling.Ignore)]
        public double?[][,] Paths { get; set; }

        // SINGLE POINT
        [JsonProperty("x", NullValueHandling = NullValueHandling.Ignore)]
        public double? X { get; set; }
        [JsonProperty("y", NullValueHandling = NullValueHandling.Ignore)]
        public double? Y { get; set; }

        // MULTI POINT 
        [JsonProperty("points", NullValueHandling = NullValueHandling.Ignore)]
        public double?[][] Points { get; set; }

        // Envelope
        [JsonProperty("xmin", NullValueHandling = NullValueHandling.Ignore)]
        public double? XMin { get; set; }
        [JsonProperty("ymin", NullValueHandling = NullValueHandling.Ignore)]
        public double? YMin { get; set; }
        [JsonProperty("xmax", NullValueHandling = NullValueHandling.Ignore)]
        public double? XMax { get; set; }
        [JsonProperty("ymax", NullValueHandling = NullValueHandling.Ignore)]
        public double? YMax { get; set; }

        [JsonProperty(PropertyName = "hasZ", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasZ { get; set; }
        [JsonProperty(PropertyName = "hasM", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasM { get; set; }

        [JsonProperty(PropertyName = "z", NullValueHandling = NullValueHandling.Ignore)]
        public double? Z { get; set; }
        [JsonProperty(PropertyName = "m", NullValueHandling = NullValueHandling.Ignore)]
        public double? M { get; set; }

        [JsonProperty("spatialReference", NullValueHandling = NullValueHandling.Ignore)]
        public JsonSpatialReference SpatialReference { get; set; }
    }
}

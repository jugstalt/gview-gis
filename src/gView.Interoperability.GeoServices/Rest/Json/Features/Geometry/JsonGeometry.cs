using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Features.Geometry
{
    public class JsonGeometry
    {
        // AREA
        [JsonProperty("rings", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("rings")]
        public double?[][,] Rings { get; set; }

        // POLY LINE
        [JsonProperty("paths", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("paths")]
        public double?[][,] Paths { get; set; }

        // SINGLE POINT
        [JsonProperty("x", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("x")]
        public double? X { get; set; }
        [JsonProperty("y", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("y")]
        public double? Y { get; set; }

        // MULTI POINT 
        [JsonProperty("points", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("points")]
        public double?[][] Points { get; set; }

        // Envelope
        [JsonProperty("xmin", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("xmin")]
        public double? XMin { get; set; }
        [JsonProperty("ymin", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("ymin")]
        public double? YMin { get; set; }
        [JsonProperty("xmax", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("xmax")]
        public double? XMax { get; set; }
        [JsonProperty("ymax", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("ymax")]
        public double? YMax { get; set; }

        [JsonProperty(PropertyName = "hasZ", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("hasZ")]
        public bool? HasZ { get; set; }
        [JsonProperty(PropertyName = "hasM", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("hasM")]
        public bool? HasM { get; set; }

        [JsonProperty(PropertyName = "z", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("z")]
        public double? Z { get; set; }
        [JsonProperty(PropertyName = "m", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("m")]
        public double? M { get; set; }

        [JsonProperty("spatialReference", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("spatialReference")]
        public JsonSpatialReference SpatialReference { get; set; }
    }
}

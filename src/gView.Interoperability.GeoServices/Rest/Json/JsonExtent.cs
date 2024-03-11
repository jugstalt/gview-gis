using gView.Interoperability.GeoServices.Rest.Json.Features;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonExtent
    {
        [JsonPropertyName("xmin")]
        public double Xmin { get; set; }

        [JsonPropertyName("ymin")]
        public double Ymin { get; set; }

        [JsonPropertyName("xmax")]
        public double Xmax { get; set; }

        [JsonPropertyName("ymax")]
        public double Ymax { get; set; }

        [JsonPropertyName("spatialReference")]
        public JsonSpatialReference SpatialReference { get; set; }

        public bool IsInitialized()
        {
            return Xmin != 0D || Ymin != 0D || Xmax != 0D || Ymax != 0D;
        }
    }
}

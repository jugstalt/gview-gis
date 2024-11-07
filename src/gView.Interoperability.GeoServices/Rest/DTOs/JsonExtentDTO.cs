using gView.Interoperability.GeoServices.Rest.DTOs.Features;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    public class JsonExtentDTO
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
        public JsonSpatialReferenceDTO SpatialReference { get; set; }

        public bool IsInitialized()
        {
            return Xmin != 0D || Ymin != 0D || Xmax != 0D || Ymax != 0D;
        }
    }
}

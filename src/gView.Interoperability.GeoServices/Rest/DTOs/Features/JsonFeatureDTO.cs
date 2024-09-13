using gView.Interoperability.GeoServices.Rest.DTOs.Features.Geometry;
using System.Text.Json;
using System.Dynamic;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.Features
{
    public class JsonFeatureDTO
    {
        public JsonFeatureDTO()
        {
            this.Attributes = new ExpandoObject();
        }

        [JsonPropertyName("attributes")]
        public ExpandoObject Attributes { get; set; }
        //JsonAttributes attributes { get; set; }

        [JsonPropertyName("geometry")]
        public JsonGeometryDTO Geometry { get; set; }
    }
}

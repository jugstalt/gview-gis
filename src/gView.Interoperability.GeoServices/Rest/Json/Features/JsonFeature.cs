using gView.Interoperability.GeoServices.Rest.Json.Features.Geometry;
using System.Text.Json;
using System.Dynamic;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Features
{
    public class JsonFeature
    {
        public JsonFeature()
        {
            this.Attributes = new ExpandoObject();
        }

        [JsonPropertyName("attributes")]
        public ExpandoObject Attributes { get; set; }
        //JsonAttributes attributes { get; set; }

        [JsonPropertyName("geometry")]
        public JsonGeometry Geometry { get; set; }
    }
}

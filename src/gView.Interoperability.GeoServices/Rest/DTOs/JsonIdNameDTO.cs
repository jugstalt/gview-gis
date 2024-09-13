using gView.Framework.Common.Reflection;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    public class JsonIdNameDTO
    {
        [JsonPropertyName("id")]
        [HtmlLink("{url}/{0}")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    public class JsonObjectIdResponseDTO
    {
        [JsonPropertyName("objectIdFieldName")]
        public string ObjectIdFieldName { get; set; }

        [JsonPropertyName("objectIds")]
        public IEnumerable<int> ObjectIds { get; set; }
    }
}

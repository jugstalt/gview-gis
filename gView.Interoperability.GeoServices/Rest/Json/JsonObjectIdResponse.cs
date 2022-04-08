using Newtonsoft.Json;
using System.Collections.Generic;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonObjectIdResponse
    {
        [JsonProperty("objectIdFieldName")]
        public string ObjectIdFieldName { get; set; }

        [JsonProperty("objectIds")]
        public IEnumerable<int> ObjectIds { get; set; }
    }
}

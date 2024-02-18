using gView.Framework.Common.Reflection;
using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonIdName
    {
        [JsonProperty(PropertyName = "id")]
        [HtmlLinkAttribute("{url}/{0}")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}

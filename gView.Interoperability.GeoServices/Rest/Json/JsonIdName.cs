using gView.Framework.system;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonIdName
    {
        [JsonProperty(PropertyName = "id")]
        [HtmlLinkAttribute("{url}/{0}")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "parentLayerId")]
        public int ParentLayerId { get; set; }
    }
}

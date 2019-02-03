using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonFeatureCountResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}

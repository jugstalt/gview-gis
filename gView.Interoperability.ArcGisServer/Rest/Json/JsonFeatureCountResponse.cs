using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json
{
    class JsonFeatureCountResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}

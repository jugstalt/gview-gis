using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json.Features
{
    public class JsonSpatialReference
    {
        [JsonProperty("wkt")]
        public string Wkt { get; set; }

        [JsonProperty("wkid")]
        public int Wkid { get; set; }
    }
}

using gView.Interoperability.ArcGisServer.Rest.Json.Features.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json.Features
{
    class JsonFeatures
    {
        [JsonProperty("attributes")]
        public dynamic Attributes { get; set; }
        //JsonAttributes attributes { get; set; }
        [JsonProperty("geometry")]
        public JsonGeometry Geometry { get; set; }
    }
}

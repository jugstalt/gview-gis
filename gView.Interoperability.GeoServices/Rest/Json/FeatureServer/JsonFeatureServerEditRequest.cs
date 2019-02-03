using gView.Interoperability.GeoServices.Rest.Json.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.FeatureServer
{
    public class JsonFeatureServerEditRequest
    {
        [JsonProperty(PropertyName = "layerId")]
        public int LayerId { get; set; }

        [JsonProperty("features")]
        public JsonFeature[] Features { get; set; }

        [JsonProperty("objectIds", NullValueHandling = NullValueHandling.Ignore)]
        public string ObjectIds { get; set; }
    }
}

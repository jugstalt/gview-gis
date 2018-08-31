using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json.DynamicLayers
{
    class DynamicLayerDrawingInfo
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object renderer { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? transparency { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? scaleSymbols { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? showLabels { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LabelingInfo[] labelingInfo { get; set; }
    }
}

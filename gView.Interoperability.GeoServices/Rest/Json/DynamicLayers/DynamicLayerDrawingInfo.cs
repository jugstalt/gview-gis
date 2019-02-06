using gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.DynamicLayers
{
    public class DynamicLayerDrawingInfo
    {
        [JsonProperty("renderer", NullValueHandling = NullValueHandling.Ignore)]
        public JsonRenderer Renderer { get; set; }

        [JsonProperty("transparency", NullValueHandling = NullValueHandling.Ignore)]
        public int? Transparency { get; set; }

        [JsonProperty("scaleSymbols", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ScaleSymbols { get; set; }

        [JsonProperty("showLabels", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowLabels { get; set; }

        [JsonProperty("labelingInfo", NullValueHandling = NullValueHandling.Ignore)]
        public LabelingInfo[] LabelingInfo { get; set; }
    }
}

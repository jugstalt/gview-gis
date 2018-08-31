using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json.DynamicLayers
{
    class DynamicLayer
    {
        public int id { get; set; }
        public DynamicLayerSouce source { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string definitionExpression { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DynamicLayerDrawingInfo drawingInfo { get; set; }
    }
}

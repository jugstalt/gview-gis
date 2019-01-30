using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Legend
{
    public class Layer
    {
        [JsonProperty("layerId")]
        public int LayerId { get; set; }

        [JsonProperty("layerName")]
        public string LayerName { get; set; }

        [JsonProperty("layerType")]
        public string LayerType { get; set; }

        [JsonProperty("minScale")]
        public int MinScale { get; set; }

        [JsonProperty("maxScale")]
        public int MaxScale { get; set; }

        [JsonProperty("legend")]
        public Legend[] Legend { get; set; }
    }
}

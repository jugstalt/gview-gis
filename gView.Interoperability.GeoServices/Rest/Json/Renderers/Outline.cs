using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Renderers
{
    public class Outline
    {        
        [JsonProperty("type")]
        public string Type { get; set; } // optional - one use is a Simple Fill Symbol Renderer

        [JsonProperty("style")]
        public string Style { get; set; } // optional - one use is a Simple Fill Symbol Renderer

        [JsonProperty("color")]
        public int[] Color { get; set; }

        [JsonProperty("width")]
        public float Width { get; set; }
    }
}

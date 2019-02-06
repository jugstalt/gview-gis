using gView.Framework.Carto;
using gView.Framework.Carto.Rendering;
using gView.Interoperability.GeoServices.Rest.Json.DynamicLayers;
using gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonDrawingInfo
    {
        [JsonProperty("renderer")]
        public JsonRenderer Renderer
        {
            get;set;
        }

        [JsonProperty("transparency")]
        public double Transparency { get; set; }

        [JsonProperty("labelingInfo")]
        public LabelingInfo[] LabelingInfo { get; set; }  
    }
}

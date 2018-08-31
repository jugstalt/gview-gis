using gView.Interoperability.ArcGisServer.Rest.Json.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json
{
    public class JsonExtent
    {
        [JsonProperty("xmin")]
        public double Xmin { get; set; }

        [JsonProperty("ymin")]
        public double Ymin { get; set; }

        [JsonProperty("xmax")]
        public double Xmax { get; set; }

        [JsonProperty("ymax")]
        public double Ymax { get; set; }

        [JsonProperty("spatialReference")]
        public JsonSpatialReference spatialReference { get; set; }

        public bool IsInitialized()
        {
            return Xmin != 0D || Ymin != 0D || Xmax != 0D || Ymax != 0D;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json.DynamicLayers
{
    class DynamicLayerSouce
    {
        public DynamicLayerSouce()
        {
            this.type = "mapLayer";
        }
        public string type { get; set; }

        public int mapLayerId { get; set; }
    }
}

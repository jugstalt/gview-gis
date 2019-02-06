using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers
{
    public class SimpleFillSymbol
    {
        public SimpleFillSymbol()
        {
            this.Type = "esriSFS";
        }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("style")]
        public string Style { get; set; }

        [JsonProperty("color")]
        public int[] Color { get; set; }

        [JsonProperty("outline")]
        public SimpleLineSymbol Outline { get; set; }
    }

    /*
    Simple Fill Symbol
    Simple fill symbols can be used to symbolize polygon geometries. The type property for simple fill symbols is esriSFS.
    
    JSON Syntax
    {
    "type" : "esriSFS",
    "style" : "< esriSFSBackwardDiagonal | esriSFSCross | esriSFSDiagonalCross | esriSFSForwardDiagonal | esriSFSHorizontal | esriSFSNull | esriSFSSolid | esriSFSVertical >",
    "color" : <color>,
    "outline" : <simpleLineSymbol> //if outline has been specified
    }

    JSON Example
    {
      "type": "esriSFS",
      "style": "esriSFSSolid",
      "color": [115,76,0,255],
        "outline": {
         "type": "esriSLS",
         "style": "esriSLSSolid",
         "color": [110,110,110,255],
         "width": 1
	     }
    }
    */
}

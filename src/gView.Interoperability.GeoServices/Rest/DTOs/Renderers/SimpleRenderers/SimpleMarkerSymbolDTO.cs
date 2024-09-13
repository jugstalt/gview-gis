using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.Renderers.SimpleRenderers
{
    public class SimpleMarkerSymbolDTO
    {
        public SimpleMarkerSymbolDTO()
        {
            this.Type = "esriSMS";
        }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("style")]
        public string Style { get; set; }

        [JsonPropertyName("color")]
        public int[] Color { get; set; }

        [JsonPropertyName("size")]
        public float Size { get; set; }

        [JsonPropertyName("angle")]
        public float Angle { get; set; }

        [JsonPropertyName("xoffset")]
        public float Xoffset { get; set; }

        [JsonPropertyName("yoffset")]
        public float Yoffset { get; set; }


        [JsonPropertyName("outline")]
        public SimpleLineSymbolDTO Outline { get; set; }
    }

    /*
    Simple Marker Symbol
    Simple marker symbols can be used to symbolize point geometries. The type property for simple marker symbols is esriSMS. The angle property defines the number of degrees (0 to 360) that a marker symbol is rotated. The rotation is from East in a counter-clockwise direction where East is the 0° axis.
    New in 10.1

    Support for esriSMSTriangle was added.

    JSON Syntax
    {
    "type" : "esriSMS",
    "style" : "< esriSMSCircle | esriSMSCross | esriSMSDiamond | esriSMSSquare | esriSMSX | esriSMSTriangle >",
    "color" : <color>,
    "size" : <size>,
    "angle" : <angle>,
    "xoffset" : <xoffset>,
    "yoffset" : <yoffset>,
    "outline" : { //if outline has been specified
      "color" : <color>,
      "width" : <width>
    }
    }

    JSON Example
    {
    "type": "esriSMS",
     "style": "esriSMSSquare",
     "color": [76,115,0,255],
     "size": 8,
     "angle": 0,
     "xoffset": 0,
     "yoffset": 0,
     "outline": 
      {
      "color": [152,230,0,255],
       "width": 1
      }
    }    
    */

}


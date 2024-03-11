using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers
{
    public class SimpleLineSymbol
    {
        public SimpleLineSymbol()
        {
            this.Type = "esriSLS";
        }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("style")]
        public string Style { get; set; }

        [JsonPropertyName("color")]
        public int[] Color { get; set; }

        [JsonPropertyName("width")]
        public float Width { get; set; }
    }

    /*
    Simple Line Symbol
    Simple line symbols can be used to symbolize polyline geometries or outlines for polygon fills. The type property for simple line symbols is esriSLS.
    
     * JSON Syntax
    {
    "type" : "esriSLS",
    "style" : "< esriSLSDash | esriSLSDashDot | esriSLSDashDotDot | esriSLSDot | esriSLSNull | esriSLSSolid >",
    "color" : <color>,
    "width" : <width>
    }

    JSON Example
    {
    "type": "esriSLS",
    "style": "esriSLSDot",
    "color": [115,76,0,255],
    "width": 1
    }
    */
}

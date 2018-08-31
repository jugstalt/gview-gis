using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json
{
    class JsonExportResponse
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("imageData")]
        public string ImageData { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("extent")]
        public JsonExtent Extent { get; set; }

        [JsonProperty("scale")]
        public double Scale { get; set; }
    }
}

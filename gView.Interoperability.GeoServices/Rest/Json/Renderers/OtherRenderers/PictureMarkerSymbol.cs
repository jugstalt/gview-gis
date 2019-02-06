using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Renderers.OtherRenderers
{
    class PictureMarkerSymbol
    {
        public string Type { get; set; }
        public string Url { get; set; }
        public string ImageData { get; set; }
        public string ContentType { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public float Angle { get; set; }
        public float Xoffset { get; set; }
        public float Yoffset { get; set; }
    }
}

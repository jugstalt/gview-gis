using gView.Interoperability.GeoServices.Rest.Json.Renderers;
using gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Rest.Renderers.OtherRenderers
{
    class PictureFillSymbol
    {
        public string Type { get; set; }
        public string Url { get; set; }
        public string ImageData { get; set; }
        public string ContentType { get; set; }
        public SimpleLineSymbol Outline { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Angle { get; set; }
        public float Xoffset { get; set; }
        public float Yoffset { get; set; }
        public float Xscale { get; set; }
        public float Yscale { get; set; }
    }
}

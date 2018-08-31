using gView.Interoperability.ArcGisServer.Rest.Json.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json.Rest.Renderers.OtherRenderers
{
    class PictureFillSymbol
    {
        public string Type { get; set; }
        public string Url { get; set; }
        public string ImageData { get; set; }
        public string ContentType { get; set; }
        public Outline Outline { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Angle { get; set; }
        public int Xoffset { get; set; }
        public int Yoffset { get; set; }
        public int Xscale { get; set; }
        public int Yscale { get; set; }
    }
}

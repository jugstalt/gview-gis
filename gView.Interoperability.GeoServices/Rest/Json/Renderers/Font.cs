using gView.Framework.Carto.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Renderers
{
    class Font
    {
        [JsonProperty("family")]
        public string Family { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("style")]
        public string Style { get; set; }

        [JsonProperty("weight")]
        public string Weight { get; set; }

        [JsonProperty("decoration")]
        public string Decoration { get; set; }

        //public static string FontStyle(SimpleLabelRenderer.LabelStyleEnum style)
        //{
        //    //"<italic | normal | oblique>"

        //    switch (style)
        //    {
        //        case LabelRenderer.LabelStyleEnum.italic:
        //        case LabelRenderer.LabelStyleEnum.bolditalic:
        //            return "italic";
        //        /*case Renderer.LabelRenderer.LabelStyleEnum.outline:
        //            return "oblique";*/
        //    }

        //    return "normal";
        //}

        //public static string FontWeight(LabelRenderer.LabelStyleEnum style)
        //{
        //    //"<bold | bolder | lighter | normal>"

        //    switch (style)
        //    {
        //        case LabelRenderer.LabelStyleEnum.bold:
        //        case LabelRenderer.LabelStyleEnum.bolditalic:
        //            return "bold";
        //    }

        //    return "normal";
        //}

        //public static string FontDecoration(LabelRenderer.LabelStyleEnum style)
        //{
        //    //"<line-through | underline | none>"

        //    switch (style)
        //    {
        //        case LabelRenderer.LabelStyleEnum.underline:
        //            return "underline";
        //    }

        //    return "none";
        //}
    }
}

using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.Renderers
{
    public class FontDTO
    {
        [JsonPropertyName("family")]
        public string Family { get; set; }

        [JsonPropertyName("size")]
        public float Size { get; set; }

        [JsonPropertyName("style")]
        public string Style { get; set; }

        [JsonPropertyName("weight")]
        public string Weight { get; set; }

        [JsonPropertyName("decoration")]
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

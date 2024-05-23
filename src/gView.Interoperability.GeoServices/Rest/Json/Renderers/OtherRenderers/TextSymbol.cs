using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Renderers.OtherRenderers
{
    public class TextSymbol
    {
        public TextSymbol()
        {
            this.Type = "esriTS";
        }
        //public TextSymbol(LabelRenderer labelRenderer)
        //    : this()
        //{
        //    if (labelRenderer == null)
        //        return;

        //    SetHalo(labelRenderer);
        //    this.Color = new int[] { labelRenderer.FontColor.R, labelRenderer.FontColor.G, labelRenderer.FontColor.B, 255 };
        //    this.Font = new Renderers.Font()
        //                                {
        //                                    Family = "Arial",
        //                                    Size = (int)labelRenderer.FontSize,
        //                                    Style = Renderers.Font.FontStyle(labelRenderer.LabelStyle),
        //                                    Weight = Renderers.Font.FontWeight(labelRenderer.LabelStyle),
        //                                    Decoration = Renderers.Font.FontDecoration(labelRenderer.LabelStyle)
        //                                };
        //}
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("color")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int[] Color { get; set; }

        [JsonPropertyName("backgroundColor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int[] BackgroundColor { get; set; }

        [JsonPropertyName("borderLineSize")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? BorderLineSize { get; set; }

        [JsonPropertyName("borderLineColor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int[] BorderLineColor { get; set; }

        [JsonPropertyName("haloSize")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? HaloSize { get; set; }

        [JsonPropertyName("haloColor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int[] HaloColor { get; set; }

        [JsonPropertyName("verticalAlignment")]
        public string VerticalAlignment { get; set; }

        [JsonPropertyName("horizontalAlignment")]
        public string HorizontalAlignment { get; set; }

        [JsonPropertyName("rightToLeft")]
        public bool RightToLeft { get; set; }

        [JsonPropertyName("angle")]
        public float Angle { get; set; }

        [JsonPropertyName("xoffset")]
        public float Xoffset { get; set; }

        [JsonPropertyName("yoffset")]
        public float Yoffset { get; set; }

        [JsonPropertyName("kerning")]
        public bool Kerning { get; set; }

        [JsonPropertyName("font")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Font Font { get; set; }

        //public void SetHalo(LabelRenderer renderer)
        //{
        //    if (renderer == null || renderer.LabelBorderStyle == LabelRenderer.LabelBorderStyleEnum.none)
        //        return;

        //    switch (renderer.LabelBorderStyle)
        //    {
        //        case LabelRenderer.LabelBorderStyleEnum.blockout:
        //            this.BackgroundColor = new int[] { renderer.BorderColor.R, renderer.BorderColor.G, renderer.BorderColor.B, 255 };
        //            break;
        //        case LabelRenderer.LabelBorderStyleEnum.glowing:
        //            this.HaloColor = new int[] { renderer.BorderColor.R, renderer.BorderColor.G, renderer.BorderColor.B, 255 };
        //            this.HaloSize = 3;
        //            break;
        //        case LabelRenderer.LabelBorderStyleEnum.shadow:
        //            // ???
        //            break;
        //    }

        //    /*
        //    if (renderer.LabelStyle == Renderer.LabelRenderer.LabelStyleEnum.outline)
        //    {
        //        this.BorderLineColor = new int[] { renderer.BorderColor.R, renderer.BorderColor.G, renderer.BorderColor.B, 255 };
        //        this.BorderLineSize = 10;
        //    }
        //     * */
        //}
    }

    /*
     * 

{
  "type" : "esriTS",
  "color" : <color>,
  "backgroundColor" : <color>,
  "borderLineColor" : <color>,
  "verticalAlignment" : "<baseline | top | middle | bottom>",
  "horizontalAlignment" : "<left | right | center | justify>",
  "rightToLeft" : <true | false>,
  "angle" : <angle>,
  "xoffset" : <xoffset>,
  "yoffset" : <yoffset>,
  "kerning" : <true | false>,
  "font" : {
    "family" : "<fontFamily>",
    "size" : <fontSize>,
    "style" : "<italic | normal | oblique>",
    "weight" : "<bold | bolder | lighter | normal>",
    "decoration" : "<line-through | underline | none>"
  }
}
     * 
     * Example
     * 
{
     "type": "esriTS",
     "color": [78,78,78,255],
     "backgroundColor": null,
     "borderLineColor": null,
     "verticalAlignment": "bottom",
     "horizontalAlignment": "left",
     "rightToLeft": false,
     "angle": 0,
     "xoffset": 0,
     "yoffset": 0,
     "font": {
      "family": "Arial",
      "size": 12,
      "style": "normal",
      "weight": "bold",
      "decoration": "none"
	}
}
     * 
     * */
}

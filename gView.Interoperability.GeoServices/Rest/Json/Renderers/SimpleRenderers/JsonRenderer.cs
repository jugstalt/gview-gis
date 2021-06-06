using gView.Framework.Carto;
using gView.Framework.Carto.Rendering;
using gView.Interoperability.GeoServices.Rest.Json.Renderers.OtherRenderers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers
{
    public class JsonRenderer
    {
        public JsonRenderer()
        {
        }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
        public object Symbol { get; set; }

        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        #region UniqueValue Renderer

        [JsonProperty(PropertyName = "field1", NullValueHandling = NullValueHandling.Ignore)]
        public string Field1 { get; set; }
        [JsonProperty(PropertyName = "field2", NullValueHandling = NullValueHandling.Ignore)]
        public string Field2 { get; set; }
        [JsonProperty(PropertyName = "field3", NullValueHandling = NullValueHandling.Ignore)]
        public string Field3 { get; set; }

        [JsonProperty(PropertyName = "fieldDelimiter", NullValueHandling = NullValueHandling.Ignore)]
        public string FieldDelimiter { get; set; }

        [JsonProperty(PropertyName = "defaultSymbol", NullValueHandling = NullValueHandling.Ignore)]
        public object DefaultSymbol { get; set; }

        [JsonProperty(PropertyName = "uniqueValueInfos", NullValueHandling = NullValueHandling.Ignore)]
        public object[] UniqueValueInfos { get; set; }

        public class UniqueValueInfo
        {
            [JsonProperty(PropertyName = "symbol", NullValueHandling = NullValueHandling.Ignore)]
            public object Symbol { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }
        }

        #endregion

        #region Static Members

        static public JsonRenderer FromFeatureRenderer(IFeatureRenderer featureRenderer)
        {
            if (featureRenderer is SimpleRenderer)
            {
                var simpleRenderer = (SimpleRenderer)featureRenderer;
                return new JsonRenderer()
                {
                    Type = "simple",
                    Symbol = FromSymbol(simpleRenderer.Symbol)
                };
            }

            if (featureRenderer is ValueMapRenderer)
            {
                var valueMapRenderer = (ValueMapRenderer)featureRenderer;

                if (valueMapRenderer.Keys != null)
                {
                    var keys = new string[valueMapRenderer.Keys.Count];
                    valueMapRenderer.Keys.CopyTo(keys, 0);
                    var index = 0;
                    return new JsonRenderer()
                    {
                        Type = "uniqueValue",
                        Field1 = valueMapRenderer.ValueField,
                        FieldDelimiter = ", ",
                        DefaultSymbol = FromSymbol(valueMapRenderer.DefaultSymbol),
                        UniqueValueInfos = keys.Select(k =>
                        {
                            return new UniqueValueInfo()
                            {
                                Value = k,
                                Label = valueMapRenderer.LegendItem(index++)?.LegendLabel,
                                Symbol = FromSymbol(valueMapRenderer[k]),
                            };
                        }).ToArray()
                    };
                }
            }

            if (featureRenderer is UniversalGeometryRenderer)
            {
                var univeralRenderer = (UniversalGeometryRenderer)featureRenderer;
                if (univeralRenderer.Symbols != null && univeralRenderer.Symbols.Count() == 3)
                {
                    // take first
                    if (univeralRenderer.UsePointSymbol)
                    {
                        return new JsonRenderer()
                        {
                            Type = "simple",
                            Symbol = FromSymbol(univeralRenderer.Symbols[0])
                        };
                    }
                    else if (univeralRenderer.UseLineSymbol)
                    {
                        return new JsonRenderer()
                        {
                            Type = "simple",
                            Symbol = FromSymbol(univeralRenderer.Symbols[1])
                        };
                    }
                    else if (univeralRenderer.UsePolygonSymbol)
                    {
                        return new JsonRenderer()
                        {
                            Type = "simple",
                            Symbol = FromSymbol(univeralRenderer.Symbols[2])
                        };
                    }
                }
            }

            return null;
        }

        static public object FromSymbol(Framework.Symbology.ISymbol symbol)
        {
            try
            {
                if (symbol is Framework.Symbology.SimplePointSymbol)
                {
                    var spSymbol = (Framework.Symbology.SimplePointSymbol)symbol;

                    return new SimpleMarkerSymbol()
                    {
                        Style = spSymbol.Marker.FromMarkerType(),
                        Color = spSymbol.Color.ToArray(),
                        Size = spSymbol.Size,
                        Angle = spSymbol.Angle,
                        Xoffset = spSymbol.HorizontalOffset,
                        Yoffset = spSymbol.VerticalOffset,
                        Outline = new SimpleLineSymbol()
                        {
                            Color = spSymbol.OutlineColor.ToArray(),
                            Width = spSymbol.OutlineWidth
                        }
                    };
                }

                if (symbol is Framework.Symbology.TrueTypeMarkerSymbol)
                {
                    var ttmSymbol = (Framework.Symbology.TrueTypeMarkerSymbol)symbol;

                    return new TextSymbol()
                    {
                        Color = ttmSymbol.FontColor.ToArray(),
                        Angle = ttmSymbol.Angle,
                        Xoffset = ttmSymbol.HorizontalOffset,
                        Yoffset = ttmSymbol.VerticalOffset,
                        Font = ttmSymbol.Font != null ?
                        new Font()
                        {
                            Family = ttmSymbol.Font.Name,
                            Size = (int)ttmSymbol.Font.Size,
                            Style = ttmSymbol.Font.Style.ToString()
                        } :
                        null
                    };
                }

                if (symbol is Framework.Symbology.SimpleLineSymbol)
                {
                    var lineSymbol = (Framework.Symbology.SimpleLineSymbol)symbol;

                    return new SimpleLineSymbol()
                    {
                        Color = lineSymbol.Color.ToArray(),
                        Style = lineSymbol.DashStyle.FromDashStyle(),
                        Width = lineSymbol.Width
                    };
                }

                if (symbol is Framework.Symbology.SimpleFillSymbol)
                {
                    var fillSymbol = (Framework.Symbology.SimpleFillSymbol)symbol;

                    return new SimpleFillSymbol()
                    {
                        Style = "esriSFSSolid",
                        Color = fillSymbol.FillColor.ToArray(),
                        Outline = JsonRenderer.FromSymbol(fillSymbol.OutlineSymbol) as SimpleLineSymbol
                    };
                }

                if(symbol is Framework.Symbology.HatchSymbol)
                {
                    var hatchSymbol = (Framework.Symbology.HatchSymbol)symbol;
                    return new SimpleFillSymbol()
                    {
                        Style = "esriSFSSolid",
                        Color = hatchSymbol.FillColor.ToArray(),
                        Outline = JsonRenderer.FromSymbol(hatchSymbol.OutlineSymbol) as SimpleLineSymbol
                    };
                }
            }
            catch { }

            return null;
        }

        static public IFeatureRenderer FromJsonRenderer(JsonRenderer jsonRenderer)
        {
            if (jsonRenderer.Type?.ToLower() == "simple")
            {
                var featureRenderer = new SimpleRenderer();
                featureRenderer.Symbol = FromSymbolJObject(jsonRenderer.Symbol as JObject);

                return featureRenderer;
            }

            return null;
        }

        static public Framework.Symbology.ISymbol FromSymbolJObject(JObject jObject)
        {
            if (jObject == null)
                return null;

            var type = jObject.GetValue("type").Value<string>();

            if (type == "esriSMS")
            {
                var sms = JsonConvert.DeserializeObject<SimpleMarkerSymbol>(jObject.ToString());

                var symbol = new Framework.Symbology.SimplePointSymbol();
                symbol.Marker = sms.Style.ToMarkerType();
                symbol.Color = sms.Color.ToColor();
                symbol.Size = sms.Size;
                symbol.Angle = sms.Angle;
                symbol.HorizontalOffset = sms.Xoffset;
                symbol.VerticalOffset = sms.Yoffset;

                if (sms.Outline != null)
                {
                    symbol.OutlineColor = sms.Outline.Color.ToColor();
                    symbol.OutlineWidth = sms.Outline.Width;
                    symbol.SymbolSmothingMode = Framework.Symbology.SymbolSmoothing.AntiAlias;
                }

                return symbol;
            }

            if (type == "esriSLS")
            {
                var sls = JsonConvert.DeserializeObject<SimpleLineSymbol>(jObject.ToString());

                var symbol = new Framework.Symbology.SimpleLineSymbol();
                symbol.Color = sls.Color.ToColor();
                symbol.DashStyle = sls.Style.ToDashStyle();
                symbol.Width = sls.Width;
                symbol.SymbolSmothingMode = Framework.Symbology.SymbolSmoothing.AntiAlias;
                symbol.LineEndCap = symbol.LineStartCap = System.Drawing.Drawing2D.LineCap.Round;

                return symbol;
            }

            if (type == "esriSFS")
            {
                var sfs = JsonConvert.DeserializeObject<SimpleFillSymbol>(jObject.ToString());

                var symbol = new Framework.Symbology.SimpleFillSymbol();
                symbol.FillColor = sfs.Color.ToColor();
                if (sfs.Outline != null)
                {
                    symbol.OutlineSymbol = new Framework.Symbology.SimpleLineSymbol()
                    {
                        Color = sfs.Outline.Color.ToColor(),
                        Width = sfs.Outline.Width,
                        DashStyle = sfs.Outline.Style.ToDashStyle(),
                        SymbolSmothingMode = Framework.Symbology.SymbolSmoothing.AntiAlias,
                        LineStartCap = System.Drawing.Drawing2D.LineCap.Round,
                        LineEndCap = System.Drawing.Drawing2D.LineCap.Round
                    };
                }

                return symbol;
            }

            if (type == "esriTS")
            {
                var ts = JsonConvert.DeserializeObject<TextSymbol>(jObject.ToString());

                if (ts.Font != null)
                {
                    Framework.Symbology.SimpleTextSymbol symbol = null;
                    if (ts.HaloSize.HasValue && ts.HaloSize.Value > 0)
                    {
                        symbol = new Framework.Symbology.GlowingTextSymbol();
                        ((Framework.Symbology.GlowingTextSymbol)symbol).GlowingColor = ts.HaloColor.ToColor();
                    }
                    else
                    {
                        symbol = new Framework.Symbology.SimpleTextSymbol();
                    }

                    symbol.FontColor = ts.Color.ToColor();
                    symbol.Angle = ts.Angle;
                    symbol.VerticalOffset = ts.Yoffset;
                    symbol.HorizontalOffset = ts.Xoffset;
                    switch (ts.VerticalAlignment + " " + ts.HorizontalAlignment)
                    {
                        case "top left":
                            symbol.TextSymbolAlignment = Framework.Symbology.TextSymbolAlignment.leftAlignOver;
                            break;
                        case "middle left":
                            symbol.TextSymbolAlignment = Framework.Symbology.TextSymbolAlignment.leftAlignCenter;
                            break;
                        case "bottom left":
                            symbol.TextSymbolAlignment = Framework.Symbology.TextSymbolAlignment.leftAlignUnder;
                            break;
                        case "top center":
                            symbol.TextSymbolAlignment = Framework.Symbology.TextSymbolAlignment.Over;
                            break;
                        case "middle center":
                            symbol.TextSymbolAlignment = Framework.Symbology.TextSymbolAlignment.Center;
                            break;
                        case "bottom center":
                            symbol.TextSymbolAlignment = Framework.Symbology.TextSymbolAlignment.Under;
                            break;
                        case "top right":
                            symbol.TextSymbolAlignment = Framework.Symbology.TextSymbolAlignment.rightAlignOver;
                            break;
                        case "middle right":
                            symbol.TextSymbolAlignment = Framework.Symbology.TextSymbolAlignment.rightAlignCenter;
                            break;
                        case "bottom right":
                            symbol.TextSymbolAlignment = Framework.Symbology.TextSymbolAlignment.rightAlignUnder;
                            break;
                        default:
                            symbol.TextSymbolAlignment = Framework.Symbology.TextSymbolAlignment.Center;
                            break;
                    }
                    symbol.SymbolSmothingMode = Framework.Symbology.SymbolSmoothing.AntiAlias;

                    var fontStyle = System.Drawing.FontStyle.Regular;
                    if (ts.Font.Weight == "bold" || ts.Font.Weight == "bolder")
                        fontStyle |= System.Drawing.FontStyle.Bold;

                    if (ts.Font.Style == "italic")
                        fontStyle |= System.Drawing.FontStyle.Italic;

                    if (ts.Font.Decoration == "underline")
                        fontStyle |= System.Drawing.FontStyle.Underline;

                    symbol.Font = new System.Drawing.Font(ts.Font.Family, ts.Font.Size, fontStyle);
                    //symbol.Font.Style = ts.Font.Style;
                    //if(ts.Font.Weight)

                    return symbol;
                }
            }

            return null;
        }

        #endregion
    }
}

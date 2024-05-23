using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Symbology;
using gView.GraphicsEngine;
using gView.Interoperability.GeoServices.Rest.Json.Renderers.OtherRenderers;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers
{
    public class JsonRenderer
    {
        public JsonRenderer()
        {
        }

        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Type { get; set; }

        [JsonPropertyName("symbol")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Symbol { get; set; }

        [JsonPropertyName("label")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Label { get; set; }

        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Description { get; set; }

        #region UniqueValue Renderer

        [JsonPropertyName("field1")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Field1 { get; set; }
        [JsonPropertyName("field2")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Field2 { get; set; }
        [JsonPropertyName("field3")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Field3 { get; set; }

        [JsonPropertyName("fieldDelimiter")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string FieldDelimiter { get; set; }

        [JsonPropertyName("defaultSymbol")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object DefaultSymbol { get; set; }

        [JsonPropertyName("uniqueValueInfos")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object[] UniqueValueInfos { get; set; }

        public class UniqueValueInfo
        {
            [JsonPropertyName("symbol")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public object Symbol { get; set; }

            [JsonPropertyName("value")]
            public string Value { get; set; }

            [JsonPropertyName("label")]
            public string Label { get; set; }

            [JsonPropertyName("description")]
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
                    var keys = valueMapRenderer.Keys;
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

        static public object FromSymbol(ISymbol symbol)
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

                if (symbol is Framework.Symbology.HatchSymbol)
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

                featureRenderer.Symbol = jsonRenderer.Symbol switch
                {
                    JsonElement jsonElement => FromSymbolJObject(jsonElement),
                    _ => null
                };

                return featureRenderer;
            }

            return null;
        }

        static public ISymbol FromSymbolJObject(JsonElement jElement)
        {
            if(!jElement.TryGetProperty("type", out var typeElement))
            {
                return null;
            }

            try
            {
                var type = typeElement.GetString();

                if (type == "esriSMS")
                {
                    var sms = JsonSerializer.Deserialize<SimpleMarkerSymbol>(jElement.ToString());

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
                        symbol.SymbolSmoothingMode = SymbolSmoothing.AntiAlias;
                    }

                    return symbol;
                }

                if (type == "esriSLS")
                {
                    var sls = JsonSerializer.Deserialize<SimpleLineSymbol>(jElement.ToString());

                    var symbol = new Framework.Symbology.SimpleLineSymbol();
                    symbol.Color = sls.Color.ToColor();
                    symbol.DashStyle = sls.Style.ToDashStyle();
                    symbol.Width = sls.Width;
                    symbol.SymbolSmoothingMode = SymbolSmoothing.AntiAlias;
                    symbol.LineEndCap = symbol.LineStartCap = LineCap.Round;

                    return symbol;
                }

                if (type == "esriSFS")
                {
                    var sfs = JsonSerializer.Deserialize<SimpleFillSymbol>(jElement.ToString());

                    var symbol = new Framework.Symbology.SimpleFillSymbol();
                    symbol.FillColor = sfs.Color.ToColor();
                    if (sfs.Outline != null)
                    {
                        symbol.OutlineSymbol = new Framework.Symbology.SimpleLineSymbol()
                        {
                            Color = sfs.Outline.Color.ToColor(),
                            Width = sfs.Outline.Width,
                            DashStyle = sfs.Outline.Style.ToDashStyle(),
                            SymbolSmoothingMode = SymbolSmoothing.AntiAlias,
                            LineStartCap = LineCap.Round,
                            LineEndCap = LineCap.Round
                        };
                    }

                    return symbol;
                }

                if (type == "esriTS")
                {
                    var ts = JsonSerializer.Deserialize<TextSymbol>(jElement.ToString());

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
                                symbol.TextSymbolAlignment = TextSymbolAlignment.leftAlignOver;
                                break;
                            case "middle left":
                                symbol.TextSymbolAlignment = TextSymbolAlignment.leftAlignCenter;
                                break;
                            case "bottom left":
                                symbol.TextSymbolAlignment = TextSymbolAlignment.leftAlignUnder;
                                break;
                            case "top center":
                                symbol.TextSymbolAlignment = TextSymbolAlignment.Over;
                                break;
                            case "middle center":
                                symbol.TextSymbolAlignment = TextSymbolAlignment.Center;
                                break;
                            case "bottom center":
                                symbol.TextSymbolAlignment = TextSymbolAlignment.Under;
                                break;
                            case "top right":
                                symbol.TextSymbolAlignment = TextSymbolAlignment.rightAlignOver;
                                break;
                            case "middle right":
                                symbol.TextSymbolAlignment = TextSymbolAlignment.rightAlignCenter;
                                break;
                            case "bottom right":
                                symbol.TextSymbolAlignment = TextSymbolAlignment.rightAlignUnder;
                                break;
                            default:
                                symbol.TextSymbolAlignment = TextSymbolAlignment.Center;
                                break;
                        }
                        symbol.SymbolSmoothingMode = SymbolSmoothing.AntiAlias;

                        var fontStyle = FontStyle.Regular;
                        if (ts.Font.Weight == "bold" || ts.Font.Weight == "bolder")
                        {
                            fontStyle |= FontStyle.Bold;
                        }

                        if (ts.Font.Style == "italic")
                        {
                            fontStyle |= FontStyle.Italic;
                        }

                        if (ts.Font.Decoration == "underline")
                        {
                            fontStyle |= FontStyle.Underline;
                        }

                        symbol.Font = Current.Engine.CreateFont(ts.Font.Family, ts.Font.Size, fontStyle);
                        //symbol.Font.Style = ts.Font.Style;
                        //if(ts.Font.Weight)

                        return symbol;
                    }
                }
            } 
            catch { }
            return null;
        }

        #endregion
    }
}

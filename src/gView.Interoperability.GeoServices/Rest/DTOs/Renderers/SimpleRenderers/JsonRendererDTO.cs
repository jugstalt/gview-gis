using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Symbology;
using gView.GraphicsEngine;
using gView.Interoperability.GeoServices.Rest.DTOs.Renderers.OtherRenderers;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.Renderers.SimpleRenderers
{
    public class JsonRendererDTO
    {
        public JsonRendererDTO()
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

        #region ClassBreaks Renderer

        [JsonPropertyName("field")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Field { get; set; }

        [JsonPropertyName("minValue")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? MinValue { get; set; }

        [JsonPropertyName("classBreakInfos")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object[] ClassBreakInfos { get; set; }

        public class ClassBreakInfo
        {
            [JsonPropertyName("classMinValue")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public double? ClassMinValue { get; set; }

            [JsonPropertyName("classMaxValue")]
            public double ClassMaxValue { get; set; }

            [JsonPropertyName("label")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string Label { get; set; }

            [JsonPropertyName("description")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string Description { get; set; }

            [JsonPropertyName("symbol")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public object Symbol { get; set; }
        }

        #endregion

        #region Static Members

        static public JsonRendererDTO FromFeatureRenderer(IFeatureRenderer featureRenderer)
        {
            if (featureRenderer is SimpleRenderer)
            {
                var simpleRenderer = (SimpleRenderer)featureRenderer;
                return new JsonRendererDTO()
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
                    var keys = valueMapRenderer.Keys.Where(k => k != ValueMapRenderer.AllOtherValuesKey).ToArray();

                    return new JsonRendererDTO()
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
                                Label = (valueMapRenderer[k] as ILegendItem)?.LegendLabel,
                                Symbol = FromSymbol(valueMapRenderer[k]),
                            };
                        }).ToArray()
                    };
                }
            }

            if (featureRenderer is QuantityRenderer quantityRenderer)
            {
                var classes = quantityRenderer.QuantityClasses
                    .Where(c => c != null)
                    .OrderBy(c => c.Min)
                    .ToArray();

                return new JsonRendererDTO()
                {
                    Type = "classBreaks",
                    Field = quantityRenderer.ValueField,
                    MinValue = classes.Length > 0 ? classes[0].Min : null,
                    DefaultSymbol = FromSymbol(quantityRenderer.DefaultSymbol),
                    ClassBreakInfos = classes.Select(c => (object)new ClassBreakInfo()
                    {
                        ClassMinValue = c.Min,
                        ClassMaxValue = c.Max,
                        Label = (c.Symbol as ILegendItem)?.LegendLabel,
                        Symbol = FromSymbol(c.Symbol)
                    }).ToArray()
                };
            }

            if (featureRenderer is UniversalGeometryRenderer)
            {
                var univeralRenderer = (UniversalGeometryRenderer)featureRenderer;
                if (univeralRenderer.Symbols != null && univeralRenderer.Symbols.Count() == 3)
                {
                    // take first
                    if (univeralRenderer.UsePointSymbol)
                    {
                        return new JsonRendererDTO()
                        {
                            Type = "simple",
                            Symbol = FromSymbol(univeralRenderer.Symbols[0])
                        };
                    }
                    else if (univeralRenderer.UseLineSymbol)
                    {
                        return new JsonRendererDTO()
                        {
                            Type = "simple",
                            Symbol = FromSymbol(univeralRenderer.Symbols[1])
                        };
                    }
                    else if (univeralRenderer.UsePolygonSymbol)
                    {
                        return new JsonRendererDTO()
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

                    return new SimpleMarkerSymbolDTO()
                    {
                        Style = spSymbol.Marker.FromMarkerType(),
                        Color = spSymbol.Color.ToArray(),
                        Size = spSymbol.Size,
                        Angle = spSymbol.Angle,
                        Xoffset = spSymbol.HorizontalOffset,
                        Yoffset = spSymbol.VerticalOffset,
                        Outline = new SimpleLineSymbolDTO()
                        {
                            Color = spSymbol.OutlineColor.ToArray(),
                            Width = spSymbol.OutlineWidth
                        }
                    };
                }

                if (symbol is Framework.Symbology.TrueTypeMarkerSymbol)
                {
                    var ttmSymbol = (Framework.Symbology.TrueTypeMarkerSymbol)symbol;

                    return new TextSymbolDTO()
                    {
                        Color = ttmSymbol.FontColor.ToArray(),
                        Angle = ttmSymbol.Angle,
                        Xoffset = ttmSymbol.HorizontalOffset,
                        Yoffset = ttmSymbol.VerticalOffset,
                        Font = ttmSymbol.Font != null ?
                        new FontDTO()
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

                    return new SimpleLineSymbolDTO()
                    {
                        Color = lineSymbol.Color.ToArray(),
                        Style = lineSymbol.DashStyle.FromDashStyle(),
                        Width = lineSymbol.Width
                    };
                }

                if (symbol is Framework.Symbology.SimpleFillSymbol)
                {
                    var fillSymbol = (Framework.Symbology.SimpleFillSymbol)symbol;

                    return new SimpleFillSymbolDTO()
                    {
                        Style = "esriSFSSolid",
                        Color = fillSymbol.FillColor.ToArray(),
                        Outline = JsonRendererDTO.FromSymbol(fillSymbol.OutlineSymbol) as SimpleLineSymbolDTO
                    };
                }

                if (symbol is Framework.Symbology.HatchSymbol)
                {
                    var hatchSymbol = (Framework.Symbology.HatchSymbol)symbol;
                    return new SimpleFillSymbolDTO()
                    {
                        Style = hatchSymbol.HatchStyle.FromHatchStyle(),
                        Color = hatchSymbol.FillColor.ToArray(),
                        Outline = JsonRendererDTO.FromSymbol(hatchSymbol.OutlineSymbol) as SimpleLineSymbolDTO
                    };
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Converts an ArcGIS REST renderer (dynamicLayers/drawingInfo) into a gView feature renderer.
        /// Returns null if the renderer type (or its content) is not supported.
        /// </summary>
        static public IFeatureRenderer FromJsonRenderer(JsonRendererDTO jsonRenderer)
        {
            switch (jsonRenderer?.Type?.ToLower())
            {
                case "simple":
                    return new SimpleRenderer()
                    {
                        Symbol = SymbolFromObject(jsonRenderer.Symbol)
                    };
                case "uniquevalue":
                    return FromUniqueValueRenderer(jsonRenderer);
                case "classbreaks":
                    return FromClassBreaksRenderer(jsonRenderer);
            }

            return null;
        }

        static private ISymbol SymbolFromObject(object symbol)
            => symbol switch
            {
                JsonElement jsonElement => FromSymbolJObject(jsonElement),
                _ => null
            };

        static private void SetLegendLabel(ISymbol symbol, string label)
        {
            if (symbol is ILegendItem legendItem && !string.IsNullOrEmpty(label))
            {
                legendItem.LegendLabel = label;
            }
        }

        static private T[] DeserializeInfos<T>(object[] infos)
            => infos?
                .OfType<JsonElement>()
                .Select(e => JsonSerializer.Deserialize<T>(e.GetRawText()))
                .Where(i => i != null)
                .ToArray() ?? [];

        static private void SetDefaultSymbol(object jsonSymbol, Action<ISymbol> setter)
        {
            var defaultSymbol = SymbolFromObject(jsonSymbol);
            if (defaultSymbol != null)
            {
                setter(defaultSymbol);
            }
        }

        static private IFeatureRenderer FromUniqueValueRenderer(JsonRendererDTO json)
        {
            var fields = new[] { json.Field1, json.Field2, json.Field3 }
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToArray();

            if (fields.Length == 0 || string.IsNullOrWhiteSpace(json.Field1))
            {
                return null;
            }

            var infos = DeserializeInfos<UniqueValueInfo>(json.UniqueValueInfos);

            if (fields.Length == 1)
            {
                var renderer = new ValueMapRenderer() { ValueField = fields[0] };

                foreach (var info in infos)
                {
                    var symbol = SymbolFromObject(info.Symbol);
                    if (symbol == null || info.Value == null)
                    {
                        continue;
                    }

                    SetLegendLabel(symbol, info.Label);
                    renderer[info.Value] = symbol;
                }

                SetDefaultSymbol(json.DefaultSymbol, symbol => renderer.DefaultSymbol = symbol);

                return renderer;
            }

            // multiple fields: ManyValueMapRenderer builds its key as "v1|v2|v3"
            var delimiter = string.IsNullOrEmpty(json.FieldDelimiter) ? ", " : json.FieldDelimiter;
            var manyRenderer = new ManyValueMapRenderer()
            {
                ValueField1 = fields[0],
                ValueField2 = fields[1],
                ValueField3 = fields.Length > 2 ? fields[2] : string.Empty
            };

            foreach (var info in infos)
            {
                var symbol = SymbolFromObject(info.Symbol);
                var parts = info.Value?.Split(delimiter);
                if (symbol == null || parts == null || parts.Length != fields.Length)
                {
                    continue;
                }

                SetLegendLabel(symbol, info.Label);
                manyRenderer[string.Join("|", parts)] = symbol;
            }

            SetDefaultSymbol(json.DefaultSymbol, symbol => manyRenderer[null] = symbol);

            return manyRenderer;
        }

        static private IFeatureRenderer FromClassBreaksRenderer(JsonRendererDTO json)
        {
            if (string.IsNullOrWhiteSpace(json.Field))
            {
                return null;
            }

            var renderer = new QuantityRenderer() { ValueField = json.Field };

            // ArcGIS: a class covers (previous classMaxValue, classMaxValue], the first one [minValue, classMaxValue].
            // QuantityRenderer takes the first class that contains the value (min and max inclusive), so touching
            // classes in ascending order give the same result.
            var infos = DeserializeInfos<ClassBreakInfo>(json.ClassBreakInfos)
                .OrderBy(i => i.ClassMaxValue)
                .ToArray();

            double min = json.MinValue ?? infos.FirstOrDefault()?.ClassMinValue ?? double.MinValue;
            foreach (var info in infos)
            {
                var symbol = SymbolFromObject(info.Symbol);
                if (symbol != null && info.ClassMaxValue >= min)
                {
                    SetLegendLabel(symbol, info.Label);
                    renderer.AddClass(new QuantityRenderer.QuantityClass(min, info.ClassMaxValue, symbol));
                }
                min = info.ClassMaxValue;
            }

            SetDefaultSymbol(json.DefaultSymbol, symbol => renderer.DefaultSymbol = symbol);

            return renderer;
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
                    var sms = JsonSerializer.Deserialize<SimpleMarkerSymbolDTO>(jElement.ToString());

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
                    var sls = JsonSerializer.Deserialize<SimpleLineSymbolDTO>(jElement.ToString());

                    var symbol = new Framework.Symbology.SimpleLineSymbol();
                    symbol.Color = sls.Style == "esriSLSNull" ? ArgbColor.Transparent : sls.Color.ToColor();
                    symbol.DashStyle = sls.Style.ToDashStyle();
                    symbol.Width = sls.Width;
                    symbol.SymbolSmoothingMode = SymbolSmoothing.AntiAlias;
                    symbol.LineEndCap = symbol.LineStartCap = LineCap.Round;

                    return symbol;
                }

                if (type == "esriSFS")
                {
                    var sfs = JsonSerializer.Deserialize<SimpleFillSymbolDTO>(jElement.ToString());

                    var outline = sfs.Outline != null
                        ? new Framework.Symbology.SimpleLineSymbol()
                        {
                            Color = sfs.Outline.Style == "esriSLSNull" ? ArgbColor.Transparent : sfs.Outline.Color.ToColor(),
                            Width = sfs.Outline.Width,
                            DashStyle = sfs.Outline.Style.ToDashStyle(),
                            SymbolSmoothingMode = SymbolSmoothing.AntiAlias,
                            LineStartCap = LineCap.Round,
                            LineEndCap = LineCap.Round
                        }
                        : null;

                    var hatchStyle = sfs.Style.ToHatchStyle();
                    if (hatchStyle.HasValue)
                    {
                        return new Framework.Symbology.HatchSymbol()
                        {
                            HatchStyle = hatchStyle.Value,
                            ForeColor = sfs.Color.ToColor(),
                            BackColor = ArgbColor.Transparent,
                            OutlineSymbol = outline
                        };
                    }

                    var symbol = new Framework.Symbology.SimpleFillSymbol();
                    // esriSFSNull: no fill, outline only
                    symbol.FillColor = sfs.Style == "esriSFSNull" ? ArgbColor.Transparent : sfs.Color.ToColor();
                    symbol.OutlineSymbol = outline;

                    return symbol;
                }

                if (type == "esriPMS")
                {
                    return PictureMarkerSymbolDTO.ToSymbol(JsonSerializer.Deserialize<PictureMarkerSymbolDTO>(jElement.ToString()));
                }

                if (type == "esriTS")
                {
                    var ts = JsonSerializer.Deserialize<TextSymbolDTO>(jElement.ToString());

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

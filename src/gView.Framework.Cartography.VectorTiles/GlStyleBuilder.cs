using gView.Framework.Cartography.Rendering;
using gView.Framework.Cartography.Rendering.Abstractions;
using gView.Framework.Cartography.VectorTiles.Extensions;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Metadata;
using gView.GraphicsEngine;
using System.Globalization;
using System.Text.Json.Nodes;

namespace gView.Framework.Cartography.VectorTiles;

/// <summary>
/// Builds a MapLibre/Mapbox GL style (version 8) for the vector tiles of a service map, derived from
/// the feature renderers as far as GL can express them.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Zoom levels: for a Mapbox conform WebMercator cache the real zoom levels, otherwise the scale index of the
/// metadata (as OpenLayers numbers the levels of a custom tile grid).</item>
/// <item>Renderers: Simple, ValueMap and ManyValueMap (one filter per key), Quantity (range filters), ScaleDependent (minzoom/maxzoom),
/// FeatureGroup (several style layers). Others get a default style per geometry type.</item>
/// <item>Symbols: fill, line (color, width, dash), point (circle). Symbol collections become several style layers.
/// Anything GL can't draw is approximated (hatch → fill color, picture/font marker → circle).</item>
/// <item>Sizes follow the reference scale (and pixel/meter units) per level with <c>interpolate</c>.</item>
/// </list>
/// Labels are not part of the style yet.
/// </remarks>
public sealed class GlStyleBuilder
{
    public const string SourceName = "gview";

    private readonly IServiceMap _map;
    private readonly TileServiceMetadata _metadata;
    private readonly int _epsg;
    private readonly double[] _scales;
    private readonly double[] _zooms;
    private readonly List<string> _warnings = new();

    public GlStyleBuilder(IServiceMap map, TileServiceMetadata metadata, int epsg)
    {
        _map = map;
        _metadata = metadata;
        _epsg = epsg;
        _scales = metadata.Scales.InnerList.ToArray();

        bool webMercator = epsg == 3857 && metadata.IsWebMercatorConform();
        _zooms = Enumerable.Range(0, _scales.Length)
            .Select(level => webMercator ? (double)metadata.WebMercatorZoom(level) : level)
            .ToArray();
        IsWebMercator = webMercator;
    }

    /// <summary>True: zoom levels are real WebMercator zooms and the tiles are served as z/x/y.</summary>
    public bool IsWebMercator { get; }

    /// <summary>Renderers or symbols that could only be approximated.</summary>
    public IReadOnlyList<string> Warnings => _warnings;

    public double MinZoom => _zooms.Length > 0 ? _zooms.Min() : 0;
    public double MaxZoom => _zooms.Length > 0 ? _zooms.Max() : 0;

    public JsonObject Build(string name, string tileUrlTemplate)
    {
        var layers = new JsonArray();
        var layerNames = MvtLayerNames.FromMap(_map);

        foreach (var layer in DrawingOrder())
        {
            if (layer is not IFeatureLayer featureLayer || featureLayer.FeatureClass is null)
            {
                continue;
            }

            string? sourceLayer = layerNames[layer];
            var levels = VisibleLevels(layer.MinimumScale, layer.MaximumScale, Enumerable.Range(0, _scales.Length));
            if (sourceLayer == null || levels.Count == 0 || featureLayer.FeatureRenderer == null)
            {
                continue;
            }

            var context = new LayerContext(featureLayer, sourceLayer, featureLayer.FeatureClass.GeometryType);
            foreach (var styleLayer in RendererLayers(context, featureLayer.FeatureRenderer, filter: null, levels))
            {
                styleLayer["id"] = $"{sourceLayer}-{context.Counter++}";
                layers.Add(styleLayer);
            }
        }

        var source = new JsonObject
        {
            ["type"] = "vector",
            ["tiles"] = new JsonArray(tileUrlTemplate),
            ["minzoom"] = MinZoom,
            ["maxzoom"] = MaxZoom
        };

        return new JsonObject
        {
            ["version"] = 8,
            ["name"] = name,
            ["metadata"] = new JsonObject
            {
                ["gview:epsg"] = _epsg,
                ["gview:webmercator"] = IsWebMercator,
                ["gview:warnings"] = new JsonArray(_warnings.Distinct().Select(w => (JsonNode)w!).ToArray())
            },
            ["sources"] = new JsonObject { [SourceName] = source },
            ["layers"] = layers
        };
    }

    #region Renderers

    private IEnumerable<JsonObject> RendererLayers(LayerContext context, IFeatureRenderer renderer, JsonNode? filter, List<int> levels)
    {
        switch (renderer)
        {
            case SimpleRenderer simple:
                return SymbolLayers(context, simple.Symbol, filter, levels);

            case ValueMapRenderer valueMap:
                return ValueMapLayers(context, new JsonArray("to-string", new JsonArray("get", valueMap.ValueField)),
                    valueMap.Keys, key => valueMap[key], filter, levels);

            case ManyValueMapRenderer manyValueMap:
                return ValueMapLayers(context, ManyValueKey(manyValueMap),
                    manyValueMap.Keys, key => manyValueMap[key], filter, levels);

            case QuantityRenderer quantity:
                return QuantityLayers(context, quantity, filter, levels);

            case ScaleDependentRenderer scaleDependent:
                return scaleDependent.RendererItems
                    .OfType<IScaledependent>()
                    .SelectMany(item =>
                    {
                        var itemLevels = VisibleLevels(item.MinimumScale, item.MaximumScale, levels);
                        var inner = ((IRendererGroupItem)item).Renderer as IFeatureRenderer;
                        return inner == null || itemLevels.Count == 0
                            ? []
                            : RendererLayers(context, inner, filter, itemLevels);
                    })
                    .ToList();

            case FeatureGroupRenderer group:
                return group.RendererItems
                    .Select(item => item.Renderer as IFeatureRenderer)
                    .Where(inner => inner != null)
                    .SelectMany(inner => RendererLayers(context, inner!, filter, levels))
                    .ToList();

            default:
                _warnings.Add($"{context.SourceLayer}: renderer '{renderer.Name}' is not supported, default style used");
                return DefaultLayers(context, filter, levels);
        }
    }

    /// <summary>
    /// One set of style layers per key, filtered on <paramref name="value"/> (a string expression),
    /// and one for "all other values" if the renderer has it.
    /// </summary>
    private IEnumerable<JsonObject> ValueMapLayers(LayerContext context,
                                                   JsonArray value,
                                                   IEnumerable<string> rendererKeys,
                                                   Func<string, ISymbol?> symbolOf,
                                                   JsonNode? filter,
                                                   List<int> levels)
    {
        var allKeys = rendererKeys.ToList();
        var keys = allKeys.Where(k => k != ValueMapRenderer.AllOtherValuesKey).ToList();
        var result = new List<JsonObject>();

        foreach (string key in keys)
        {
            var keyFilter = new JsonArray("==", value.DeepClone(), key);
            result.AddRange(SymbolLayers(context, symbolOf(key), And(filter, keyFilter), levels));
        }

        if (allKeys.Contains(ValueMapRenderer.AllOtherValuesKey))
        {
            var othersFilter = new JsonArray("!",
                new JsonArray("in", value.DeepClone(), new JsonArray("literal", new JsonArray(keys.Select(k => (JsonNode)k!).ToArray()))));
            result.AddRange(SymbolLayers(context, symbolOf(ValueMapRenderer.AllOtherValuesKey), And(filter, othersFilter), levels));
        }

        return result;
    }

    /// <summary>
    /// The key of <see cref="ManyValueMapRenderer.GetKey"/> as GL expression: the field values as strings
    /// (null → empty), a "|" in front of field 2 and 3 when they are set.
    /// </summary>
    private static JsonArray ManyValueKey(ManyValueMapRenderer renderer)
    {
        var concat = new JsonArray("concat");
        void Add(string? field, bool separator)
        {
            if (String.IsNullOrEmpty(field))
            {
                return;
            }

            if (separator)
            {
                concat.Add("|");
            }

            concat.Add(new JsonArray("to-string", new JsonArray("get", field)));
        }

        Add(renderer.ValueField1, separator: false);
        Add(renderer.ValueField2, separator: true);
        Add(renderer.ValueField3, separator: true);

        return concat.Count == 2 ? (JsonArray)concat[1]!.DeepClone() : concat;
    }

    private IEnumerable<JsonObject> QuantityLayers(LayerContext context, QuantityRenderer renderer, JsonNode? filter, List<int> levels)
    {
        var value = new JsonArray("to-number", new JsonArray("get", renderer.ValueField));
        var result = new List<JsonObject>();
        var inAnyClass = new JsonArray("any");

        foreach (var qClass in renderer.QuantityClasses)
        {
            var classFilter = new JsonArray("all",
                new JsonArray(">=", value.DeepClone(), qClass.Min),
                new JsonArray("<=", value.DeepClone(), qClass.Max));
            inAnyClass.Add(classFilter.DeepClone());
            result.AddRange(SymbolLayers(context, qClass.Symbol, And(filter, classFilter), levels));
        }

        if (renderer.DefaultSymbol != null)
        {
            result.AddRange(SymbolLayers(context, renderer.DefaultSymbol, And(filter, new JsonArray("!", inAnyClass)), levels));
        }

        return result;
    }

    private IEnumerable<JsonObject> DefaultLayers(LayerContext context, JsonNode? filter, List<int> levels)
    {
        var gray = ArgbColor.FromArgb(255, 128, 128, 128);
        return context.GeometryType switch
        {
            GeometryType.Polygon =>
            [
                Layer(context, "fill", filter, levels, new JsonObject { ["fill-color"] = Color(gray), ["fill-opacity"] = 0.5 }),
                Layer(context, "line", filter, levels, new JsonObject { ["line-color"] = Color(gray), ["line-width"] = 1 })
            ],
            GeometryType.Polyline => [Layer(context, "line", filter, levels, new JsonObject { ["line-color"] = Color(gray), ["line-width"] = 1 })],
            _ => [Layer(context, "circle", filter, levels, new JsonObject { ["circle-color"] = Color(gray), ["circle-radius"] = 3 })]
        };
    }

    #endregion

    #region Symbols

    private IEnumerable<JsonObject> SymbolLayers(LayerContext context, ISymbol? symbol, JsonNode? filter, List<int> levels)
    {
        if (symbol == null)
        {
            return [];
        }

        if (symbol is ISymbolCollection collection)
        {
            return collection.Symbols
                .Where(item => item.Visible)
                .SelectMany(item => SymbolLayers(context, item.Symbol, filter, levels))
                .ToList();
        }

        var result = new List<JsonObject>();

        switch (symbol)
        {
            case IFillSymbol fill:
                if (fill is IBrushColor brush && brush.FillColor.A > 0)
                {
                    if (symbol.GetType().Name != "SimpleFillSymbol")
                    {
                        _warnings.Add($"{context.SourceLayer}: {symbol.GetType().Name} approximated by its fill color");
                    }

                    var (color, opacity) = ColorAndOpacity(brush.FillColor);
                    result.Add(Layer(context, "fill", filter, levels, new JsonObject
                    {
                        ["fill-color"] = color,
                        ["fill-opacity"] = opacity
                    }));
                }

                if (fill is IOutlineSymbol outlined && outlined.OutlineSymbol != null)
                {
                    result.AddRange(SymbolLayers(context, outlined.OutlineSymbol, filter, levels));
                }
                break;

            case ILineSymbol:
                if (symbol is IPenColor pen && pen.PenColor.A > 0)
                {
                    var (color, opacity) = ColorAndOpacity(pen.PenColor);
                    var paint = new JsonObject
                    {
                        ["line-color"] = color,
                        ["line-opacity"] = opacity,
                        ["line-width"] = symbol is IPenWidth width
                            ? Size(context, width.PenWidth, width.MinPenWidth, width.MaxPenWidth, width.PenWidthUnit, minimum: 1)
                            : 1
                    };

                    if (symbol is IPenDashStyle dash && DashArray(dash.PenDashStyle) is JsonArray dashArray)
                    {
                        paint["line-dasharray"] = dashArray;
                    }

                    var layer = Layer(context, "line", filter, levels, paint);
                    layer["layout"] = new JsonObject { ["line-join"] = "round", ["line-cap"] = "round" };
                    result.Add(layer);
                }
                else
                {
                    _warnings.Add($"{context.SourceLayer}: line symbol {symbol.GetType().Name} is not supported");
                }
                break;

            case IPointSymbol:
                if (symbol.GetType().Name != "SimplePointSymbol")
                {
                    _warnings.Add($"{context.SourceLayer}: {symbol.GetType().Name} approximated by a circle");
                }

                var fillColor = symbol is IBrushColor pointBrush ? pointBrush.FillColor : ArgbColor.FromArgb(255, 128, 128, 128);
                var circle = new JsonObject
                {
                    ["circle-color"] = ColorAndOpacity(fillColor).color,
                    ["circle-opacity"] = ColorAndOpacity(fillColor).opacity,
                    ["circle-radius"] = symbol is ISymbolSize size
                        ? Size(context, size.SymbolSize / 2f, size.MinSymbolSize / 2f, size.MaxSymbolSize / 2f, DrawingUnit.Pixel, minimum: 1)
                        : 3
                };

                if (symbol is IPenColor outline && outline.PenColor.A > 0)
                {
                    circle["circle-stroke-color"] = ColorAndOpacity(outline.PenColor).color;
                    circle["circle-stroke-opacity"] = ColorAndOpacity(outline.PenColor).opacity;
                    circle["circle-stroke-width"] = symbol is IPenWidth outlineWidth ? Math.Max(0, outlineWidth.PenWidth) : 1;
                }

                result.Add(Layer(context, "circle", filter, levels, circle));
                break;

            default:
                _warnings.Add($"{context.SourceLayer}: symbol {symbol.GetType().Name} is not supported");
                break;
        }

        return result;
    }

    /// <summary>
    /// A size in pixels: constant, or interpolated over the zoom levels when it depends on the
    /// reference scale (pixel units) or the resolution (meter units).
    /// </summary>
    private JsonNode Size(LayerContext context, float size, float minSize, float maxSize, DrawingUnit unit, float minimum)
    {
        var values = new double[_scales.Length];
        for (int level = 0; level < _scales.Length; level++)
        {
            double value;
            if (unit == DrawingUnit.Meters)
            {
                double metersPerPixel = _scales[level] / (_metadata.Dpi / 0.0254);
                value = size / metersPerPixel;
            }
            else
            {
                value = size * context.RefScaleFactor(_map.Display.ReferenceScale, _scales[level]);
                if (minSize > 0 && value < minSize) value = minSize;
                if (maxSize > 0 && value > maxSize) value = maxSize;
            }

            values[level] = Math.Round(Math.Max(minimum, value), 2);
        }

        if (values.Length == 0 || values.All(v => v == values[0]))
        {
            return values.Length == 0 ? size : values[0];
        }

        var interpolate = new JsonArray("interpolate", new JsonArray("linear"), new JsonArray("zoom"));
        foreach (int level in Enumerable.Range(0, _scales.Length).OrderBy(l => _zooms[l]))
        {
            interpolate.Add(_zooms[level]);
            interpolate.Add(values[level]);
        }

        return interpolate;
    }

    private static JsonArray? DashArray(LineDashStyle style) => style switch
    {
        LineDashStyle.Dash => new JsonArray(4, 2),
        LineDashStyle.Dot => new JsonArray(1, 2),
        LineDashStyle.DashDot => new JsonArray(4, 2, 1, 2),
        LineDashStyle.DashDotDot => new JsonArray(4, 2, 1, 2, 1, 2),
        _ => null
    };

    private static string Color(ArgbColor color)
        => $"rgb({color.R},{color.G},{color.B})";

    private static (string color, double opacity) ColorAndOpacity(ArgbColor color)
        => (Color(color), Math.Round(color.A / 255.0, 3));

    #endregion

    #region Layers, levels and filters

    private JsonObject Layer(LayerContext context, string type, JsonNode? filter, List<int> levels, JsonObject paint)
    {
        var layer = new JsonObject
        {
            ["id"] = "",
            ["type"] = type,
            ["source"] = SourceName,
            ["source-layer"] = context.SourceLayer
        };

        // levels are contiguous scale indices: GL minzoom is inclusive, maxzoom exclusive. The last level stays
        // open, so clients can overzoom with its tiles
        double minZoom = levels.Min(l => _zooms[l]);
        int lastLevel = levels.MaxBy(l => _zooms[l]);
        if (minZoom > MinZoom)
        {
            layer["minzoom"] = minZoom;
        }

        if (_zooms[lastLevel] < MaxZoom)
        {
            layer["maxzoom"] = NextZoom(_zooms[lastLevel]);
        }

        var geometryFilter = type switch
        {
            "fill" => new JsonArray("==", new JsonArray("geometry-type"), "Polygon"),
            "circle" => new JsonArray("==", new JsonArray("geometry-type"), "Point"),
            _ => null
        };

        var combined = And(filter?.DeepClone(), geometryFilter);
        if (combined != null)
        {
            layer["filter"] = combined;
        }

        layer["paint"] = paint;
        return layer;
    }

    private double NextZoom(double zoom)
        => _zooms.Where(z => z > zoom).DefaultIfEmpty(zoom + 1).Min();

    /// <summary>
    /// The scale indices inside a gView scale range (denominators, values &lt;= 1 = unlimited).
    /// </summary>
    private List<int> VisibleLevels(double minimumScale, double maximumScale, IEnumerable<int> levels)
        => levels.Where(level =>
                !(minimumScale > 1 && minimumScale > _scales[level]) &&
                !(maximumScale > 1 && maximumScale < _scales[level]))
            .ToList();

    private static JsonNode? And(JsonNode? a, JsonNode? b)
    {
        if (a == null) return b;
        if (b == null) return a;
        return new JsonArray("all", a.DeepClone(), b.DeepClone());
    }

    private IEnumerable<ILayer> DrawingOrder()
    {
        if (_map.TOC != null)
        {
            var layers = new List<ILayer>(_map.TOC.VisibleLayers);
            layers.Reverse();  // TOC is top to bottom, GL draws in order
            return layers;
        }

        return _map.MapElements.OfType<ILayer>().Where(l => l.Visible);
    }

    private sealed class LayerContext(IFeatureLayer layer, string sourceLayer, GeometryType geometryType)
    {
        public string SourceLayer { get; } = sourceLayer;
        public GeometryType GeometryType { get; } = geometryType;
        public int Counter { get; set; }

        public double RefScaleFactor(double referenceScale, double scale) => layer.RefScaleFactor(referenceScale, scale);
    }

    #endregion
}

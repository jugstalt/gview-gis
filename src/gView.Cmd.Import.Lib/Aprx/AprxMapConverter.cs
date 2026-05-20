using gView.Cmd.Import.Aprx.Models;
using gView.DataSources.Unknown;
using gView.Framework.Cartography;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.Symbology.Models;
using gView.GraphicsEngine;

namespace gView.Cmd.Import.Aprx;

/// <summary>
/// Converts a parsed <see cref="AprxMapResult"/> (CIM model) into a gView <see cref="Map"/>
/// with <see cref="FeatureLayer"/> objects, renderers, label renderers and definition queries.
/// </summary>
internal class AprxMapConverter
{
    private readonly Action<string>? _warn;
    private string _currentLayerName = string.Empty;
    private readonly DatasetPluginOptions? _datasetOptions;
    private IDataset? _sharedDataset;

    /// <param name="warn">Optional callback invoked for non-fatal conversion warnings.</param>
    /// <param name="datasetPlugin">When supplied, all imported feature classes are bound to this dataset instead of <see cref="UnknownFeatureDataset"/>.</param>
    public AprxMapConverter(Action<string>? warn = null, DatasetPluginOptions? datasetPlugin = null)
    {
        _warn = warn;
        _datasetOptions = datasetPlugin;
    }

    private IFeatureClass CreateFeatureClassFromPlugin(string datasetName)
    {
        // Lazily create and open the shared dataset on first use
        if (_sharedDataset == null)
        {
            var pluginManager = new PlugInManager();
            _sharedDataset = pluginManager.CreateInstance(_datasetOptions!.PluginGuid) as IDataset
                ?? throw new InvalidOperationException($"Plugin '{_datasetOptions.PluginGuid}' could not be instantiated as IDataset.");

            _sharedDataset.SetConnectionString(_datasetOptions.ConnectionString).GetAwaiter().GetResult();
            _sharedDataset.Open().GetAwaiter().GetResult();
        }

        return new UnknownFeatureClass(_sharedDataset, datasetName);
    }

    /// <summary>
    /// Creates a gView <see cref="Map"/> from the supplied CIM map result.
    /// </summary>
    public Map Convert(AprxMapResult mapResult)
    {
        var cimMap = mapResult.Map;

        var map = new Map
        {
            Name = cimMap.Name ?? "Imported Map"
        };

        // Spatial reference
        if (cimMap.SpatialReference != null)
        {
            int wkid = cimMap.SpatialReference.EffectiveWkid;
            if (wkid > 0)
            {
                map.LayerDefaultSpatialReference = SpatialReference.FromID($"epsg:{wkid}");
            }
        }

        // Initial extent
        var extent = cimMap.MapExtent ?? cimMap.DefaultExtent;
        if (extent != null)
        {
            map.ZoomTo(new Envelope(extent.XMin, extent.YMin, extent.XMax, extent.YMax));
        }

        // Add layers in the same order as in the APRX so the TOC matches
        foreach (var cimLayer in mapResult.Layers)
        {
            AddLayer(map, cimLayer);
        }

        return map;
    }

    // -----------------------------------------------------------------------
    // Layer conversion
    // -----------------------------------------------------------------------

    private void AddLayer(Map map, CimBaseLayer cimLayer)
    {
        switch (cimLayer)
        {
            case CimGroupLayer group:
                AddGroupLayer(map, group);
                break;

            case CimFeatureLayer feature:
                AddFeatureLayer(map, feature);
                break;
        }
    }

    private void AddGroupLayer(Map map, CimGroupLayer cimGroup)
    {
        var groupLayer = new GroupLayer(cimGroup.Name ?? string.Empty)
        {
            Visible = cimGroup.Visibility
        };

        // Add the group to the map first so its TOC entry exists
        // before child layers are added (child AddLayer needs the parent TOC element).
        map.AddLayer(groupLayer);

        if (cimGroup.LayerDefinitions != null)
        {
            foreach (var child in cimGroup.LayerDefinitions)
            {
                AddChildLayer(map, groupLayer, child);
            }
        }
    }

    /// <summary>
    /// Adds a CIM layer as a child of <paramref name="parentGroupLayer"/> inside the map.
    /// Both <see cref="GroupLayer.Add"/> and <see cref="Map.AddLayer"/> must be called:
    /// the former registers the parent reference on the layer object, and the latter
    /// adds the layer to the map's flat layer list and TOC.
    /// </summary>
    private void AddChildLayer(Map map, GroupLayer parentGroupLayer, CimBaseLayer cimLayer)
    {
        switch (cimLayer)
        {
            case CimGroupLayer cimGroup:
            {
                var childGroup = new GroupLayer(cimGroup.Name ?? string.Empty)
                {
                    Visible = cimGroup.Visibility
                };

                parentGroupLayer.Add(childGroup);
                map.AddLayer(childGroup);

                if (cimGroup.LayerDefinitions != null)
                {
                    foreach (var grandChild in cimGroup.LayerDefinitions)
                    {
                        AddChildLayer(map, childGroup, grandChild);
                    }
                }
                break;
            }

            case CimFeatureLayer cimFeature:
            {
                var layer = CreateFeatureLayer(cimFeature);
                parentGroupLayer.Add(layer);
                map.AddLayer(layer);

                // Set the layer name
                map.TOC.GetTOCElement(layer)?.Name = cimFeature.Name;

                break;
            }
        }
    }

    private void AddFeatureLayer(Map map, CimFeatureLayer cimFeature)
    {
        var layer = (FeatureLayer)CreateFeatureLayer(cimFeature);
        map.AddLayer(layer);

        // Set the layer name
        map.TOC.GetTOCElement(layer)?.Name = cimFeature.Name;
    }

    private Layer? CreateLayer(CimBaseLayer cimLayer)
    {
        return cimLayer switch
        {
            CimFeatureLayer feature => CreateFeatureLayer(feature),
            CimGroupLayer group => CreateGroupLayerObject(group),
            _ => null
        };
    }

    private GroupLayer CreateGroupLayerObject(CimGroupLayer cimGroup)
    {
        var groupLayer = new GroupLayer(cimGroup.Name ?? string.Empty)
        {
            Visible = cimGroup.Visibility
        };

        if (cimGroup.LayerDefinitions != null)
        {
            foreach (var child in cimGroup.LayerDefinitions)
            {
                var childLayer = CreateLayer(child);
                if (childLayer is not null)
                {
                    groupLayer.Add(childLayer);
                }
            }
        }

        return groupLayer;
    }

    private FeatureLayer CreateFeatureLayer(CimFeatureLayer cimFeature)
    {
        _currentLayerName = cimFeature.Name ?? string.Empty;

        IFeatureClass featureClass;
        if (_datasetOptions != null)
        {
            featureClass = CreateFeatureClassFromPlugin(
                cimFeature.FeatureTable?.DataConnection?.Dataset ?? string.Empty);
        }
        else
        {
            featureClass = new UnknownFeatureClass(
                new UnknownFeatureDataset()
                {
                    ConnectionString = cimFeature.FeatureTable?.DataConnection?.WorkspaceConnectionString ?? ""
                },
                cimFeature.FeatureTable?.DataConnection?.Dataset ?? string.Empty
            );
        }
        var layer = new FeatureLayer(featureClass)
        {
            Visible = cimFeature.Visibility,
            // Title may differ from the feature class name (display alias)
            Title = featureClass.Name, //cimFeature.Name ?? String.Empty,
            MinimumScale = cimFeature.MinScale,
            MaximumScale = cimFeature.MaxScale,
            MinimumLabelScale = cimFeature.MinScale,
            MaximumLabelScale = cimFeature.MaxScale,
        };

        layer.ID = cimFeature.ServiceLayerId;

        // Definition query
        if (!string.IsNullOrWhiteSpace(cimFeature.DefinitionExpression))
        {
            layer.FilterQuery = new QueryFilter
            {
                WhereClause = cimFeature.DefinitionExpression
            };
        }

        // Feature renderer
        if (cimFeature.Renderer != null)
        {
            layer.FeatureRenderer = ConvertRenderer(cimFeature.Renderer);
            if (layer.FeatureRenderer is null)
            {
                _warn?.Invoke($"Layer '{cimFeature.Name}': renderer type '{cimFeature.Renderer.GetType().Name}' could not be converted and was skipped.");
            }
        }

        // Label renderer
        if (cimFeature.LabelVisibility && cimFeature.LabelClasses?.Count > 0)
        {
            layer.LabelRenderer = ConvertLabelRenderer(cimFeature.LabelClasses[0]);
        }

        return layer;
    }

    // -----------------------------------------------------------------------
    // Renderer conversion
    // -----------------------------------------------------------------------

    private IFeatureRenderer? ConvertRenderer(CimRenderer cimRenderer)
    {
        return cimRenderer switch
        {
            CimSimpleRenderer simple => ConvertSimpleRenderer(simple),
            CimUniqueValueRenderer unique => ConvertUniqueValueRenderer(unique),
            CimClassBreaksRenderer breaks => ConvertClassBreaksRenderer(breaks),
            _ => null
        };
    }

    private SimpleRenderer ConvertSimpleRenderer(CimSimpleRenderer cimSimple)
    {
        var renderer = new SimpleRenderer();
        var symbol = ConvertSymbolReference(cimSimple.Symbol);
        if (symbol != null)
        {
            renderer.Symbol = symbol;
        }
        if (!string.IsNullOrWhiteSpace(cimSimple.Label) && renderer.Symbol is ILegendItem legendItem)
        {
            legendItem.LegendLabel = cimSimple.Label;
        }
        return renderer;
    }

    private IFeatureRenderer ConvertUniqueValueRenderer(CimUniqueValueRenderer cimUnique)
    {
        var fields = cimUnique.Fields ?? [];
        return fields.Count > 1
            ? ConvertManyValueMapRenderer(cimUnique, fields)
            : ConvertValueMapRenderer(cimUnique);
    }

    private ValueMapRenderer ConvertValueMapRenderer(CimUniqueValueRenderer cimUnique)
    {
        var renderer = new ValueMapRenderer
        {
            ValueField = cimUnique.Fields?.FirstOrDefault() ?? string.Empty
        };

        ApplyUniqueRendererDefaults(renderer, cimUnique);

        if (cimUnique.Groups != null)
        {
            foreach (var group in cimUnique.Groups)
            {
                if (group.Classes == null) continue;

                foreach (var cls in group.Classes)
                {
                    if (!cls.Visible) continue;

                    var symbol = ConvertSymbolReference(cls.Symbol);
                    if (symbol == null) continue;

                    if (symbol is ILegendItem li)
                        li.LegendLabel = cls.Label ?? string.Empty;

                    var value = cls.Values?.FirstOrDefault()?.FieldValues?.FirstOrDefault();
                    if (value != null)
                        renderer[value] = symbol;
                }
            }
        }

        return renderer;
    }

    private ManyValueMapRenderer ConvertManyValueMapRenderer(CimUniqueValueRenderer cimUnique, List<string> fields)
    {
        var renderer = new ManyValueMapRenderer();

        if (fields.Count >= 1) renderer.ValueField1 = fields[0];
        if (fields.Count >= 2) renderer.ValueField2 = fields[1];
        if (fields.Count >= 3) renderer.ValueField3 = fields[2];

        ApplyUniqueRendererDefaults(renderer, cimUnique);

        if (cimUnique.Groups != null)
        {
            foreach (var group in cimUnique.Groups)
            {
                if (group.Classes == null) continue;

                foreach (var cls in group.Classes)
                {
                    if (!cls.Visible) continue;

                    var symbol = ConvertSymbolReference(cls.Symbol);
                    if (symbol == null) continue;

                    if (symbol is ILegendItem li)
                        li.LegendLabel = cls.Label ?? string.Empty;

                    // Build the composite key: "val1|val2|val3" (same format as ManyValueMapRenderer.GetKey)
                    var key = cls.Values?.FirstOrDefault() is { } uv
                        ? string.Join("|", uv.FieldValues ?? [])
                        : null;

                    if (!string.IsNullOrEmpty(key))
                        renderer[key] = symbol;
                }
            }
        }

        return renderer;
    }

    private void ApplyUniqueRendererDefaults(ValueMapRenderer renderer, CimUniqueValueRenderer cimUnique)
    {
        if (!cimUnique.UseDefaultSymbol || cimUnique.DefaultSymbol == null) return;

        var sym = ConvertSymbolReference(cimUnique.DefaultSymbol);
        renderer.DefaultSymbol = sym;
        if (sym is ILegendItem li && !string.IsNullOrEmpty(cimUnique.DefaultLabel))
        {
            li.LegendLabel = cimUnique.DefaultLabel;
        }
    }

    private void ApplyUniqueRendererDefaults(ManyValueMapRenderer renderer, CimUniqueValueRenderer cimUnique)
    {
        if (!cimUnique.UseDefaultSymbol || cimUnique.DefaultSymbol == null) return;

        // ManyValueMapRenderer has no DefaultSymbol property; the null key acts as "all other values"
        var sym = ConvertSymbolReference(cimUnique.DefaultSymbol);
        if (sym == null) return;

        if (sym is ILegendItem li && !string.IsNullOrEmpty(cimUnique.DefaultLabel))
        {
            li.LegendLabel = cimUnique.DefaultLabel;
        }
        renderer[null] = sym;
    }

    private SimpleRenderer ConvertClassBreaksRenderer(CimClassBreaksRenderer cimBreaks)
    {
        // gView does not have a direct class-breaks renderer.
        // Fall back to a simple renderer using the first break's symbol.
        var renderer = new SimpleRenderer();
        var firstBreak = cimBreaks.Breaks?.FirstOrDefault();
        if (firstBreak?.Symbol != null)
        {
            renderer.Symbol = ConvertSymbolReference(firstBreak.Symbol);
        }
        return renderer;
    }

    // -----------------------------------------------------------------------
    // Label renderer conversion
    // -----------------------------------------------------------------------

    private SimpleLabelRenderer ConvertLabelRenderer(CimLabelClass cimLabel)
    {
        var renderer = new SimpleLabelRenderer();

        // Determine the label field/expression
        var fieldName = cimLabel.FieldNames?.FirstOrDefault()
                     ?? cimLabel.Expression
                     ?? string.Empty;

        renderer.FieldName = fieldName;

        if (cimLabel.TextSymbol?.Symbol is CimTextSymbol textSym)
        {
            ApplyTextSymbol(renderer, textSym);
        }

        // Note: SimpleLabelRenderer does not expose a WhereClause;
        // label filtering via where-clause requires a FilterDependentLabelRenderer
        // which is not yet implemented in gView. The property is ignored for now.

        return renderer;
    }

    private void ApplyTextSymbol(SimpleLabelRenderer renderer, CimTextSymbol cimText)
    {
        float fontSize = cimText.Height > 0 ? (float)cimText.Height : 10f;
        string fontFamily = string.IsNullOrWhiteSpace(cimText.FontFamilyName)
            ? "Arial"
            : cimText.FontFamilyName;

        if (renderer.TextSymbol is SimpleTextSymbol textSymbol)
        {
            textSymbol.Font = gView.GraphicsEngine.Current.Engine.CreateFont(fontFamily, fontSize);
        }
    }

    // -----------------------------------------------------------------------
    // Symbol conversion
    // -----------------------------------------------------------------------

    private ISymbol? ConvertSymbolReference(CimSymbolReference? symRef)
    {
        if (symRef?.Symbol == null)
        {
            return null;
        }

        return ConvertSymbol(symRef.Symbol);
    }

    private ISymbol? ConvertSymbol(CimSymbol cimSymbol)
    {
        return cimSymbol switch
        {
            CimPointSymbol point => ConvertPointSymbol(point),
            CimLineSymbol line => ConvertLineSymbol(line),
            CimPolygonSymbol poly => ConvertPolygonSymbol(poly),
            _ => null
        };
    }

    private ISymbol ConvertPointSymbol(CimPointSymbol cimPoint)
    {
        var enabledLayers = cimPoint.SymbolLayers?.Where(l => l.Enable).ToList() ?? [];

        // Convert each enabled layer to a gView symbol
        var symbols = new List<ISymbol>();
        foreach (var layer in enabledLayers)
        {
            ISymbol? sym = layer switch
            {
                CimCharacterMarker marker => ConvertCharacterMarker(marker),
                // Additional point layer types can be added here
                _ => null
            };
            if (sym != null) symbols.Add(sym);
        }

        // Fallback: no marker layers → simple point from fill + stroke
        if (symbols.Count == 0)
        {
            symbols.Add(ConvertSimplePointFromLayers(cimPoint.SymbolLayers));
        }

        symbols.Reverse();
        return symbols.Count == 1
            ? symbols[0]
            : new SymbolCollection(symbols);
    }

    private SimplePointSymbol ConvertSimplePointFromLayers(List<CimSymbolLayer>? layers)
    {
        var fill   = layers?.OfType<CimSolidFill>().FirstOrDefault();
        var stroke = layers?.OfType<CimSolidStroke>().FirstOrDefault();

        var symbol = new SimplePointSymbol() { SymbolSmoothingMode = SymbolSmoothing.AntiAlias };
        if (fill?.Color != null)
        {
            ((IBrushColor)symbol).FillColor = ToArgbColor(fill.Color);
        }
        if (stroke?.Color != null)
        {
            ((IPenColor)symbol).PenColor  = ToArgbColor(stroke.Color);
            ((IPenWidth)symbol).PenWidth  = (float)stroke.Width;
        }
        return symbol;
    }

    private TrueTypeMarkerSymbol ConvertCharacterMarker(CimCharacterMarker marker)
    {
        var ttmSymbol = new TrueTypeMarkerSymbol() { SymbolSmoothingMode = SymbolSmoothing.AntiAlias };

        ttmSymbol.Charakter = new Charakter() { Value = (byte)marker.CharacterIndex };
        ttmSymbol.Font = gView.GraphicsEngine.Current.Engine.CreateFont(
            marker.FontFamilyName,
            (float)marker.Size);

        // Color: prefer direct color, fall back to nested symbol's first solid fill
        var markerColor = marker.Color
            ?? (marker.Symbol as CimPolygonSymbol)
                   ?.SymbolLayers?.OfType<CimSolidFill>().FirstOrDefault()?.Color;
        if (markerColor != null)
        {
            ttmSymbol.Color = ToArgbColor(markerColor);
        }

        if (marker.Rotation != 0)
        {
            ttmSymbol.Angle = (float)-marker.Rotation;
        }

        return ttmSymbol;
    }

    private ISymbol ConvertLineSymbol(CimLineSymbol cimLine)
    {
        // Convert every enabled stroke layer; collect into SymbolCollection if there are multiple
        var lineSymbols = cimLine.SymbolLayers?
            .OfType<CimSolidStroke>()
            .Where(s => s.Enable)
            .Select(ConvertSolidStroke)
            .ToList() ?? [];

        if (lineSymbols.Count == 0)
        {
            return new SimpleLineSymbol() { Smoothingmode = SymbolSmoothing.AntiAlias };
        }

        lineSymbols.Reverse();
        return lineSymbols.Count == 1
            ? lineSymbols[0]
            : new SymbolCollection(lineSymbols.Cast<ISymbol>());
    }

    private SimpleLineSymbol ConvertSolidStroke(CimSolidStroke stroke)
    {
        var line = new SimpleLineSymbol() { Smoothingmode = SymbolSmoothing.AntiAlias };

        if (stroke.Color != null)
        {
            ((IPenColor)line).PenColor = ToArgbColor(stroke.Color);
        }
        if (stroke.Width > 0)
        {
            ((IPenWidth)line).PenWidth = (float)stroke.Width;
        }
        if (stroke.Effects != null)
        {
            foreach (var unknown in stroke.Effects.OfType<CimUnknownGeometricEffect>())
            {
                _warn?.Invoke($"Layer '{_currentLayerName}': Not supported geometric effect {unknown.TypeName}. Effect will be ignored.");
            }

            var dashes = stroke.Effects.OfType<CimGeometricEffectDashes>().FirstOrDefault();
            if (dashes?.DashTemplate != null)
            {
                line.DashStyle = ResolveDashStyle(dashes.DashTemplate);
            }
        }

        return line;
    }

    /// <summary>
    /// Maps a CIM dash template (alternating dash/gap lengths) to the closest
    /// <see cref="LineDashStyle"/> value.
    /// </summary>
    private static LineDashStyle ResolveDashStyle(List<double> template)
    {
        // Normalise: count how many distinct segment types appear
        // Common patterns (values are proportional, not absolute):
        //   [dash, gap]              → Dash
        //   [dot, gap]               → Dot   (dash ≈ 0 or very short relative to gap)
        //   [dash, gap, dot, gap]    → DashDot
        //   [dash, gap, dot, gap, dot, gap] → DashDotDot
        return template.Count switch
        {
            2 => template[0] <= template[1] * 0.4   // very short dash = dot
                    ? LineDashStyle.Dot
                    : LineDashStyle.Dash,
            4 => LineDashStyle.DashDot,
            >= 6 => LineDashStyle.DashDotDot,
            _ => LineDashStyle.Dash
        };
    }

    private ISymbol ConvertPolygonSymbol(CimPolygonSymbol cimPoly)
    {
        var stroke = cimPoly.SymbolLayers?.OfType<CimSolidStroke>().FirstOrDefault();

        // Warn about unsupported fill types and fall back gracefully
        if (cimPoly.SymbolLayers != null)
        {
            foreach (var unsupported in cimPoly.SymbolLayers
                         .Where(l => l is CimPictureFill))
            {
                _warn?.Invoke($"Layer '{_currentLayerName}': Not supported symbol type {unsupported.GetType().Name.Replace("Cim", "CIM")}. Using SimpleFillSymbol instead...");
            }
        }

        // Collect all fill-type layers in document order (SolidFill + HatchFill + unsupported falls back to empty fill)
        var fillLayers = cimPoly.SymbolLayers?
            .Where(l => l is CimSolidFill or CimHatchFill or CimPictureFill)
            .ToList() ?? [];

        if (fillLayers.Count == 0)
        {
            // Stroke-only polygon
            var empty = new SimpleFillSymbol();
            ((IBrushColor)empty).FillColor = ArgbColor.Transparent;
            if (stroke != null) empty.OutlineSymbol = CreateOutlineLineSymbol(stroke);
            return empty;
        }

        // Build individual fill symbols. Outline stroke goes on the FIRST symbol only.
        var fillSymbols = new List<IFillSymbol>();
        for (int i = 0; i < fillLayers.Count; i++)
        {
            var outlineForThis = i == 0 ? stroke : null;
            IFillSymbol fs = fillLayers[i] switch
            {
                CimHatchFill hatch  => ConvertHatchFill(hatch, outlineForThis),
                CimSolidFill fill   => ConvertSolidFill(fill, outlineForThis),
                CimPictureFill      => ConvertSolidFill(null, outlineForThis),  // unsupported → plain fill
                _                   => ConvertSolidFill(null, outlineForThis)
            };
            fillSymbols.Add(fs);
        }

        if (fillSymbols.Count == 1)
        {
            return fillSymbols[0];
        }

        fillSymbols.Reverse();
        // Multiple fill layers → SymbolCollection
        return new SymbolCollection(fillSymbols.Cast<ISymbol>());
    }

    private SimpleFillSymbol ConvertSolidFill(CimSolidFill? fill, CimSolidStroke? stroke)
    {
        var symbol = new SimpleFillSymbol();

        if (fill?.Color != null)
        {
            ((IBrushColor)symbol).FillColor = ToArgbColor(fill.Color);
        }
        if (stroke != null)
        {
            symbol.OutlineSymbol = CreateOutlineLineSymbol(stroke);
        }

        return symbol;
    }

    private HatchSymbol ConvertHatchFill(CimHatchFill hatch, CimSolidStroke? outlineStroke)
    {
        var symbol = new HatchSymbol
        {
            HatchStyle = ResolveHatchStyle(hatch.Rotation),
            SymbolSmoothingMode = SymbolSmoothing.AntiAlias
        };

        // Hatch line color comes from the inner lineSymbol's first stroke
        var hatchStroke = hatch.LineSymbol?.SymbolLayers?.OfType<CimSolidStroke>().FirstOrDefault();
        if (hatchStroke?.Color != null)
        {
            symbol.ForeColor = ToArgbColor(hatchStroke.Color);
        }

        if (outlineStroke != null)
        {
            symbol.OutlineSymbol = CreateOutlineLineSymbol(outlineStroke);
        }

        return symbol;
    }

    /// <summary>
    /// Creates a <see cref="SimpleLineSymbol"/> from a <see cref="CimSolidStroke"/>
    /// for use as the <c>OutlineSymbol</c> of a fill symbol.
    /// </summary>
    private SimpleLineSymbol CreateOutlineLineSymbol(CimSolidStroke stroke)
        => ConvertSolidStroke(stroke);

    /// <summary>
    /// Maps a CIM hatch rotation angle (degrees) to the closest <see cref="HatchStyle"/>.
    /// CIM uses 0°=horizontal lines. gView HatchStyle uses Windows GDI conventions.
    /// </summary>
    private static HatchStyle ResolveHatchStyle(double rotation)
    {
        // Normalise to 0-179°
        var r = ((rotation % 180) + 180) % 180;

        return r switch
        {
            < 10 or >= 170      => HatchStyle.Horizontal,          //   0° – horizontal
            >= 10  and < 35     => HatchStyle.LightDownwardDiagonal,// ~22°
            >= 35  and < 55     => HatchStyle.ForwardDiagonal,      //  45°
            >= 55  and < 80     => HatchStyle.DarkDownwardDiagonal, // ~67°
            >= 80  and < 100    => HatchStyle.Vertical,             //  90°
            >= 100 and < 125    => HatchStyle.LightUpwardDiagonal,  // ~112°
            >= 125 and < 145    => HatchStyle.BackwardDiagonal,     // 135°
            _                   => HatchStyle.DarkUpwardDiagonal    // ~157°
        };
    }

    // -----------------------------------------------------------------------
    // Color conversion
    // -----------------------------------------------------------------------

    private static ArgbColor ToArgbColor(CimColor cimColor)
    {
        byte a = cimColor.AlphaByte;

        return cimColor switch
        {
            CimRgbColor rgb => ArgbColor.FromArgb(a, Clamp(rgb.R), Clamp(rgb.G), Clamp(rgb.B)),
            CimGrayColor gray => ArgbColor.FromArgb(a, Clamp(gray.Level), Clamp(gray.Level), Clamp(gray.Level)),
            CimCmykColor cmyk => CmykToArgb(a, cmyk.C, cmyk.M, cmyk.Y, cmyk.K),
            CimHsvColor hsv => HsvToArgb(a, hsv.H, hsv.S, hsv.V),
            _ => ArgbColor.Gray
        };
    }

    private static byte Clamp(double value) => (byte)Math.Clamp(Math.Round(value), 0, 255);

    private static ArgbColor CmykToArgb(byte a, double c, double m, double y, double k)
    {
        double r = 255 * (1 - c / 100.0) * (1 - k / 100.0);
        double g = 255 * (1 - m / 100.0) * (1 - k / 100.0);
        double b = 255 * (1 - y / 100.0) * (1 - k / 100.0);
        return ArgbColor.FromArgb(a, Clamp(r), Clamp(g), Clamp(b));
    }

    private static ArgbColor HsvToArgb(byte a, double h, double s, double v)
    {
        s /= 100.0;
        v /= 100.0;
        double c = v * s;
        double x = c * (1 - Math.Abs(h / 60.0 % 2 - 1));
        double m = v - c;

        double r = 0, g = 0, b = 0;
        if (h < 60) { r = c; g = x; }
        else if (h < 120) { r = x; g = c; }
        else if (h < 180) { g = c; b = x; }
        else if (h < 240) { g = x; b = c; }
        else if (h < 300) { r = x; b = c; }
        else { r = c; b = x; }

        return ArgbColor.FromArgb(a, Clamp((r + m) * 255), Clamp((g + m) * 255), Clamp((b + m) * 255));
    }
}

/// <summary>
/// Identifies the dataset plugin (by GUID) and its connection string to use
/// for all imported feature classes instead of the default <see cref="UnknownFeatureDataset"/>.
/// </summary>
internal sealed record DatasetPluginOptions(Guid PluginGuid, string ConnectionString);

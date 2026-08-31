using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;
using gView.DataSources.Unknown;
using gView.Framework.Cartography;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Symbology;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.Symbology.Models;
using gView.GraphicsEngine;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx;

/// <summary>
/// Converts a parsed <see cref="AprxMapResult"/> (CIM model) into a gView <see cref="Map"/>
/// with <see cref="FeatureLayer"/> objects, renderers, label renderers and definition queries.
/// </summary>
internal class AprxMapConverter
{
    private readonly Action<string>? _warn;
    private readonly Action<string>? _info;
    private string _currentLayerName = string.Empty;
    private readonly DatasetPluginOptions? _datasetOptions;
    // Pool of open datasets keyed by their effective connection string (after {dbname} substitution)
    private readonly Dictionary<string, IDataset> _datasetPool = new(StringComparer.OrdinalIgnoreCase);
    // Which label engine the current map uses (set once per Convert() call) - every
    // CimLabelClass carries placement properties for both engines regardless of which one is
    // actually active, so ConvertLabelRenderer needs to know which block to read.
    private bool _useMaplexLabelEngine = true;
    // Counts down from -1 to hand out a distinct placeholder ID for every layer whose CIM
    // serviceLayerID is unresolved (-1) - see ResolveServiceLayerId.
    private int _nextPlaceholderLayerId = -1;
    // The current layer's own "transparency" (0 = opaque .. 100 = fully invisible, ArcGIS
    // Pro's Layer Properties -> Display slider) - set once per layer before converting its
    // renderer/labels, then applied to every color ToArgbColor produces for that layer. This
    // is separate from, and multiplies with, whatever alpha a symbol's own color already has.
    // Left at 0 for a layer using CompositionMode.Copy instead (see ShouldUseCompositionModeCopy) -
    // there the *colors* stay fully opaque and the transparency is applied once, to the whole
    // rendered layer, instead.
    private double _currentLayerTransparency;
    // Layer-name wildcard patterns (see Wildcard.WildcardEx - "*"/"?", case-insensitive) opting
    // a transparent layer into CompositionMode.Copy instead of baking its transparency into every
    // symbol color. Baking it into colors is wrong whenever the layer's own features can overlap
    // themselves (e.g. many crossing semi-transparent line/polygon symbols at the same
    // transparency): each overlap gets drawn/blended twice, producing a visibly darker seam that
    // doesn't exist in ArcGIS Pro (which always composites a layer once, then applies its
    // transparency to the whole result - exactly what CompositionMode.Copy does). Not applied by
    // default because rendering to an extra full-size bitmap first costs real memory/CPU per
    // matched layer - opt in by layer name for the ones that actually show the artifact.
    private readonly List<WildcardEx> _compositionModeCopyLayerPatterns = [];
    // Target LabelPriority for a label class that has ArcGIS Pro's "Allow overlapping labels"
    // checked - see the comment at its one use site in ConvertLabelRenderer for why this isn't
    // just hard-coded to RenderLabelPriority.Always.
    private readonly RenderLabelPriority _allowOverlappingLabelsPriority;

    /// <param name="warn">Optional callback invoked for non-fatal conversion warnings.</param>
    /// <param name="info">Optional callback invoked for informational conversion notices (e.g. a label expression that was successfully translated).</param>
    /// <param name="datasetPlugin">When supplied, all imported feature classes are bound to this dataset instead of <see cref="UnknownFeatureDataset"/>.</param>
    /// <param name="allowOverlappingLabelsPriority">
    /// gView <see cref="RenderLabelPriority"/> to assign a label class that has ArcGIS Pro's
    /// "Allow overlapping labels" checked (Standard engine). Defaults to
    /// <see cref="RenderLabelPriority.Always"/> (matches the ArcGIS Pro checkbox's own name most
    /// closely), but that skips gView's overlap check entirely - unlike ArcGIS Pro, which still
    /// tries a normal placement first and only allows overlap as a fallback - so a busy layer can
    /// come out visibly noisier than in ArcGIS Pro. Pass e.g. <see cref="RenderLabelPriority.High"/>
    /// for a gentler equivalent (checked, but preferred over Normal/Low priority labels).
    /// </param>
    /// <param name="compositionModeCopyLayerPatterns">
    /// Layer-name wildcard patterns ("*"/"?", case-insensitive) opting a transparent layer into
    /// <see cref="FeatureLayerCompositionMode.Copy"/> instead of the default (baking the layer's
    /// "transparency" into every symbol color, which produces a visibly darker seam wherever the
    /// layer's own features overlap themselves - not present in ArcGIS Pro). Only layers whose
    /// name matches, and that have a non-zero aprx "transparency", are affected; everything else
    /// keeps the previous behaviour unchanged.
    /// </param>
    public AprxMapConverter(
        Action<string>? warn = null,
        Action<string>? info = null,
        DatasetPluginOptions? datasetPlugin = null,
        RenderLabelPriority allowOverlappingLabelsPriority = RenderLabelPriority.Always,
        IEnumerable<string>? compositionModeCopyLayerPatterns = null)
    {
        _warn = warn;
        _info = info;
        _datasetOptions = datasetPlugin;
        _allowOverlappingLabelsPriority = allowOverlappingLabelsPriority;

        if (compositionModeCopyLayerPatterns != null)
        {
            foreach (var pattern in compositionModeCopyLayerPatterns)
            {
                if (!string.IsNullOrWhiteSpace(pattern))
                {
                    _compositionModeCopyLayerPatterns.Add(new WildcardEx(pattern.Trim(), System.Text.RegularExpressions.RegexOptions.IgnoreCase));
                }
            }
        }
    }

    /// <summary>
    /// True when <paramref name="layerName"/> matches one of <see cref="_compositionModeCopyLayerPatterns"/>
    /// and <paramref name="transparency"/> is actually non-zero (a matching pattern is a no-op
    /// for an already-opaque layer).
    /// </summary>
    private bool ShouldUseCompositionModeCopy(string? layerName, double transparency)
    {
        if (transparency <= 0 || string.IsNullOrEmpty(layerName) || _compositionModeCopyLayerPatterns.Count == 0)
        {
            return false;
        }

        return _compositionModeCopyLayerPatterns.Any(p => p.IsMatch(layerName));
    }

    private IFeatureClass CreateFeatureClassFromPlugin(string rawFcName, string? workspaceConnectionString)
    {
        // --- 0. Parse "dbname.schema.tablename" ---
        // SDE names can be: tablename | schema.tablename | dbname.schema.tablename
        // gView does not use the dbname part in element names.
        var parts = rawFcName.Split('.');
        string dbNameFromFcName = parts.Length >= 3 ? parts[0] : string.Empty;
        // gView name: strip the leading dbname if present
        string gviewName = parts.Length >= 3
            ? string.Join(".", parts.Skip(1))   // schema.tablename
            : rawFcName;

        // --- Build effective connection string ---
        // Placeholders in --connection-string are resolved from this *layer's own* aprx
        // workspace connection properties (SERVER, INSTANCE, DATABASE, DBCLIENT, USER, ...) -
        // not every layer in an aprx necessarily comes from the same database/server, so this
        // is done per layer rather than once globally. "{dbname}" is a legacy alias: it prefers
        // the database name embedded in the qualified feature class name
        // ("dbname.schema.table" - historically the only source of it) but falls back to the
        // connection string's own DATABASE property, since not every aprx qualifies names that
        // way.
        var connectionProperties = ParseWorkspaceConnectionProperties(workspaceConnectionString);
        var dbName = !string.IsNullOrEmpty(dbNameFromFcName)
            ? dbNameFromFcName
            : connectionProperties.GetValueOrDefault("DATABASE", string.Empty);
        if (!string.IsNullOrEmpty(dbName))
        {
            connectionProperties["dbname"] = dbName;
        }

        string effectiveCs = ApplyConnectionStringPlaceholders(_datasetOptions!.ConnectionString, connectionProperties);

        // --- 1. Get or create a dataset for this effective connection string ---
        if (!_datasetPool.TryGetValue(effectiveCs, out var dataset))
        {
            var pluginManager = new PlugInManager();
            dataset = pluginManager.CreateInstance(_datasetOptions.PluginGuid) as IDataset
                ?? throw new InvalidOperationException(
                    $"Plugin '{_datasetOptions.PluginGuid}' could not be instantiated as IDataset.");

            dataset.SetConnectionString(effectiveCs).GetAwaiter().GetResult();
            dataset.Open().GetAwaiter().GetResult();
            _datasetPool[effectiveCs] = dataset;
        }

        // --- 2. Resolve the element name the dataset actually knows ---
        // Try the full gView name first (schema.tablename), then just the table name.
        var element = dataset.Element(gviewName).GetAwaiter().GetResult();
        if (element == null && gviewName != rawFcName)
        {
            // Already stripped dbname above; now also try bare table name (no schema)
            var tableOnly = parts.Last();
            element = dataset.Element(tableOnly).GetAwaiter().GetResult();
            if (element != null)
            {
                gviewName = tableOnly;
            }
        }

        if (element?.Class is IFeatureClass fc)
        {
            return fc;
        }

        // Fallback: element not found in dataset — use a placeholder bound to the dataset
        if (element == null)
        {
            _warn?.Invoke($"Layer '{_currentLayerName}': element '{gviewName}' not found in dataset. Using placeholder.");
        }

        return new UnknownFeatureClass(dataset, gviewName);
    }

    private static readonly System.Text.RegularExpressions.Regex _connectionStringPlaceholderPattern =
        new(@"\{([A-Za-z0-9_]+)\}", System.Text.RegularExpressions.RegexOptions.Compiled);

    // Placeholder names already warned about (so a typo in --connection-string, e.g. "{passwrd}",
    // produces one warning total instead of one per layer that hits it).
    private readonly HashSet<string> _warnedUnresolvedConnectionPlaceholders = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parses an ArcGIS workspace connection string (e.g. "SERVER=host;DATABASE=db;USER=me;...")
    /// into a case-insensitive key/value lookup. Returns an empty (but mutable) dictionary for
    /// null/empty input.
    /// </summary>
    internal static Dictionary<string, string> ParseWorkspaceConnectionProperties(string? workspaceConnectionString)
    {
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(workspaceConnectionString))
        {
            return properties;
        }

        foreach (var part in workspaceConnectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = part.IndexOf('=');
            if (eq <= 0)
            {
                continue;
            }

            var key = part[..eq].Trim();
            var value = part[(eq + 1)..].Trim();
            if (key.Length > 0)
            {
                properties[key] = value;
            }
        }

        return properties;
    }

    /// <summary>
    /// Replaces every "{key}" placeholder in <paramref name="template"/> (--connection-string)
    /// with the matching value from <paramref name="properties"/> - matched case-insensitively,
    /// so "{server}"/"{Server}"/"{SERVER}" all resolve the same way. A placeholder with no
    /// matching property (e.g. a typo, or a property this particular layer's aprx connection
    /// simply doesn't have) is left as literal text and warned about once.
    /// </summary>
    internal string ApplyConnectionStringPlaceholders(string template, Dictionary<string, string> properties)
    {
        return _connectionStringPlaceholderPattern.Replace(template, m =>
        {
            var key = m.Groups[1].Value;
            if (properties.TryGetValue(key, out var value))
            {
                return value;
            }

            if (_warnedUnresolvedConnectionPlaceholders.Add(key))
            {
                _warn?.Invoke($"--connection-string placeholder '{{{key}}}' has no matching value in the aprx's workspace connection - left as-is.");
            }
            return m.Value;
        });
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

        // Map units: derive from the resolved spatial reference instead of assuming Meters -
        // wrong units silently break ReferenceScale-based symbol scaling below.
        var resolvedUnit = map.LayerDefaultSpatialReference?.SpatialParameters?.Unit;
        map.Display.DisplayUnits = map.Display.MapUnits =
            (resolvedUnit is null or GeoUnits.Unknown) ? GeoUnits.Meters : resolvedUnit.Value;

        // ArcGIS Pro only ties symbol/text sizes to ground distance (so they visually grow/
        // shrink as you zoom) when the author explicitly sets a reference scale (Map
        // Properties -> General -> Reference Scale). Without one, symbols keep a constant
        // page size at every map scale. Mirror that: use the CIM's referenceScale when
        // present, otherwise 0 disables gView's reference-scale symbol scaling entirely
        // (Display.ReferenceScale <= 0) instead of forcing an arbitrary scale that would make
        // symbols render at a different size than in ArcGIS Pro.
        map.Display.ReferenceScale = cimMap.ReferenceScale ?? 0;

        // ArcGIS Pro's actual default (when the map has no explicit generalPlacementProperties
        // at all) is the modern Maplex engine; only an explicit "...Standard..." type means the
        // author is on the older engine.
        _useMaplexLabelEngine = !string.Equals(
            cimMap.GeneralPlacementProperties?.Type,
            "CIMStandardGeneralPlacementProperties",
            StringComparison.OrdinalIgnoreCase);

        // Add layers in the same order as in the APRX so the TOC matches
        foreach (var cimLayer in mapResult.Layers)
        {
            AddLayer(map, cimLayer);
        }

        ResolveUnassignedLayerIds(map);

        return map;
    }

    /// <summary>
    /// ArcGIS Pro sometimes never resolves a layer's service layer ID in the aprx itself
    /// (serviceLayerID = -1) - observed for annotation sub-layers, whose real ID is assigned by
    /// ArcGIS Server only at actual publish time and never written back into the project file.
    /// Leaving -1 in the converted map would be wrong, so for every such layer: warn (the
    /// number below is a best-effort guess, not a value that was actually in the aprx) and
    /// assign the first free ID starting at its parent group's ID + 1 - matching how ArcGIS
    /// Server lays a group's own children out immediately after it - while never reusing an ID
    /// another layer (original or already-resolved-this-way) already has.
    /// </summary>
    /// <summary>
    /// Returns <paramref name="cimServiceLayerId"/> unchanged when ArcGIS Pro actually resolved
    /// it, otherwise a distinct placeholder (a unique negative number - never a real ID) that
    /// <see cref="ResolveUnassignedLayerIds"/> replaces with a real one once every layer has
    /// been added. Assigning the literal -1 to more than one layer must be avoided here: since
    /// <see cref="Map.AddLayer(gView.Framework.Core.Data.ILayer)"/> already renumbers a layer
    /// on the spot the moment its ID collides with one already in the map (Map.SetNewLayerID),
    /// a second "-1" layer would immediately get silently reassigned via the framework's own
    /// generic sequence *before* ResolveUnassignedLayerIds ever sees it - defeating the
    /// parent-relative numbering below and making the outcome depend on unrelated add order.
    /// </summary>
    private int ResolveServiceLayerId(int cimServiceLayerId) =>
        cimServiceLayerId >= 0 ? cimServiceLayerId : _nextPlaceholderLayerId--;

    /// <summary>
    /// ArcGIS Pro sometimes never resolves a layer's service layer ID in the aprx itself
    /// (serviceLayerID = -1) - observed for annotation sub-layers, whose real ID is assigned by
    /// ArcGIS Server only at actual publish time and never written back into the project file.
    /// Leaving that in the converted map would be wrong, so for every such layer (recognizable
    /// by the negative placeholder <see cref="ResolveServiceLayerId"/> gave it): warn (the
    /// number below is a best-effort guess, not a value that was actually in the aprx) and
    /// assign the first free ID starting at its parent group's ID + 1 - matching how ArcGIS
    /// Server lays a group's own children out immediately after it - while never reusing an ID
    /// another layer (original or already-resolved-this-way) already has.
    /// </summary>
    private void ResolveUnassignedLayerIds(Map map)
    {
        var usedIds = new HashSet<int>(map.MapElements.Where(e => e.ID >= 0).Select(e => e.ID));

        foreach (var element in map.MapElements)
        {
            if (element.ID >= 0)
            {
                continue;
            }

            var layer = element as ILayer;
            var parentId = layer?.GroupLayer?.ID ?? -1;
            var candidate = parentId >= 0 ? parentId + 1 : 0;

            while (usedIds.Contains(candidate))
            {
                candidate++;
            }

            var name = (layer != null ? map.TOC.GetTOCElement(layer)?.Name : null) ?? element.Title;
            _warn?.Invoke($"Layer '{name}': the aprx never resolved a service layer ID for this layer (serviceLayerID = -1) - assigned {candidate} instead. Verify this against the actual published service.");

            element.ID = candidate;
            usedIds.Add(candidate);
        }
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

            case CimAnnotationLayer annotation:
                AddAnnotationLayer(map, null, annotation);
                break;

            default:
                // A layer type not in CimBaseLayer's [JsonDerivedType] list (e.g.
                // CIMRasterLayer) deserializes down to the bare base type instead of null -
                // warn instead of letting it silently vanish. (The concrete type name isn't
                // recoverable here - CimBaseLayer can't carry its own "type" property without
                // conflicting with System.Text.Json's discriminator handling for this exact
                // hierarchy. The other route layers take, each stored as its own standalone
                // JSON file - ArcGIS Pro 3.x - *does* report the type name, in
                // AprxReader.WarnUnsupportedLayer.)
                _warn?.Invoke($"Layer '{cimLayer.Name ?? "?"}': unsupported layer type - not included in the converted map.");
                break;
        }
    }

    private void AddGroupLayer(Map map, CimGroupLayer cimGroup)
    {
        var groupLayer = new GroupLayer(cimGroup.Name ?? string.Empty)
        {
            Visible = cimGroup.Visibility,
            MinimumScale = cimGroup.MaxScale,
            MaximumScale = cimGroup.MinScale,
            MinimumLabelScale = cimGroup.MaxScale,
            MaximumLabelScale = cimGroup.MinScale
        };

        groupLayer.ID = ResolveServiceLayerId(cimGroup.ServiceLayerId);

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
                        Visible = cimGroup.Visibility,
                        MinimumScale = cimGroup.MaxScale,
                        MaximumScale = cimGroup.MinScale,
                        MinimumLabelScale = cimGroup.MaxScale,
                        MaximumLabelScale = cimGroup.MinScale
                    };

                    childGroup.ID = ResolveServiceLayerId(cimGroup.ServiceLayerId);

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

            case CimAnnotationLayer cimAnnotation:
                AddAnnotationLayer(map, parentGroupLayer, cimAnnotation);
                break;
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
            Visible = cimGroup.Visibility,
            MinimumScale = cimGroup.MaxScale,
            MaximumScale = cimGroup.MinScale,
            MinimumLabelScale = cimGroup.MaxScale,
            MaximumLabelScale = cimGroup.MinScale
        };

        groupLayer.ID = ResolveServiceLayerId(cimGroup.ServiceLayerId);

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

        var useCompositionModeCopy = ShouldUseCompositionModeCopy(cimFeature.Name, cimFeature.Transparency);
        // Colors stay fully opaque here (0) when using CompositionMode.Copy - the transparency
        // is applied once below, to the whole rendered layer, instead of baked into every color.
        _currentLayerTransparency = useCompositionModeCopy ? 0 : cimFeature.Transparency;

        IFeatureClass featureClass;
        if (_datasetOptions != null)
        {
            featureClass = CreateFeatureClassFromPlugin(
                cimFeature.FeatureTable?.DataConnection?.Dataset ?? string.Empty,
                cimFeature.FeatureTable?.DataConnection?.WorkspaceConnectionString);
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
            MinimumScale = cimFeature.MaxScale,
            MaximumScale = cimFeature.MinScale,
            MinimumLabelScale = cimFeature.MaxScale,
            MaximumLabelScale = cimFeature.MinScale,
            // ArcGIS Pro's "Scale symbols when a reference scale is set" checkbox - whether
            // this layer's symbol/label sizes track the map's reference scale as it's zoomed,
            // or always render at a constant screen size. gView's FeatureLayer defaults both
            // of these to true regardless of the aprx, so without this every layer would scale
            // with the reference scale even where ArcGIS Pro has that turned off.
            ApplyRefScale = cimFeature.ScaleSymbols,
            ApplyLabelRefScale = cimFeature.ScaleSymbols,
        };

        if (useCompositionModeCopy)
        {
            layer.CompositionMode = FeatureLayerCompositionMode.Copy;
            layer.CompositionModeCopyTransparency = (float)cimFeature.Transparency;
        }

        layer.ID = ResolveServiceLayerId(cimFeature.ServiceLayerId);

        if (cimFeature.FeatureTable is not null)
        {
            // Definition query: featureTable.definitionExpression takes priority
            var definitionExpression = cimFeature.FeatureTable.DefinitionExpression
                ?? cimFeature.DefinitionExpression;

            if (!string.IsNullOrWhiteSpace(definitionExpression))
            {
                layer.FilterQuery = new QueryFilter
                {
                    WhereClause = definitionExpression
                };
            }

            foreach (var fieldDescription in cimFeature.FeatureTable.FieldDescriptions ?? [])
            {
                var field = layer.Fields.FindField(fieldDescription.FieldName) as Field;
                if (field is null) continue;

                field.visible = fieldDescription.Visible;
                field.aliasname = fieldDescription.Alias;
            }
        }

        // Feature renderer
        if (cimFeature.Renderer != null)
        {
            if (RendererIsFullyTransparent(cimFeature.Renderer))
            {
                // Deliberately invisible "anchor" symbol (every color alpha 0) - a common
                // real-world pattern for label-only layers (a point feature that exists purely
                // to carry a label, e.g. NS-Kasten-Beschriftung; see BuildTextSymbol's contrast
                // fallback for another symptom of the same authoring style). Rendering it would
                // cost real time for zero visible output, so this leaves FeatureRenderer unset -
                // same as gView's own "Render features for this layer" checkbox being off -
                // rather than assigning a renderer that always draws nothing. Deliberately left
                // as-is rather than "fixed": RenderFeatureLayer only calls AddBlockingGeometry
                // when FeatureRenderer is set, so an invisible feature also correctly stops
                // blocking other labels, matching what an invisible feature should do.
                _info?.Invoke($"Layer '{cimFeature.Name}': renderer symbol is fully transparent (alpha 0) - feature rendering left off for this layer (its labels, if any, are unaffected).");
            }
            else
            {
                layer.FeatureRenderer = ConvertRenderer(cimFeature.Renderer);
                if (layer.FeatureRenderer is null)
                {
                    _warn?.Invoke($"Layer '{cimFeature.Name}': renderer type '{cimFeature.Renderer.GetType().Name}' could not be converted and was skipped.");
                }
            }
        }

        // Selection highlight symbol - same CimSymbolReference shape as a simple renderer's
        // own symbol, so it's wrapped in a SimpleRenderer the same way ConvertSimpleRenderer
        // does. Without this, gView falls back to its own default selection color/symbol
        // instead of the one authored in ArcGIS Pro.
        var selectionSymbol = ConvertSymbolReference(cimFeature.SelectionSymbol);
        if (selectionSymbol != null)
        {
            layer.SelectionRenderer = new SimpleRenderer { Symbol = selectionSymbol };
        }

        // Label renderer
        if (cimFeature.LabelVisibility && cimFeature.LabelClasses?.Count > 0)
        {
            layer.LabelRenderer = ConvertLabelRenderer(cimFeature.LabelClasses[0], DetermineRendererGeometryKind(cimFeature.Renderer));
        }

        // ArcGIS Pro's per-label-class "Feature weight" (Label Priority Ranking / Placement
        // Properties) - how strongly this layer's own feature geometry blocks *other* labels
        // (from any layer) from being placed on top of it, e.g. so a symbol never gets covered
        // by a neighbouring label. Read independently of LabelVisibility above: ArcGIS Pro keeps
        // this setting per label class regardless of whether the layer's own labels are shown.
        //
        // Maplex only: gated by EnableFeatureWeight, which ArcGIS Pro must set to true for the
        // weight to actually apply. No real aprx seen so far sets it (it's absent from the JSON
        // entirely, defaulting to false) - so this is currently null for every real file
        // available, matching Esri's own documented default (off) rather than inferring a signal
        // ArcGIS Pro isn't actually emitting. Standard has no confirmed equivalent "does this
        // feature's geometry block other labels" concept, so it isn't used for
        // FeatureLabelPriority here (contrast with LabelPriority below, which reuses the same
        // featureWeight value for both engines, ungated, for the label's own priority tier).
        var firstLabelClass = cimFeature.LabelClasses?.FirstOrDefault();
        layer.FeatureLabelPriority = _useMaplexLabelEngine
            && firstLabelClass?.MaplexLabelPlacementProperties?.EnableFeatureWeight == true
                ? MapMaplexFeatureWeight(firstLabelClass?.MaplexLabelPlacementProperties?.FeatureWeight)
                : null;

        return layer;
    }

    /// <summary>
    /// ArcGIS Pro/Server publish an annotation layer not as one flat layer but as a group
    /// named after the layer itself, containing one child layer per annotation class
    /// ("Standard" unless the author defined more) - e.g. "FW-Text" containing "Standard".
    /// <paramref name="parentGroupLayer"/> is the group this annotation layer itself is nested
    /// under in the aprx, if any (null for a top-level layer).
    /// </summary>
    private void AddAnnotationLayer(Map map, GroupLayer? parentGroupLayer, CimAnnotationLayer cimAnnotation)
    {
        var groupLayer = new GroupLayer(cimAnnotation.Name ?? string.Empty)
        {
            Visible = cimAnnotation.Visibility,
            MinimumScale = cimAnnotation.MaxScale,
            MaximumScale = cimAnnotation.MinScale,
            MinimumLabelScale = cimAnnotation.MaxScale,
            MaximumLabelScale = cimAnnotation.MinScale,
            // So the GeoServices REST interface reports this group's "type" as "Annotation
            // Layer" (matching ArcGIS Server) instead of the default "Group Layer" - clients
            // that specifically key off that string need it to keep working the same as
            // against the original ArcGIS Server service. See GeoServicesRestController.JsonLayer.
            MapServerStyle = MapServerGrouplayerStyle.EsriAnnotationLayer
        };

        groupLayer.ID = ResolveServiceLayerId(cimAnnotation.ServiceLayerId);

        parentGroupLayer?.Add(groupLayer);
        // Add the group to the map before its children - AddLayer's child logic below needs
        // the parent's TOC element to already exist.
        map.AddLayer(groupLayer);
        map.TOC.GetTOCElement(groupLayer)?.Name = cimAnnotation.Name;

        var subLayers = cimAnnotation.SubLayers is { Count: > 0 }
            ? cimAnnotation.SubLayers
            // Defensive fallback for a CIM that (unexpectedly) has no sub-layers: still
            // produce one usable layer instead of nothing, just without the ArcGIS Server
            // group/"Standard" nesting or an AnnotationClassID filter.
            : [new CimAnnotationSubLayer { Name = cimAnnotation.Name, ServiceLayerId = cimAnnotation.ServiceLayerId }];

        foreach (var subLayer in subLayers)
        {
            var childLayer = CreateAnnotationSubLayer(cimAnnotation, subLayer);
            groupLayer.Add(childLayer);
            map.AddLayer(childLayer);
            map.TOC.GetTOCElement(childLayer)?.Name = subLayer.Name;
        }
    }

    /// <summary>
    /// Builds the layer for one annotation class ("sub-layer"). ArcGIS Pro annotation layers
    /// have no CIM renderer/labelClasses of their own - each feature stores its own rendered
    /// text via plain attribute columns (the older, field-based annotation schema: TextString/
    /// Angle/FontName/FontSize/...; not the newer per-feature binary graphic overrides some
    /// annotation feature classes use instead, which this doesn't attempt to decode). Converts
    /// to a regular <see cref="FeatureLayer"/> - with no feature renderer, so the (usually
    /// invisible bounding) geometry itself never draws - plus a label renderer built from those
    /// columns instead of a CIM label class.
    /// </summary>
    private FeatureLayer CreateAnnotationSubLayer(CimAnnotationLayer cimAnnotation, CimAnnotationSubLayer subLayer)
    {
        _currentLayerName = $"{cimAnnotation.Name}/{subLayer.Name}";
        _currentLayerTransparency = cimAnnotation.Transparency;

        IFeatureClass featureClass;
        if (_datasetOptions != null)
        {
            featureClass = CreateFeatureClassFromPlugin(
                cimAnnotation.FeatureTable?.DataConnection?.Dataset ?? string.Empty,
                cimAnnotation.FeatureTable?.DataConnection?.WorkspaceConnectionString);
        }
        else
        {
            featureClass = new UnknownFeatureClass(
                new UnknownFeatureDataset()
                {
                    ConnectionString = cimAnnotation.FeatureTable?.DataConnection?.WorkspaceConnectionString ?? ""
                },
                cimAnnotation.FeatureTable?.DataConnection?.Dataset ?? string.Empty
            );
        }

        var layer = new FeatureLayer(featureClass)
        {
            Visible = subLayer.Visibility,
            Title = featureClass.Name,
            MinimumScale = cimAnnotation.MaxScale,
            MaximumScale = cimAnnotation.MinScale,
            MinimumLabelScale = cimAnnotation.MaxScale,
            MaximumLabelScale = cimAnnotation.MinScale,
            // See CreateFeatureLayer - same "scale symbols/labels with reference scale" flag,
            // just read from the annotation layer since sub-layers don't carry their own copy.
            ApplyRefScale = cimAnnotation.ScaleSymbols,
            ApplyLabelRefScale = cimAnnotation.ScaleSymbols,
        };

        layer.ID = ResolveServiceLayerId(subLayer.ServiceLayerId);

        // Combine the layer-level definition query (if any) with a filter restricting this
        // sub-layer to its own annotation class - otherwise, with more than one annotation
        // class sharing the same feature class, every sub-layer would render every feature.
        var whereClauses = new List<string>();
        if (!string.IsNullOrWhiteSpace(cimAnnotation.FeatureTable?.DefinitionExpression))
        {
            whereClauses.Add(cimAnnotation.FeatureTable!.DefinitionExpression!);
        }
        if (int.TryParse(subLayer.SubLayerId, out var annotationClassId))
        {
            whereClauses.Add($"AnnotationClassID = {annotationClassId}");
        }
        if (whereClauses.Count > 0)
        {
            layer.FilterQuery = new QueryFilter
            {
                WhereClause = string.Join(" AND ", whereClauses)
            };
        }

        foreach (var fieldDescription in cimAnnotation.FeatureTable?.FieldDescriptions ?? [])
        {
            var field = layer.Fields.FindField(fieldDescription.FieldName) as Field;
            if (field is null) continue;

            field.visible = fieldDescription.Visible;
            field.aliasname = fieldDescription.Alias;
        }

        layer.LabelRenderer = BuildAnnotationLabelRenderer();

        return layer;
    }

    /// <summary>
    /// Builds the label renderer standing in for an annotation layer's per-feature text.
    /// Text and rotation are field-driven (the "TextString"/"Angle" columns every field-based
    /// annotation feature class has); font is a fixed default for the whole layer, since
    /// gView's label renderer doesn't currently apply per-feature font overrides at draw time
    /// even though it has FontField/SizeFieldName properties (they're only used to include
    /// those columns in the query, not to vary the rendered font) - so per-row FontName/
    /// FontSize/Bold/Italic/Underline/XOffset/YOffset from the CIM schema aren't reproduced.
    /// LabelPriority is "Always" (no overlap-avoidance repositioning): annotation, unlike a
    /// dynamic label, was deliberately placed exactly where it is by whoever authored it.
    /// </summary>
    private SimpleLabelRenderer BuildAnnotationLabelRenderer()
    {
        return new SimpleLabelRenderer
        {
            FieldName = "TextString",
            LabelPriority = RenderLabelPriority.Always,
            TextSymbol = new SimpleTextSymbol
            {
                Font = gView.GraphicsEngine.Current.Engine.CreateFont("Arial", 10f),
                Color = ApplyLayerTransparency(ArgbColor.Black)
            },
            SymbolRotation = new SymbolRotation
            {
                RotationFieldName = "Angle",
                RotationType = RotationType.ArithmeticMinus90,
                RotationUnit = RotationUnit.deg
            }
        };
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
        TryApplyRotation(renderer, cimSimple);
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

        TryApplyRotation(renderer, cimUnique);
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

                    foreach (var values in cls.Values ?? [])  // there can be more than one 
                    {
                        // Build the composite key: "val1|val2|val3" (same format as ManyValueMapRenderer.GetKey)
                        var key = values is { } uv
                            ? string.Join("|", uv.FieldValues?
                                .Select(v => "<Null>".Equals(v, StringComparison.OrdinalIgnoreCase)   // ESRI Null is Empty...
                                     ? ""
                                     : v) ?? [])
                            : null;

                        if (!string.IsNullOrEmpty(key))
                            renderer[key] = (ISymbol)symbol.Clone();
                    }
                }
            }
        }

        TryApplyRotation(renderer, cimUnique);
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
        TryApplyRotation(renderer, cimBreaks);
        return renderer;
    }

    /// <summary>
    /// Reads <c>visualVariables</c> from <paramref name="cimRenderer"/>, finds the first
    /// <see cref="CimRotationVisualVariable"/> whose Z-axis info has a non-empty expression,
    /// extracts the field name (pattern <c>[FieldName]</c>), and sets it on the renderer's
    /// <see cref="SymbolRotation"/>. Emits a warning if the expression cannot be mapped to a
    /// simple field name.
    /// </summary>
    private void TryApplyRotation(dynamic renderer, CimRenderer cimRenderer)
    {
        var rotVar = cimRenderer.VisualVariables
            ?.OfType<CimRotationVisualVariable>()
            .FirstOrDefault(v => !string.IsNullOrEmpty(v.VisualVariableInfoZ?.Expression));

        if (rotVar == null) return;

        var expression = rotVar.VisualVariableInfoZ!.Expression!;

        // Simple field reference: "[FieldName]"
        var match = System.Text.RegularExpressions.Regex.Match(expression, @"^\[([^\]]+)\]$");
        if (!match.Success)
        {
            _warn?.Invoke($"Layer '{_currentLayerName}': rotation expression '{expression}' cannot be mapped to a simple field name and will be ignored.");
            return;
        }

        var fieldName = match.Groups[1].Value;

        renderer.SymbolRotation = new SymbolRotation
        {
            RotationFieldName = fieldName,
            RotationType = MapCimRotationTypeForMarker(rotVar.RotationTypeZ),
            RotationUnit = RotationUnit.deg
        };
    }

    /// <summary>
    /// Maps a CIM rotation-angle convention ("Arithmetic" or "Geographic") to gView's
    /// <see cref="RotationType"/> for rotating a <b>marker/point symbol</b> (e.g.
    /// TrueTypeMarkerSymbol via a renderer's rotation visual variable). Many point marker glyphs
    /// are authored pointing "up" (north) at their own zero rotation rather than "right" (east),
    /// so aligning them to a field angle measured the usual mathematical way (0°=east, CCW+)
    /// needs the extra 90° frame shift the "...Plus90"/"...Minus90" variants apply - unlike plain
    /// text, see <see cref="MapCimRotationTypeForLabel"/>.
    /// </summary>
    private static RotationType MapCimRotationTypeForMarker(string? cimRotationType) => cimRotationType switch
    {
        "Arithmetic" => RotationType.ArithmeticMinus90,
        "Geographic" => RotationType.GeographicPlus90,
        _ => RotationType.ArithmeticMinus90   // default / unknown
    };

    /// <summary>
    /// Maps a CIM rotation-angle convention ("Arithmetic" or "Geographic") to gView's
    /// <see cref="RotationType"/> for rotating <b>label text</b> (a point label's "RotationField"
    /// placement method). Plain text's own zero rotation already reads left-to-right along the
    /// same "east" axis a mathematical angle is measured from, so - unlike a marker glyph (see
    /// <see cref="MapCimRotationTypeForMarker"/>) - no extra 90° frame shift is needed: using the
    /// "Plus90"/"Minus90" variants here rotated every label a constant 90° off from ArcGIS Pro.
    /// </summary>
    private static RotationType MapCimRotationTypeForLabel(string? cimRotationType) => cimRotationType switch
    {
        "Arithmetic" => RotationType.Arithmetic,
        "Geographic" => RotationType.Geographic,
        _ => RotationType.Arithmetic   // default / unknown
    };

    /// <summary>
    /// Maps Maplex's continuous 0-1000 "Feature weight" scale (see
    /// <see cref="CimMaplexLabelPlacementProperties.FeatureWeight"/>) onto gView's
    /// <see cref="RenderLabelPriority"/> tiers, used both for <see cref="IFeatureLayer.FeatureLabelPriority"/>
    /// and (as a fallback) a label class's own <see cref="SimpleLabelRenderer.LabelPriority"/>. 0 or unset
    /// matches Standard's "None" (no signal - leave the existing default alone). There is no Esri-documented
    /// mapping from this numeric scale onto discrete tiers; splitting the 1-1000 range into equal thirds is
    /// a judgement call, not confirmed against Esri's own preset semantics.
    /// </summary>
    private static RenderLabelPriority? MapMaplexFeatureWeight(double? featureWeight)
    {
        if (featureWeight is null || featureWeight <= 0)
        {
            return null;
        }

        if (featureWeight <= 333)
        {
            return RenderLabelPriority.Low;
        }

        if (featureWeight <= 666)
        {
            return RenderLabelPriority.Normal;
        }

        return RenderLabelPriority.High;
    }

    // -----------------------------------------------------------------------
    // Label renderer conversion
    // -----------------------------------------------------------------------

    private SimpleLabelRenderer ConvertLabelRenderer(CimLabelClass cimLabel, RendererGeometryKind geometryKind)
    {
        // --- Determine whether the expression is a simple field reference, and (if not) try to
        // convert it - done up front, before constructing the renderer, so the right concrete type
        // can be picked in one go: an AdvancedLabelRenderer only when a per-branch "<CLR>" tag
        // actually produced a ColorExpression (see AprxLabelExpressionParser's <remarks>), a plain
        // SimpleLabelRenderer otherwise - which is everything else, unchanged from before this
        // feature existed. FieldNames entries are plain field names (no brackets, never an
        // expression), so the fallback needs to be bracketed here - otherwise the "is this just
        // [Field]?" check below never matches, and a plain field name gets misrouted into
        // AprxLabelExpressionParser (which rejects it, since it isn't valid VB) and ends up
        // flagged as a "complex" expression.
        var fieldNameFallback = cimLabel.FieldNames?.FirstOrDefault();
        var bracketedFallback = fieldNameFallback is not null
            ? (fieldNameFallback.StartsWith('[') && fieldNameFallback.EndsWith(']')
                ? fieldNameFallback           // already bracketed - don't double-wrap it
                : $"[{fieldNameFallback}]")
            : null;
        var expression = cimLabel.Expression ?? bracketedFallback ?? string.Empty;
        var fieldMatch = System.Text.RegularExpressions.Regex.Match(expression, @"^\[([^\]]+)\]$");

        AprxLabelExpressionParser.ConversionResult? conversion = null;
        bool converted = false;
        if (!fieldMatch.Success && !string.IsNullOrEmpty(expression))
        {
            converted = AprxLabelExpressionParser.TryConvert(expression, out conversion) && conversion is not null;
        }

        SimpleLabelRenderer renderer = conversion?.ColorExpression != null
            ? new AdvancedLabelRenderer()
            : new SimpleLabelRenderer();

        // "One label per name/feature/part" - names match 1:1 between CIM and gView.
        renderer.HowManyLabels = cimLabel.StandardLabelPlacementProperties?.NumLabelsOption switch
        {
            "OneLabelPerName" => SimpleLabelRenderer.RenderHowManyLabels.OnPerName,
            "OneLabelPerFeature" or "OneLabelPerShape" => SimpleLabelRenderer.RenderHowManyLabels.OnPerFeature,
            "OneLabelPerPart" => SimpleLabelRenderer.RenderHowManyLabels.OnPerPart,
            _ => renderer.HowManyLabels
        };

        if (_useMaplexLabelEngine)
        {
            // Maplex has no equivalent modelled here of Standard's "Allow overlapping labels"
            // checkbox (below) - just "Feature weight", on its own continuous 0-1000 scale (see
            // MapMaplexFeatureWeight) rather than Standard's None/Low/Medium/High enum. 0/unset
            // keeps gView's flat default (Normal) untouched.
            var mapped = MapMaplexFeatureWeight(cimLabel.MaplexLabelPlacementProperties?.FeatureWeight);
            if (mapped.HasValue)
            {
                renderer.LabelPriority = mapped.Value;
            }
        }
        else if (cimLabel.StandardLabelPlacementProperties?.AllowOverlappingLabels == true)
        {
            // ArcGIS Pro's per-label-class "Allow overlapping labels" checkbox. gView has no
            // direct equivalent of "place normally, but permit overlap as a last resort" - its
            // RenderLabelPriority.Always skips the overlap check entirely and always places at
            // the very first candidate position, which can be far noisier than what ArcGIS Pro
            // actually produces. _allowOverlappingLabelsPriority lets the caller pick a gentler
            // target tier (e.g. High) instead of Always; defaults to Always.
            renderer.LabelPriority = _allowOverlappingLabelsPriority;
        }
        else
        {
            // Otherwise reuse "Feature weight" (see FeatureLabelPriority below - same source
            // property) for the label's *own* priority too, instead of always leaving it at
            // gView's flat default (Normal) regardless of what ArcGIS Pro's Label Priority
            // Ranking actually says. "None"/unset keeps today's default untouched.
            renderer.LabelPriority = cimLabel.StandardLabelPlacementProperties?.FeatureWeight switch
            {
                "Low" => RenderLabelPriority.Low,
                "Medium" => RenderLabelPriority.Normal,
                "High" => RenderLabelPriority.High,
                _ => renderer.LabelPriority
            };
        }

        // --- Apply the expression/conversion already determined above ---
        if (fieldMatch.Success)
        {
            renderer.FieldName = fieldMatch.Groups[1].Value;
        }
        else if (!string.IsNullOrEmpty(expression))
        {
            if (converted && conversion is not null)
            {
                // Successfully reduced the (VBScript-like) ArcGIS Pro expression to a gView
                // expression: plain text with [Field] placeholders, optionally wrapped in
                // gView's "@@start/@@if/@@endif/@@end" conditional-line mini-script.
                renderer.FieldName = conversion.Expression;   // used as fallback field name
                renderer.LabelExpression = conversion.Expression;
                renderer.UseExpression = true;

                var kind = conversion.IsConditional
                    ? "gView conditional label script"
                    : "gView placeholder text";

                _info?.Invoke($"""
                    Layer '{_currentLayerName}': LabelRenderer expression converted to {kind}:
                    ------------------------------------------------------------------
                    Original (ArcGIS Pro):
                    {expression}
                    ------------------------------------------------------------------
                    Converted (gView):
                    {conversion.Expression}
                    ------------------------------------------------------------------
                    """);

                if (conversion.Expression.Contains("[$feature.length", StringComparison.OrdinalIgnoreCase) ||
                    conversion.Expression.Contains("[$feature.area", StringComparison.OrdinalIgnoreCase))
                {
                    // See AprxLabelExpressionParser's <remarks> for the full rationale: "Length"/
                    // "Area" are always assumed to be ArcGIS Pro's geometry accessors, never a
                    // same-named real attribute field (indistinguishable from source text alone),
                    // and the computed value is planar, not geodesic like ArcGIS Pro's own.
                    _info?.Invoke($"""
                        Layer '{_currentLayerName}': the converted expression above computes a
                        geometry length/area at render time ("[$feature.length]"/"[$feature.area]").
                        This is always assumed to mean ArcGIS Pro's "Length($feature)"/"Area($feature)"
                        - if this layer's schema actually has a real field literally named "Length"/
                        "Area", verify this conversion is correct. The computed value is planar
                        (native/unprojected coordinate units), not geodesic like ArcGIS Pro's own -
                        numbers can differ from the original label, especially under a geographic
                        coordinate system or for very long/large features.
                        """);
                }

                if (conversion.ColorExpression is not null && renderer is AdvancedLabelRenderer advancedRenderer)
                {
                    advancedRenderer.ColorExpression = conversion.ColorExpression;

                    _info?.Invoke($"""
                        Layer '{_currentLayerName}': the original expression wrapped its output in
                        ArcGIS Pro's "<CLR red=.. green=.. blue=..>" per-branch color tag - converted
                        to a color expression instead of just stripping the tag:
                        ------------------------------------------------------------------
                        {conversion.ColorExpression}
                        ------------------------------------------------------------------
                        Applied dynamically at render time via AdvancedLabelRenderer (not
                        SimpleLabelRenderer) - the tag is always assumed to be ArcGIS Pro's own
                        color markup, never text from a same-named real field.
                        """);
                }
            }
            else
            {
                // Complex expression: set on renderer as-is so gView can evaluate it
                renderer.FieldName = expression;   // used as fallback field name
                renderer.LabelExpression = expression;
                renderer.UseExpression = true;

                _warn?.Invoke($"""
                    Layer '{_currentLayerName}': LabelRenderer with (complex?) expression:
                    ------------------------------------------------------------------
                    {expression}
                    ------------------------------------------------------------------
                    """);
            }
        }

        if (cimLabel.TextSymbol?.Symbol is CimTextSymbol textSym)
        {
            renderer.TextSymbol = BuildTextSymbol(textSym, colorWillBeOverridden: conversion?.ColorExpression != null);
        }

        switch (geometryKind)
        {
            // Point placement ("around point", ranked by zone: above/center/below x left/
            // center/right) - for lines/polygons this would fight with the handling below.
            case RendererGeometryKind.Point:
            {
                var zonePriorities = _useMaplexLabelEngine
                    ? cimLabel.MaplexLabelPlacementProperties?.PointExternalZonePriorities
                        ?? cimLabel.StandardLabelPlacementProperties?.PointPlacementPriorities
                    : cimLabel.StandardLabelPlacementProperties?.PointPlacementPriorities
                        ?? cimLabel.MaplexLabelPlacementProperties?.PointExternalZonePriorities;

                var placementMethod = _useMaplexLabelEngine
                    ? cimLabel.MaplexLabelPlacementProperties?.PointPlacementMethod
                    : cimLabel.StandardLabelPlacementProperties?.PointPlacementMethod;

                // "AroundPoint" (try the ranked zones below) is the default for point features
                // in both engines; other methods (e.g. Maplex's "CenteredOnPoint") don't have a
                // zone ranking to convert, so leave gView's built-in default (Center) for those.
                if (zonePriorities != null &&
                    (placementMethod is null || string.Equals(placementMethod, "AroundPoint", StringComparison.OrdinalIgnoreCase)))
                {
                    var alignments = OrderedPointPlacementAlignments(zonePriorities);
                    if (alignments.Length > 0 && renderer.TextSymbol != null)
                    {
                        renderer.TextSymbol.TextSymbolAlignment = alignments[0];
                        renderer.TextSymbol.SecondaryTextSymbolAlignments = alignments;
                    }
                }
                // "RotationField": each feature carries its own label angle in a field, instead
                // of ArcGIS Pro trying fixed placement zones - e.g. point features pre-placed
                // along a line network to stand in for a rotated line label. Standard engine
                // only: Maplex has no equivalent modelled here (see ApplyLineLabelPlacement).
                else if (!_useMaplexLabelEngine &&
                    string.Equals(placementMethod, "RotationField", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(cimLabel.StandardLabelPlacementProperties?.RotationField))
                {
                    renderer.SymbolRotation = new SymbolRotation
                    {
                        RotationFieldName = cimLabel.StandardLabelPlacementProperties.RotationField,
                        RotationType = MapCimRotationTypeForLabel(cimLabel.StandardLabelPlacementProperties.RotationType),
                        RotationUnit = RotationUnit.deg
                    };
                }

                break;
            }

            case RendererGeometryKind.Line:
                ApplyLineLabelPlacement(cimLabel, renderer);
                break;
        }

        return renderer;
    }

    /// <summary>
    /// Standard engine only: ArcGIS Pro's simple on/off "place above the line" / "place
    /// centered on the line" / "place below the line" toggles map directly onto gView's
    /// existing <see cref="TextSymbolAlignment.Over"/>/<see cref="TextSymbolAlignment.Center"/>/
    /// <see cref="TextSymbolAlignment.Under"/> - the very same enum used for point placement,
    /// since <c>SimpleTextSymbol</c> positions text relative to a line's baseline exactly like
    /// it does relative to a point. "Above" is offered before "centered" before "below" when
    /// more than one is allowed, matching ArcGIS Pro's own default trial order.
    /// Maplex doesn't expose an equivalent simple toggle for line labels (it places them via
    /// offset/anchor-point properties instead), so this only applies when the Standard engine
    /// is the one actually active for the map.
    /// </summary>
    private void ApplyLineLabelPlacement(CimLabelClass cimLabel, SimpleLabelRenderer renderer)
    {
        if (_useMaplexLabelEngine || renderer.TextSymbol == null)
        {
            return;
        }

        var position = cimLabel.StandardLabelPlacementProperties?.LineLabelPosition;
        if (position == null)
        {
            return;
        }

        List<TextSymbolAlignment> candidates = [];
        if (position.Above) { candidates.Add(TextSymbolAlignment.Over); }
        if (position.InLine) { candidates.Add(TextSymbolAlignment.Center); }
        if (position.Below) { candidates.Add(TextSymbolAlignment.Under); }

        if (candidates.Count > 0)
        {
            renderer.TextSymbol.TextSymbolAlignment = candidates[0];
            renderer.TextSymbol.SecondaryTextSymbolAlignments = candidates.ToArray();
        }
    }

    /// <summary>
    /// Converts ArcGIS Pro's 8 point-label placement zones into gView's
    /// <see cref="TextSymbolAlignment"/> equivalents, ranked best (lowest priority number)
    /// first - directly usable as <see cref="ILabel.SecondaryTextSymbolAlignments"/>, which
    /// gView's label engine tries in order until one doesn't collide.
    /// Mapping derived from SimpleTextSymbol's point placement math (x right / y down):
    /// zones are named from ArcGIS's point of view (e.g. "aboveLeft" = label appears above and
    /// to the left of the point), gView's *Align* names instead say which edge of the label
    /// sits at the point (e.g. "rightAlign" = the label's right edge is at the point, so the
    /// label extends to the left - i.e. the same "...Left" zone).
    /// Per ArcGIS Pro's docs (both Maplex and Standard): 1 is tried first, higher numbers
    /// later - and a priority of *0 means the zone is blocked/prohibited*, not "best". Blocked
    /// zones are dropped entirely rather than sorted to the front.
    /// </summary>
    private static TextSymbolAlignment[] OrderedPointPlacementAlignments(CimPointZonePriorities zones)
    {
        (int Priority, TextSymbolAlignment Alignment)[] zonesByPriority =
        [
            (zones.AboveLeft, TextSymbolAlignment.rightAlignOver),
            (zones.AboveCenter, TextSymbolAlignment.Over),
            (zones.AboveRight, TextSymbolAlignment.leftAlignOver),
            (zones.CenterLeft, TextSymbolAlignment.rightAlignCenter),
            (zones.CenterRight, TextSymbolAlignment.leftAlignCenter),
            (zones.BelowLeft, TextSymbolAlignment.rightAlignUnder),
            (zones.BelowCenter, TextSymbolAlignment.Under),
            (zones.BelowRight, TextSymbolAlignment.leftAlignUnder),
        ];

        return zonesByPriority
            .Where(z => z.Priority > 0)  // 0 = "prohibit this zone", not "best zone" - exclude it
            .OrderBy(z => z.Priority)    // lower number = tried first; stable sort keeps the order above on ties
            .Select(z => z.Alignment)
            .ToArray();
    }

    private enum RendererGeometryKind { Unknown, Point, Line, Polygon }

    /// <summary>
    /// Best-effort check for which geometry type a layer's renderer draws (used to decide
    /// which of the CIM's label placement properties apply). Scans every symbol reference the
    /// renderer can carry (single symbol, unique-value classes, class breaks).
    /// </summary>
    private static RendererGeometryKind DetermineRendererGeometryKind(CimRenderer? renderer)
    {
        if (AnyRendererSymbolIs<CimPointSymbol>(renderer)) { return RendererGeometryKind.Point; }
        if (AnyRendererSymbolIs<CimLineSymbol>(renderer)) { return RendererGeometryKind.Line; }
        if (AnyRendererSymbolIs<CimPolygonSymbol>(renderer)) { return RendererGeometryKind.Polygon; }

        return RendererGeometryKind.Unknown;
    }

    private static bool AnyRendererSymbolIs<T>(CimRenderer? renderer) where T : CimSymbol
    {
        return renderer switch
        {
            CimSimpleRenderer simple => simple.Symbol?.Symbol is T,
            CimUniqueValueRenderer uv =>
                uv.DefaultSymbol?.Symbol is T ||
                uv.Groups?.SelectMany(g => g.Classes ?? []).Any(c => c.Symbol?.Symbol is T) == true,
            CimClassBreaksRenderer cb =>
                cb.DefaultSymbol?.Symbol is T ||
                cb.Breaks?.Any(b => b.Symbol?.Symbol is T) == true,
            _ => false
        };
    }

    /// <summary>
    /// True when every color reachable from <paramref name="renderer"/>'s symbols is fully
    /// transparent (alpha 0) - see the comment at this method's one call site in
    /// CreateFeatureLayer. Symbol types this can't see a color for (picture markers/fills,
    /// referenced by URL rather than a color) make it return false, so a genuinely visible
    /// image-based layer is never mistaken for an invisible one.
    /// </summary>
    private static bool RendererIsFullyTransparent(CimRenderer? renderer)
    {
        var colors = new List<CimColor>();
        var sawPictureSymbol = false;
        CollectColors(renderer, colors, ref sawPictureSymbol);

        return !sawPictureSymbol && colors.Count > 0 && colors.All(c => c.AlphaByte == 0);
    }

    private static void CollectColors(CimRenderer? renderer, List<CimColor> colors, ref bool sawPictureSymbol)
    {
        switch (renderer)
        {
            case CimSimpleRenderer simple:
                CollectColors(simple.Symbol?.Symbol, colors, ref sawPictureSymbol);
                break;
            case CimUniqueValueRenderer uv:
                CollectColors(uv.DefaultSymbol?.Symbol, colors, ref sawPictureSymbol);
                foreach (var cls in uv.Groups?.SelectMany(g => g.Classes ?? []) ?? [])
                {
                    CollectColors(cls.Symbol?.Symbol, colors, ref sawPictureSymbol);
                }
                break;
            case CimClassBreaksRenderer cb:
                CollectColors(cb.DefaultSymbol?.Symbol, colors, ref sawPictureSymbol);
                foreach (var brk in cb.Breaks ?? [])
                {
                    CollectColors(brk.Symbol?.Symbol, colors, ref sawPictureSymbol);
                }
                break;
        }
    }

    private static void CollectColors(CimSymbol? symbol, List<CimColor> colors, ref bool sawPictureSymbol)
    {
        foreach (var layer in symbol?.SymbolLayers?.Where(l => l.Enable) ?? [])
        {
            switch (layer)
            {
                case CimSolidFill fill:
                    if (fill.Color != null) colors.Add(fill.Color);
                    break;
                case CimSolidStroke stroke:
                    if (stroke.Color != null) colors.Add(stroke.Color);
                    break;
                case CimCharacterMarker marker:
                    if (marker.Color != null) colors.Add(marker.Color);
                    CollectColors(marker.Symbol, colors, ref sawPictureSymbol);
                    break;
                case CimHatchFill hatch:
                    CollectColors(hatch.LineSymbol, colors, ref sawPictureSymbol);
                    break;
                case CimVectorMarker vectorMarker:
                    foreach (var graphic in vectorMarker.MarkerGraphics ?? [])
                    {
                        CollectColors(graphic.Symbol, colors, ref sawPictureSymbol);
                    }
                    break;
                case CimPictureMarker:
                case CimPictureFill:
                    sawPictureSymbol = true;
                    break;
            }
        }
    }

    private ITextSymbol BuildTextSymbol(CimTextSymbol cimText, bool colorWillBeOverridden = false)
    {
        float fontSize = cimText.Height > 0 ? (float)cimText.Height : 10f;
        string fontFamily = string.IsNullOrWhiteSpace(cimText.FontFamilyName) ? "Arial" : cimText.FontFamilyName;

        // Resolve font style flags from "fontStyleName" ("Bold", "Italic", "Bold Italic", "Regular", …)
        bool bold = cimText.FontStyleName?.Contains("Bold", StringComparison.OrdinalIgnoreCase) == true;
        bool italic = cimText.FontStyleName?.Contains("Italic", StringComparison.OrdinalIgnoreCase) == true;
        var fontStyle = (bold, italic) switch
        {
            (true, true) => gView.GraphicsEngine.FontStyle.Bold | gView.GraphicsEngine.FontStyle.Italic,
            (true, false) => gView.GraphicsEngine.FontStyle.Bold,
            (false, true) => gView.GraphicsEngine.FontStyle.Italic,
            _ => gView.GraphicsEngine.FontStyle.Regular
        };
        var font = gView.GraphicsEngine.Current.Engine.CreateFont(fontFamily, fontSize, fontStyle);

        // Resolve text (foreground) color from the nested TextFillSymbol
        var textColor = ApplyLayerTransparency(ArgbColor.Black);
        if (cimText.TextFillSymbol is CimPolygonSymbol fillPoly)
        {
            var solidFill = fillPoly.SymbolLayers?.OfType<CimSolidFill>().FirstOrDefault();
            if (solidFill?.Color != null)
                textColor = ToArgbColor(solidFill.Color);
        }

        // --- Callout background ("mask" box behind the text) → BlockoutTextSymbol ---
        // Checked before the halo: a background box is the more deliberate authoring choice,
        // and ArcGIS Pro rarely combines both on the same label class (this converter can only
        // produce one or the other - BlockoutTextSymbol and GlowingTextSymbol are both
        // SimpleTextSymbol subclasses, not composable).
        if (cimText.Callout != null)
        {
            if (cimText.Callout.Type == "CIMBalloonCallout" && cimText.Callout.BackgroundSymbol is CimPolygonSymbol backgroundPoly)
            {
                var backgroundFill = backgroundPoly.SymbolLayers?.OfType<CimSolidFill>().FirstOrDefault();
                var backgroundColor = backgroundFill?.Color != null
                    ? ToArgbColor(backgroundFill.Color)
                    : ApplyLayerTransparency(ArgbColor.White);

                // The background CIMPolygonSymbol can carry its own CIMSolidStroke layer for the
                // box's border - ArcGIS Pro renders it (visible e.g. as a thin frame around the
                // white readability box), gView used to just ignore it.
                var backgroundStroke = backgroundPoly.SymbolLayers?.OfType<CimSolidStroke>().FirstOrDefault();

                var blockout = new BlockoutTextSymbol();
                blockout.Font = font;
                blockout.Color = EnsureContrastingTextColor(textColor, backgroundColor, "background box", colorWillBeOverridden);
                blockout.ColorOutline = backgroundColor; // despite the name, this is the box's fill color

                // CIMBalloonCallout.margin pads the box out from the text - ArcGIS Pro's box is
                // noticeably bigger than the pixel-tight rectangle gView used to draw. Its four
                // sides can differ; BlockoutTextSymbol only has a single, symmetric Padding (same
                // simplification this method already makes for its own Margin/AddPadding fallback
                // above), so take the largest of the four.
                var margin = cimText.Callout.Margin;
                if (margin != null)
                {
                    blockout.Padding = PointsToPixels(Math.Max(Math.Max(margin.Left, margin.Right), Math.Max(margin.Top, margin.Bottom)));
                }

                if (backgroundStroke?.Color != null && backgroundStroke.Width > 0)
                {
                    blockout.BorderColor = ToArgbColor(backgroundStroke.Color);
                    blockout.BorderWidth = PointsToPixels(backgroundStroke.Width);
                }

                return blockout;
            }

            _warn?.Invoke(cimText.Callout.Type == "CIMBalloonCallout"
                ? $"Layer '{_currentLayerName}': text callout has no usable backgroundSymbol and was ignored."
                : $"Layer '{_currentLayerName}': text callout of type '{cimText.Callout.Type}' is not supported and was ignored.");
        }

        // --- Halo → GlowingTextSymbol ---
        if (cimText.HaloSize > 0 && cimText.HaloSymbol is CimPolygonSymbol haloPoly)
        {
            var haloFill = haloPoly.SymbolLayers?.OfType<CimSolidFill>().FirstOrDefault();
            var haloColor = haloFill?.Color != null ? ToArgbColor(haloFill.Color) : ApplyLayerTransparency(ArgbColor.White);

            var glow = new GlowingTextSymbol();
            glow.Font = font;
            glow.Color = EnsureContrastingTextColor(textColor, haloColor, "halo", colorWillBeOverridden);
            glow.GlowingColor = haloColor;
            glow.GlowingWidth = (int)Math.Round(PointsToPixels(cimText.HaloSize));
            glow.GlowingSmoothingmode = SymbolSmoothing.AntiAlias;
            return glow;
        }

        // --- Plain text symbol ---
        var sym = new SimpleTextSymbol();
        sym.Font = font;
        sym.Color = textColor;
        return sym;
    }

    /// <summary>
    /// Falls back to a contrasting color when <paramref name="textColor"/> and
    /// <paramref name="haloOrBackgroundColor"/> resolve to the exact same RGB - text drawn in
    /// that combination is invisible regardless of why the colors matched. The recurring
    /// real-world cause: ArcGIS Pro's per-feature "&lt;CLR red=.. green=.. blue=..&gt;" label
    /// expression tags. When <paramref name="colorWillBeOverridden"/> is <see langword="false"/>
    /// (this converter couldn't turn those tags into a <c>ColorExpression</c> - see
    /// <see cref="AprxLabelExpressionParser"/>'s per-branch <c>&lt;CLR&gt;</c> support and its
    /// <c>&lt;remarks&gt;</c>), this only ever sees the label class's own static text color, which
    /// authors commonly leave equal to the halo/background color precisely because ArcGIS Pro
    /// never actually displays it - so it also warns, since the black/white pick is a guess, not
    /// a faithful reproduction of whatever ArcGIS Pro actually shows. When
    /// <paramref name="colorWillBeOverridden"/> is <see langword="true"/>, the fallback color is
    /// still computed and applied the same way (it's still needed for whichever features don't
    /// match any color branch, and might fall through to this same static color) but the warning
    /// is skipped - that case is now correctly handled dynamically, so warning about it would be
    /// stale/misleading.
    /// </summary>
    private ArgbColor EnsureContrastingTextColor(ArgbColor textColor, ArgbColor haloOrBackgroundColor, string kind, bool colorWillBeOverridden = false)
    {
        if (textColor.R != haloOrBackgroundColor.R ||
            textColor.G != haloOrBackgroundColor.G ||
            textColor.B != haloOrBackgroundColor.B)
        {
            return textColor;
        }

        // Perceived luminance (ITU-R BT.601) of the halo/background - pick whichever of
        // black/white stands out more against it. Alpha is kept from the original text color
        // (already layer-transparency-adjusted) since only the RGB was indistinguishable.
        var luminance = (0.299 * haloOrBackgroundColor.R + 0.587 * haloOrBackgroundColor.G + 0.114 * haloOrBackgroundColor.B) / 255.0;
        var fallback = luminance > 0.5
            ? ArgbColor.FromArgb(textColor.A, 0, 0, 0)
            : ArgbColor.FromArgb(textColor.A, 255, 255, 255);

        if (!colorWillBeOverridden)
        {
            _warn?.Invoke(
                $"Layer '{_currentLayerName}': label text color was identical to its {kind} color " +
                $"(both RGB {textColor.R},{textColor.G},{textColor.B}) and would have been invisible - " +
                $"falling back to {(luminance > 0.5 ? "black" : "white")}. This usually means the real " +
                "text color is set dynamically per feature via a label expression (e.g. ArcGIS Pro's " +
                "\"<CLR red=.. green=.. blue=..>\" tags), which this converter does not evaluate.");
        }

        return fallback;
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
        var fill = layers?.OfType<CimSolidFill>().FirstOrDefault();
        var stroke = layers?.OfType<CimSolidStroke>().FirstOrDefault();

        var symbol = new SimplePointSymbol() { SymbolSmoothingMode = SymbolSmoothing.AntiAlias };
        if (fill?.Color != null)
        {
            ((IBrushColor)symbol).FillColor = ToArgbColor(fill.Color);
        }
        if (stroke?.Color != null)
        {
            ((IPenColor)symbol).PenColor = ToArgbColor(stroke.Color);
            ((IPenWidth)symbol).PenWidth = PointsToPixels(stroke.Width);
        }
        return symbol;
    }

    // Caches the ink-centering correction (see GetGlyphCenteringCorrectionFraction) per
    // font+character so it's only measured once even if many layers share a symbol font.
    private readonly Dictionary<(string FontFamily, char Character), (float X, float Y)> _glyphCenteringCorrectionCache = new();

    private TrueTypeMarkerSymbol ConvertCharacterMarker(CimCharacterMarker marker)
    {
        var ttmSymbol = new TrueTypeMarkerSymbol() { SymbolSmoothingMode = SymbolSmoothing.AntiAlias };

        var character = (char)(byte)marker.CharacterIndex;
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

        // ArcGIS Pro's anchorPoint moves the point of the symbol that is placed at the
        // feature's geometry away from the symbol's bounding-box center (gView's implicit
        // anchor, StringAlignment.Center/Center). It's expressed in the symbol's own
        // coordinate space (x right, y up) and applied *before* rotation. "Relative" units
        // span -1..1 across the full symbol extent, so one unit equals half of Size.
        // offsetX/offsetY are plain post-rotation translations in the same axis convention.
        // gView's HorizontalOffset/VerticalOffset are always applied in screen space
        // (independent of rotation, see TrueTypeMarkerSymbol.PerformSymbolTransformation),
        // which matches offsetX/offsetY directly and anchorPoint whenever the marker isn't
        // rotated. Both need their Y component flipped to move from CIM's y-up convention
        // into gView's screen space (y down).
        var (anchorX, anchorY) = ResolveAnchorPoint(marker);

        // Many ArcGIS Pro marker/dingbat fonts ship with bogus ascent/descent metadata that
        // has nothing to do with where the glyph is actually drawn (e.g. an ascent far larger
        // than the font's own em-size). gView centers a character marker using those line
        // metrics (StringAlignment.Center), so on such fonts the glyph can render well off the
        // feature point even when the CIM symbol has no anchorPoint/offset at all. Measure the
        // glyph's actual rendered ink and add a correction that re-centers on it instead.
        var (glyphCorrectionX, glyphCorrectionY) = GetGlyphCenteringCorrectionFraction(marker.FontFamilyName, character);

        double hOffset = -anchorX + marker.OffsetX + glyphCorrectionX * marker.Size;
        double vOffset = anchorY - marker.OffsetY + glyphCorrectionY * marker.Size;

        ttmSymbol.HorizontalOffset = (float)hOffset;
        ttmSymbol.VerticalOffset = (float)vOffset;

        return ttmSymbol;
    }

    private static (double X, double Y) ResolveAnchorPoint(CimCharacterMarker marker)
    {
        if (marker.AnchorPoint == null)
        {
            return (0, 0);
        }

        var isAbsolute = string.Equals(marker.AnchorPointUnits, "Absolute", StringComparison.OrdinalIgnoreCase);
        var scale = isAbsolute ? 1.0 : marker.Size / 2.0;

        return (marker.AnchorPoint.X * scale, marker.AnchorPoint.Y * scale);
    }

    /// <summary>
    /// Measures how far a character's actually-rendered ink drifts from gView's default
    /// StringAlignment.Center/Center anchor (which is based on the font's ascent/descent line
    /// metrics), by rendering the glyph offscreen through the current graphics engine and
    /// scanning it. Returns the correction needed to re-center on the visible glyph instead,
    /// expressed as a *fraction of font size* so callers can scale it to any marker size.
    /// Returns (0,0) if the glyph can't be rendered/measured (e.g. font not installed).
    /// </summary>
    private (float X, float Y) GetGlyphCenteringCorrectionFraction(string fontFamilyName, char character)
    {
        var key = (fontFamilyName, character);
        if (_glyphCenteringCorrectionCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var correction = (X: 0f, Y: 0f);
        try
        {
            const float measureSize = 100f;
            const int canvasSize = 300;
            const float center = canvasSize / 2f;

            var engine = gView.GraphicsEngine.Current.Engine;

            using var font = engine.CreateFont(fontFamilyName, measureSize);
            using var bitmap = engine.CreateBitmap(canvasSize, canvasSize);
            using var canvas = bitmap.CreateCanvas();
            using var brush = engine.CreateSolidBrush(ArgbColor.Black);

            canvas.Clear(ArgbColor.White);
            canvas.TextRenderingHint = GraphicsEngine.TextRenderingHint.AntiAlias;

            var format = engine.CreateDrawTextFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            canvas.DrawText(character.ToString(), font, brush, center, center, format);
            canvas.Flush();

            int minX = canvasSize, maxX = -1, minY = canvasSize, maxY = -1;
            for (var y = 0; y < canvasSize; y++)
            {
                for (var x = 0; x < canvasSize; x++)
                {
                    var p = bitmap.GetPixel(x, y);
                    if (p.R < 250 || p.G < 250 || p.B < 250)
                    {
                        if (x < minX) { minX = x; }
                        if (x > maxX) { maxX = x; }
                        if (y < minY) { minY = y; }
                        if (y > maxY) { maxY = y; }
                    }
                }
            }

            var pixelsPerNominalUnit = engine.ScreenDpi / 72f;
            correction = (
                X: ComputeAxisCorrectionFraction(center, minX, maxX, pixelsPerNominalUnit, measureSize),
                Y: ComputeAxisCorrectionFraction(center, minY, maxY, pixelsPerNominalUnit, measureSize));
        }
        catch (Exception ex)
        {
            _warn?.Invoke($"Could not measure glyph centering for font '{fontFamilyName}' char {(int)character}: {ex.Message}");
        }

        _glyphCenteringCorrectionCache[key] = correction;
        return correction;
    }

    /// <summary>
    /// Pure part of <see cref="GetGlyphCenteringCorrectionFraction"/>: given the drawn ink's
    /// pixel extent along one axis, returns the correction - as a fraction of font size -
    /// needed to move that ink's center back onto <paramref name="drawCenter"/>.
    /// </summary>
    internal static float ComputeAxisCorrectionFraction(float drawCenter, int inkMin, int inkMax, float pixelsPerNominalUnit, float measureSize)
    {
        if (inkMax < inkMin || pixelsPerNominalUnit <= 0 || measureSize <= 0)
        {
            // No ink found (blank/missing glyph), or degenerate scale - no correction possible.
            return 0f;
        }

        var inkCenter = (inkMin + inkMax) / 2f;
        return (drawCenter - inkCenter) / pixelsPerNominalUnit / measureSize;
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
            ((IPenWidth)line).PenWidth = PointsToPixels(stroke.Width);
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
                CimHatchFill hatch => ConvertHatchFill(hatch, outlineForThis),
                CimSolidFill fill => ConvertSolidFill(fill, outlineForThis),
                CimPictureFill => ConvertSolidFill(null, outlineForThis),  // unsupported → plain fill
                _ => ConvertSolidFill(null, outlineForThis)
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
            < 10 or >= 170 => HatchStyle.Horizontal,          //   0° – horizontal
            >= 10 and < 35 => HatchStyle.LightDownwardDiagonal,// ~22°
            >= 35 and < 55 => HatchStyle.ForwardDiagonal,      //  45°
            >= 55 and < 80 => HatchStyle.DarkDownwardDiagonal, // ~67°
            >= 80 and < 100 => HatchStyle.Vertical,             //  90°
            >= 100 and < 125 => HatchStyle.LightUpwardDiagonal,  // ~112°
            >= 125 and < 145 => HatchStyle.BackwardDiagonal,     // 135°
            _ => HatchStyle.DarkUpwardDiagonal    // ~157°
        };
    }

    // -----------------------------------------------------------------------
    // Color conversion
    // -----------------------------------------------------------------------

    private ArgbColor ToArgbColor(CimColor cimColor)
    {
        byte a = ApplyLayerTransparency(cimColor.AlphaByte);

        return cimColor switch
        {
            CimRgbColor rgb => ArgbColor.FromArgb(a, Clamp(rgb.R), Clamp(rgb.G), Clamp(rgb.B)),
            CimGrayColor gray => ArgbColor.FromArgb(a, Clamp(gray.Level), Clamp(gray.Level), Clamp(gray.Level)),
            CimCmykColor cmyk => CmykToArgb(a, cmyk.C, cmyk.M, cmyk.Y, cmyk.K),
            CimHsvColor hsv => HsvToArgb(a, hsv.H, hsv.S, hsv.V),
            _ => ArgbColor.FromArgb(a, ArgbColor.Gray.R, ArgbColor.Gray.G, ArgbColor.Gray.B)
        };
    }

    /// <summary>
    /// Scales <paramref name="alpha"/> (a symbol's own opacity) down by the current layer's
    /// "transparency" (see <see cref="_currentLayerTransparency"/>) - multiplicative, not a
    /// replacement, so a symbol that already has its own partial transparency ends up even more
    /// transparent, matching how ArcGIS Pro composites the two.
    /// </summary>
    private byte ApplyLayerTransparency(byte alpha)
    {
        if (_currentLayerTransparency <= 0)
        {
            return alpha;
        }

        var opacityFactor = Math.Clamp(1.0 - _currentLayerTransparency / 100.0, 0.0, 1.0);
        return (byte)Math.Round(alpha * opacityFactor);
    }

    /// <summary>Overload for colors built directly (not via <see cref="ToArgbColor"/>) - e.g. the fixed default black text/white halo used when the CIM doesn't specify one.</summary>
    private ArgbColor ApplyLayerTransparency(ArgbColor color) =>
        ArgbColor.FromArgb(ApplyLayerTransparency(color.A), color.R, color.G, color.B);

    /// <summary>
    /// Converts a CIM size given in points (1/72 inch - the unit ArcGIS Pro uses for line/outline
    /// widths and text halo sizes) into gView's pixel-based symbol units, using the graphics
    /// engine's configured screen DPI. gView's <c>IPenWidth</c>/<c>GlowingWidth</c> values are
    /// plain pixels with an implicit 96 DPI baseline (see <c>ReferenceScaleHelper</c> /
    /// <c>CloneOptions.DpiFactor</c>, which only scales away from 96 DPI) - the same baseline
    /// ArcGIS assumes when rendering a map/feature service - so at the default 96 DPI this is a
    /// straight ×(96/72) ≈ ×1.33 factor. Without it, a value copied straight from the aprx (e.g.
    /// a "1 pt" line) renders about 25% too thin compared to ArcGIS/AGS.
    /// Font sizes and character-marker sizes don't need this: they're created via
    /// <c>IGraphicsEngine.CreateFont(..., GraphicsUnit.Point)</c>, which already applies the same
    /// conversion internally.
    /// </summary>
    private static float PointsToPixels(double points)
        => (float)(points * gView.GraphicsEngine.Current.Engine.ScreenDpi / 72.0);

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

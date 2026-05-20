using gView.Cmd.Import.Aprx.Models;
using System.IO.Compression;
using System.Text.Json;

namespace gView.Cmd.Import.Aprx;

/// <summary>
/// Reads an ESRI APRX file (ZIP archive containing CIM JSON files) and returns
/// the contained map definitions as <see cref="CimMap"/> instances together
/// with their fully resolved layer definitions.
/// </summary>
internal class AprxReader
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private readonly string _aprxPath;
    private readonly Action<string>? _warn;

    public AprxReader(string aprxPath, Action<string>? warn = null)
    {
        if (!File.Exists(aprxPath))
        {
            throw new FileNotFoundException($"APRX file not found: {aprxPath}", aprxPath);
        }
        _aprxPath = aprxPath;
        _warn = warn;
    }

    /// <summary>
    /// Opens the APRX, parses all contained maps and resolves their layer definitions.
    /// </summary>
    public async Task<IReadOnlyList<AprxMapResult>> ReadMapsAsync()
    {
        using var archive = ZipFile.OpenRead(_aprxPath);

        // 1) Read the project root
        var project = await ReadJsonEntryAsync<CimProject>(archive, "GISProject.json");
        if (project == null)
        {
            throw new InvalidDataException("GISProject.json not found or could not be parsed.");
        }

        var results = new List<AprxMapResult>();

        // 2) Find all map document items
        // Support both the legacy CIMMapDocument type and the newer CIMProjectItem format
        // (ArcGIS Pro 3.x) where maps are identified by itemType="Map".
        var mapItems = project.ProjectItems?
            .Where(i => string.Equals(i.Type, "CIMMapDocument", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(i.ItemType, "Map", StringComparison.OrdinalIgnoreCase))
            ?? [];

        foreach (var item in mapItems)
        {
            // URI is used in legacy format; catalogPath (CIMPATH=...) in ArcGIS Pro 3.x
            var rawPath = item.Uri ?? item.CatalogPath;
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                continue;
            }

            // Normalise path separator to forward slash for ZIP entry lookup
            var entryPath = ExtractCimPath(rawPath);

            var maps = await ReadMapEntriesAsync(archive, entryPath);

            foreach (var map in maps)
            {
                // 3) Resolve layer references -> load lyrx files
                var resolvedLayers = await ResolveLayersAsync(archive, map.Layers, map.LayerDefinitions);
                results.Add(new AprxMapResult(map, resolvedLayers));
            }
        }

        // Fallback: if no map items were found in the project, scan the archive for
        // *.mapx files or root-level CIMMap JSON files.
        if (results.Count == 0)
        {
            var candidateEntries = archive.Entries.Where(e =>
                e.FullName.EndsWith(".mapx", StringComparison.OrdinalIgnoreCase) ||
                e.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

            foreach (var entry in candidateEntries)
            {
                var maps = await ReadMapEntriesFromZipEntryAsync(entry);
                foreach (var map in maps)
                {
                    var resolvedLayers = await ResolveLayersAsync(archive, map.Layers, map.LayerDefinitions);
                    results.Add(new AprxMapResult(map, resolvedLayers));
                }
            }
        }

        return results;
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Loads a map entry from the archive. Handles both the legacy CIMMapDocument
    /// wrapper and the ArcGIS Pro 3.x format where the root object is a CIMMap.
    /// </summary>
    private async Task<List<CimMap>> ReadMapEntriesAsync(ZipArchive archive, string entryPath)
    {
        var entry = archive.GetEntry(entryPath)
                 ?? archive.Entries.FirstOrDefault(e =>
                        string.Equals(e.FullName, entryPath, StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            return [];
        }

        return await ReadMapEntriesFromZipEntryAsync(entry);
    }

    /// <summary>
    /// Loads CimMap instances from a ZIP entry, handling both CIMMapDocument wrapper
    /// and root-level CIMMap format.
    /// </summary>
    private async Task<List<CimMap>> ReadMapEntriesFromZipEntryAsync(ZipArchiveEntry entry)
    {
        var maps = new List<CimMap>();

        // First try the wrapper format (CIMMapDocument)
        var mapDoc = await ReadZipEntryAsync<CimMapDocument>(entry);
        if (mapDoc != null)
        {
            if (mapDoc.Map != null)
            {
                maps.Add(mapDoc.Map);
            }
            if (mapDoc.LayerDefinitions != null)
            {
                maps.AddRange(mapDoc.LayerDefinitions);
            }
        }

        // If no maps found via the wrapper, try deserialising as a root-level CIMMap
        // (ArcGIS Pro 3.x stores the map directly at root with type="CIMMap")
        if (maps.Count == 0)
        {
            var directMap = await ReadZipEntryAsync<CimMap>(entry);
            if (directMap?.Type != null &&
                directMap.Type.Contains("CIMMap", StringComparison.OrdinalIgnoreCase))
            {
                maps.Add(directMap);
            }
        }

        return maps;
    }

    private async Task<List<CimBaseLayer>> ResolveLayersAsync(
        ZipArchive archive,
        IEnumerable<string>? layerPaths,
        IEnumerable<CimBaseLayer>? inlineLayers)
    {
        var resolved = new List<CimBaseLayer>();

        // Inline definitions take priority
        if (inlineLayers != null)
        {
            foreach (var layer in inlineLayers)
            {
                await ResolveGroupLayerChildrenAsync(archive, layer);
                resolved.Add(layer);
            }
            return resolved;
        }

        if (layerPaths == null)
        {
            return resolved;
        }

        foreach (var path in layerPaths)
        {
            // Paths are like "CIMPATH=Layers/Roads.lyrx"
            var entryPath = ExtractCimPath(path);
            if (string.IsNullOrEmpty(entryPath))
            {
                continue;
            }

            var layerDoc = await ReadJsonEntryAsync<CimLayerDocument>(archive, entryPath);
            if (layerDoc?.LayerDefinitions != null && layerDoc.LayerDefinitions.Count > 0)
            {
                foreach (var layer in layerDoc.LayerDefinitions)
                {
                    await ResolveGroupLayerChildrenAsync(archive, layer);
                    resolved.Add(layer);
                }
            }
            else
            {
                // ArcGIS Pro 3.x: each layer is stored as a standalone JSON file.
                // Use the "type" property already read by CimLayerDocument to select the
                // correct concrete class directly — avoids relying on [JsonPolymorphic]
                // dispatch which can silently return null when a nested property throws.
                CimBaseLayer? directLayer = layerDoc?.Type switch
                {
                    string t when t.Contains("CIMGroupLayer", StringComparison.OrdinalIgnoreCase)
                        => await ReadJsonEntryAsync<CimGroupLayer>(archive, entryPath),
                    string t when t.Contains("CIMFeatureLayer", StringComparison.OrdinalIgnoreCase)
                        => await ReadJsonEntryAsync<CimFeatureLayer>(archive, entryPath),
                    string t when !string.IsNullOrEmpty(t)
                        => null,  // unsupported layer type (e.g. CIMAnnotationLayer) — skip silently
                    _ => await ReadJsonEntryAsync<CimBaseLayer>(archive, entryPath)
                };
                if (directLayer != null)
                {
                    await ResolveGroupLayerChildrenAsync(archive, directLayer);
                    resolved.Add(directLayer);
                }
            }
        }

        return resolved;
    }

    /// <summary>
    /// If <paramref name="layer"/> is a <see cref="CimGroupLayer"/> whose children are
    /// listed as CIMPATH references in <c>Layers</c> (rather than inline in
    /// <c>LayerDefinitions</c>), loads those children from the archive and populates
    /// <c>LayerDefinitions</c> so the converter can iterate them uniformly.
    /// Recurses into nested group layers.
    /// </summary>
    private async Task ResolveGroupLayerChildrenAsync(ZipArchive archive, CimBaseLayer layer)
    {
        if (layer is not CimGroupLayer group)
        {
            return;
        }

        // Already resolved or nothing to resolve
        if (group.LayerDefinitions is { Count: > 0 } || group.Layers is null or { Count: 0 })
        {
            return;
        }

        group.LayerDefinitions = await ResolveLayersAsync(archive, group.Layers, null);
    }

    /// <summary>
    /// Extracts the file path from a CIMPATH string like "CIMPATH=Layers/Roads.lyrx".
    /// </summary>
    private static string ExtractCimPath(string cimPath)
    {
        const string prefix = "CIMPATH=";
        if (cimPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return cimPath[prefix.Length..].Replace('\\', '/');
        }
        // Could also be a plain relative path
        return cimPath.Replace('\\', '/').TrimStart('/');
    }

    private async Task<T?> ReadJsonEntryAsync<T>(ZipArchive archive, string entryPath)
    {
        var entry = archive.GetEntry(entryPath)
                 ?? archive.Entries.FirstOrDefault(e =>
                        string.Equals(e.FullName, entryPath, StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            return default;
        }

        return await ReadZipEntryAsync<T>(entry);
    }

    private async Task<T?> ReadZipEntryAsync<T>(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Position = 0;

        try
        {
            return await JsonSerializer.DeserializeAsync<T>(ms, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _warn?.Invoke($"JSON error reading {typeof(T).Name} from '{entry.FullName}': {ex.Message}");
            return default;
        }
    }
}

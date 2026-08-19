using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="AprxReader"/>: opening the .aprx ZIP archive, locating map entries in
/// both the legacy ("CIMMapDocument"/URI) and ArcGIS Pro 3.x ("itemType=Map"/CIMPATH catalogPath)
/// project-item formats, resolving layer references (inline vs. external .lyrx/.json files,
/// nested group layers), and the various fallback/error-tolerance paths.
/// </summary>
public class AprxReaderTests
{
    // -----------------------------------------------------------------------
    // Constructor / top-level errors
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_MissingFile_ThrowsFileNotFoundException()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"does-not-exist-{Guid.NewGuid():N}.aprx");

        Assert.Throws<FileNotFoundException>(() => new AprxReader(missingPath));
    }

    [Fact]
    public async Task ReadMapsAsync_NoGISProjectJson_ThrowsInvalidDataException()
    {
        using var aprx = TempAprxFile.Create(("SomethingElse.json", "{}"));
        var reader = new AprxReader(aprx.Path);

        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadMapsAsync());
    }

    [Fact]
    public async Task ReadMapsAsync_EmptyArchive_ThrowsInvalidDataException()
    {
        using var aprx = TempAprxFile.CreateEmpty();
        var reader = new AprxReader(aprx.Path);

        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadMapsAsync());
    }

    [Fact]
    public async Task ReadMapsAsync_NoProjectItems_ReturnsNoMaps()
    {
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """{ "projectItems": [] }"""));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        Assert.Empty(results);
    }

    // -----------------------------------------------------------------------
    // Legacy format: CIMMapDocument project item + "map" wrapper property
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadMapsAsync_LegacyFormat_ReadsMapAndResolvesExternalLayer()
    {
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                {
                  "projectItems": [
                    { "type": "CIMMapDocument", "URI": "Maps/Map.mapx", "name": "Map" }
                  ]
                }
                """),
            ("Maps/Map.mapx", """
                {
                  "type": "CIMMapDocument",
                  "map": {
                    "type": "CIMMap",
                    "name": "Strom LD",
                    "layers": [ "CIMPATH=Layers/Roads.lyrx" ]
                  }
                }
                """),
            ("Layers/Roads.lyrx", """
                {
                  "layerDefinitions": [
                    { "type": "CIMFeatureLayer", "name": "Roads" }
                  ]
                }
                """));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        var result = Assert.Single(results);
        Assert.Equal("Strom LD", result.Map.Name);
        var layer = Assert.Single(result.Layers);
        var featureLayer = Assert.IsType<CimFeatureLayer>(layer);
        Assert.Equal("Roads", featureLayer.Name);
    }

    [Fact]
    public async Task ReadMapsAsync_LegacyFormat_MultipleMapsViaLayerDefinitionsProperty_ReturnsAllMaps()
    {
        // CimMapDocument.LayerDefinitions (JSON property "layerDefinitions") confusingly holds a
        // *list of CimMap*, not layers - used when a single .mapx bundles several maps.
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                { "projectItems": [ { "type": "CIMMapDocument", "URI": "Maps/Map.mapx" } ] }
                """),
            ("Maps/Map.mapx", """
                {
                  "type": "CIMMapDocument",
                  "layerDefinitions": [
                    { "type": "CIMMap", "name": "First" },
                    { "type": "CIMMap", "name": "Second" }
                  ]
                }
                """));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal("First", results[0].Map.Name);
        Assert.Equal("Second", results[1].Map.Name);
    }

    // -----------------------------------------------------------------------
    // ArcGIS Pro 3.x format: itemType="Map" + CIMPATH catalogPath, root-level CIMMap (no wrapper)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadMapsAsync_Pro3xFormat_ReadsRootLevelMapAndResolvesExternalLayer()
    {
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                {
                  "projectItems": [
                    { "itemType": "Map", "catalogPath": "CIMPATH=Maps/Map.json", "name": "Map" }
                  ]
                }
                """),
            ("Maps/Map.json", """
                {
                  "type": "CIMMap",
                  "name": "Strom LD 3x",
                  "layers": [ "CIMPATH=Layers/Roads.json" ]
                }
                """),
            ("Layers/Roads.json", """
                { "type": "CIMFeatureLayer", "name": "Roads" }
                """));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        var result = Assert.Single(results);
        Assert.Equal("Strom LD 3x", result.Map.Name);
        var featureLayer = Assert.IsType<CimFeatureLayer>(Assert.Single(result.Layers));
        Assert.Equal("Roads", featureLayer.Name);
    }

    [Fact]
    public async Task ReadMapsAsync_ProjectItemWithoutUriOrCatalogPath_IsSkipped()
    {
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                { "projectItems": [ { "itemType": "Map", "name": "Broken" } ] }
                """));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        Assert.Empty(results);
    }

    // -----------------------------------------------------------------------
    // Layer resolution: inline vs. external, group-layer children, unsupported types
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadMapsAsync_InlineLayerDefinitions_UsedDirectlyWithoutExternalLookup()
    {
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                { "projectItems": [ { "type": "CIMMapDocument", "URI": "Maps/Map.mapx" } ] }
                """),
            ("Maps/Map.mapx", """
                {
                  "type": "CIMMapDocument",
                  "map": {
                    "type": "CIMMap",
                    "name": "Inline",
                    "layerDefinitions": [
                      { "type": "CIMFeatureLayer", "name": "InlineLayer" }
                    ]
                  }
                }
                """));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        var result = Assert.Single(results);
        var layer = Assert.IsType<CimFeatureLayer>(Assert.Single(result.Layers));
        Assert.Equal("InlineLayer", layer.Name);
    }

    [Fact]
    public async Task ReadMapsAsync_InlineGroupLayer_StillResolvesExternalChildren()
    {
        // Even when the group layer itself is defined inline in the map, its own children can
        // still be referenced externally via CIMPATH and must be resolved.
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                { "projectItems": [ { "type": "CIMMapDocument", "URI": "Maps/Map.mapx" } ] }
                """),
            ("Maps/Map.mapx", """
                {
                  "type": "CIMMapDocument",
                  "map": {
                    "type": "CIMMap",
                    "name": "M",
                    "layerDefinitions": [
                      { "type": "CIMGroupLayer", "name": "Group", "layers": [ "CIMPATH=Layers/Child.lyrx" ] }
                    ]
                  }
                }
                """),
            ("Layers/Child.lyrx", """
                { "layerDefinitions": [ { "type": "CIMFeatureLayer", "name": "Child" } ] }
                """));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        var group = Assert.IsType<CimGroupLayer>(Assert.Single(results[0].Layers));
        var child = Assert.IsType<CimFeatureLayer>(Assert.Single(group.LayerDefinitions!));
        Assert.Equal("Child", child.Name);
    }

    [Fact]
    public async Task ReadMapsAsync_ExternalGroupLayer_ResolvesNestedChildrenRecursively()
    {
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                { "projectItems": [ { "type": "CIMMapDocument", "URI": "Maps/Map.mapx" } ] }
                """),
            ("Maps/Map.mapx", """
                {
                  "type": "CIMMapDocument",
                  "map": { "type": "CIMMap", "name": "M", "layers": [ "CIMPATH=Layers/Outer.lyrx" ] }
                }
                """),
            ("Layers/Outer.lyrx", """
                {
                  "layerDefinitions": [
                    { "type": "CIMGroupLayer", "name": "Outer", "layers": [ "CIMPATH=Layers/Inner.lyrx" ] }
                  ]
                }
                """),
            ("Layers/Inner.lyrx", """
                {
                  "layerDefinitions": [
                    { "type": "CIMGroupLayer", "name": "Inner", "layers": [ "CIMPATH=Layers/Leaf.lyrx" ] }
                  ]
                }
                """),
            ("Layers/Leaf.lyrx", """
                { "layerDefinitions": [ { "type": "CIMFeatureLayer", "name": "Leaf" } ] }
                """));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        var outer = Assert.IsType<CimGroupLayer>(Assert.Single(results[0].Layers));
        Assert.Equal("Outer", outer.Name);
        var inner = Assert.IsType<CimGroupLayer>(Assert.Single(outer.LayerDefinitions!));
        Assert.Equal("Inner", inner.Name);
        var leaf = Assert.IsType<CimFeatureLayer>(Assert.Single(inner.LayerDefinitions!));
        Assert.Equal("Leaf", leaf.Name);
    }

    [Fact]
    public async Task ReadMapsAsync_StandaloneLayerFile_NoWrapper_IsTypeDispatchedDirectly()
    {
        // ArcGIS Pro 3.x sometimes stores a layer as a standalone JSON file (the CIM object
        // directly, no CimLayerDocument "layerDefinitions" wrapper array).
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                { "projectItems": [ { "type": "CIMMapDocument", "URI": "Maps/Map.mapx" } ] }
                """),
            ("Maps/Map.mapx", """
                {
                  "type": "CIMMapDocument",
                  "map": { "type": "CIMMap", "name": "M", "layers": [ "CIMPATH=Layers/Roads.json" ] }
                }
                """),
            ("Layers/Roads.json", """
                { "type": "CIMFeatureLayer", "name": "Roads" }
                """));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        var layer = Assert.IsType<CimFeatureLayer>(Assert.Single(results[0].Layers));
        Assert.Equal("Roads", layer.Name);
    }

    [Fact]
    public async Task ReadMapsAsync_UnsupportedLayerType_IsSkippedWithoutWarning()
    {
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                { "projectItems": [ { "type": "CIMMapDocument", "URI": "Maps/Map.mapx" } ] }
                """),
            ("Maps/Map.mapx", """
                {
                  "type": "CIMMapDocument",
                  "map": { "type": "CIMMap", "name": "M", "layers": [ "CIMPATH=Layers/Anno.json" ] }
                }
                """),
            ("Layers/Anno.json", """
                { "type": "CIMAnnotationLayer", "name": "Anno" }
                """));
        var warnings = new List<string>();
        var reader = new AprxReader(aprx.Path, warn: warnings.Add);

        var results = await reader.ReadMapsAsync();

        Assert.Empty(results[0].Layers);
        Assert.Empty(warnings);
    }

    [Fact]
    public async Task ReadMapsAsync_MissingReferencedLayerEntry_IsSkipped()
    {
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                { "projectItems": [ { "type": "CIMMapDocument", "URI": "Maps/Map.mapx" } ] }
                """),
            ("Maps/Map.mapx", """
                {
                  "type": "CIMMapDocument",
                  "map": { "type": "CIMMap", "name": "M", "layers": [ "CIMPATH=Layers/DoesNotExist.lyrx" ] }
                }
                """));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        Assert.Empty(results[0].Layers);
    }

    // -----------------------------------------------------------------------
    // Path handling
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadMapsAsync_CaseInsensitiveEntryLookup_StillResolves()
    {
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                { "projectItems": [ { "type": "CIMMapDocument", "URI": "Maps/Map.mapx" } ] }
                """),
            ("Maps/Map.mapx", """
                {
                  "type": "CIMMapDocument",
                  "map": { "type": "CIMMap", "name": "M", "layers": [ "CIMPATH=Layers/roads.lyrx" ] }
                }
                """),
            ("Layers/ROADS.LYRX", """
                { "layerDefinitions": [ { "type": "CIMFeatureLayer", "name": "Roads" } ] }
                """));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        var layer = Assert.IsType<CimFeatureLayer>(Assert.Single(results[0].Layers));
        Assert.Equal("Roads", layer.Name);
    }

    [Fact]
    public async Task ReadMapsAsync_CimPathWithBackslashes_IsNormalizedToForwardSlashes()
    {
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                { "projectItems": [ { "type": "CIMMapDocument", "URI": "Maps/Map.mapx" } ] }
                """),
            ("Maps/Map.mapx", """
                {
                  "type": "CIMMapDocument",
                  "map": { "type": "CIMMap", "name": "M", "layers": [ "CIMPATH=Layers\\Roads.lyrx" ] }
                }
                """),
            ("Layers/Roads.lyrx", """
                { "layerDefinitions": [ { "type": "CIMFeatureLayer", "name": "Roads" } ] }
                """));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        var layer = Assert.IsType<CimFeatureLayer>(Assert.Single(results[0].Layers));
        Assert.Equal("Roads", layer.Name);
    }

    // -----------------------------------------------------------------------
    // Error tolerance
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadMapsAsync_MalformedLayerJson_WarnsAndSkipsThatEntry()
    {
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                { "projectItems": [ { "type": "CIMMapDocument", "URI": "Maps/Map.mapx" } ] }
                """),
            ("Maps/Map.mapx", """
                {
                  "type": "CIMMapDocument",
                  "map": { "type": "CIMMap", "name": "M", "layers": [ "CIMPATH=Layers/Broken.lyrx" ] }
                }
                """),
            ("Layers/Broken.lyrx", "{ not valid json !!"));
        var warnings = new List<string>();
        var reader = new AprxReader(aprx.Path, warn: warnings.Add);

        var results = await reader.ReadMapsAsync();

        Assert.Empty(results[0].Layers);
        Assert.Contains(warnings, w => w.Contains("JSON error"));
    }

    // -----------------------------------------------------------------------
    // Fallback scan when no project items match
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadMapsAsync_NoMatchingProjectItems_FallsBackToScanningMapxFiles()
    {
        using var aprx = TempAprxFile.Create(
            ("GISProject.json", """
                { "projectItems": [] }
                """),
            ("Maps/Orphan.mapx", """
                { "type": "CIMMap", "name": "Orphan", "layers": [] }
                """));
        var reader = new AprxReader(aprx.Path);

        var results = await reader.ReadMapsAsync();

        var result = Assert.Single(results);
        Assert.Equal("Orphan", result.Map.Name);
    }
}

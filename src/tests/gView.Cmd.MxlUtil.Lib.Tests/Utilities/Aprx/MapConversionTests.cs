using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;
using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Data;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="AprxMapConverter.Convert"/>'s map-level conversion (name, spatial
/// reference, extent, display defaults) and the feature/group layer tree it builds - visibility,
/// scale ranges, IDs, definition-query precedence, and TOC nesting.
/// </summary>
public class MapConversionTests
{
    private static AprxMapConverter NewConverter(out List<string> warnings, out List<string> infos)
    {
        var w = new List<string>();
        var i = new List<string>();
        warnings = w;
        infos = i;
        return new AprxMapConverter(warn: w.Add, info: i.Add);
    }

    // -----------------------------------------------------------------------
    // Map-level metadata
    // -----------------------------------------------------------------------

    [Fact]
    public void Convert_MapName_IsCopiedFromCimMap()
    {
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(Cim.Map(name: "Strom LD"), []);

        var map = converter.Convert(result);

        Assert.Equal("Strom LD", map.Name);
    }

    [Fact]
    public void Convert_MapNameMissing_DefaultsToImportedMap()
    {
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(Cim.Map(name: null), []);

        var map = converter.Convert(result);

        Assert.Equal("Imported Map", map.Name);
    }

    [Fact]
    public void Convert_DisplayDefaults_AreAlwaysSet()
    {
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(Cim.Map(), []);

        var map = converter.Convert(result);

        Assert.Equal(1000, map.Display.ReferenceScale);
        Assert.Equal(GeoUnits.Meters, map.Display.DisplayUnits);
        Assert.Equal(GeoUnits.Meters, map.Display.MapUnits);
    }

    [Fact]
    public void Convert_SpatialReference_PrefersLatestWkidOverWkid()
    {
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(Cim.Map(spatialReference: Cim.SpatialReference(wkid: 4326, latestWkid: 25832)), []);

        var map = converter.Convert(result);

        Assert.NotNull(map.LayerDefaultSpatialReference);
        Assert.Equal(25832, map.LayerDefaultSpatialReference!.EpsgCode);
    }

    [Fact]
    public void Convert_SpatialReference_FallsBackToWkidWhenNoLatestWkid()
    {
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(Cim.Map(spatialReference: Cim.SpatialReference(wkid: 4326)), []);

        var map = converter.Convert(result);

        Assert.NotNull(map.LayerDefaultSpatialReference);
        Assert.Equal(4326, map.LayerDefaultSpatialReference!.EpsgCode);
    }

    [Fact]
    public void Convert_NoSpatialReference_LeavesLayerDefaultSpatialReferenceUnset()
    {
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(Cim.Map(spatialReference: null), []);

        var map = converter.Convert(result);

        Assert.Null(map.LayerDefaultSpatialReference);
    }

    [Fact]
    public void Convert_MapExtent_TakesPriorityOverDefaultExtent()
    {
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(
            Cim.Map(
                mapExtent: Cim.Envelope(0, 0, 100, 100),
                defaultExtent: Cim.Envelope(1000, 1000, 2000, 2000)),
            []);

        var map = converter.Convert(result);

        // ZoomTo adjusts min/max to fit the display's aspect ratio, but always preserves the
        // envelope's center - that's the one property safe to assert without depending on the
        // (irrelevant here) image width/height.
        Assert.Equal(50, map.Envelope.MinX + (map.Envelope.MaxX - map.Envelope.MinX) / 2, precision: 6);
        Assert.Equal(50, map.Envelope.MinY + (map.Envelope.MaxY - map.Envelope.MinY) / 2, precision: 6);
    }

    [Fact]
    public void Convert_DefaultExtent_UsedWhenNoMapExtent()
    {
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(
            Cim.Map(mapExtent: null, defaultExtent: Cim.Envelope(0, 0, 200, 200)),
            []);

        var map = converter.Convert(result);

        Assert.Equal(100, map.Envelope.MinX + (map.Envelope.MaxX - map.Envelope.MinX) / 2, precision: 6);
        Assert.Equal(100, map.Envelope.MinY + (map.Envelope.MaxY - map.Envelope.MinY) / 2, precision: 6);
    }

    // -----------------------------------------------------------------------
    // Feature layer basics
    // -----------------------------------------------------------------------

    [Fact]
    public void Convert_FeatureLayer_IsAddedToMap()
    {
        var converter = NewConverter(out _, out _);
        var cimLayer = Cim.FeatureLayer(name: "Strommast", featureTable: Cim.FeatureTable("Strommast"));
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var layer = Assert.Single(map.MapElements);
        Assert.IsType<FeatureLayer>(layer);
    }

    [Fact]
    public void Convert_FeatureLayer_TocNameMatchesCimName()
    {
        var converter = NewConverter(out _, out _);
        var cimLayer = Cim.FeatureLayer(name: "Strommast", featureTable: Cim.FeatureTable("Strommast"));
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var layer = (FeatureLayer)map.MapElements[0];
        Assert.Equal("Strommast", map.TOC.GetTOCElement(layer)?.Name);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Convert_FeatureLayer_VisibilityIsCopied(bool visible)
    {
        var converter = NewConverter(out _, out _);
        var cimLayer = Cim.FeatureLayer(visibility: visible, featureTable: Cim.FeatureTable());
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var layer = (FeatureLayer)map.MapElements[0];
        Assert.Equal(visible, layer.Visible);
    }

    [Fact]
    public void Convert_FeatureLayer_ScaleRangeIsSwappedFromCim()
    {
        // gView's MinimumScale/MaximumScale are the inverse of CIM's minScale/maxScale (CIM's
        // "maxScale" is the *most zoomed in* bound, i.e. gView's *minimum* display scale).
        var converter = NewConverter(out _, out _);
        var cimLayer = Cim.FeatureLayer(minScale: 50000, maxScale: 500, featureTable: Cim.FeatureTable());
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var layer = (FeatureLayer)map.MapElements[0];
        Assert.Equal(500, layer.MinimumScale);
        Assert.Equal(50000, layer.MaximumScale);
        Assert.Equal(500, layer.MinimumLabelScale);
        Assert.Equal(50000, layer.MaximumLabelScale);
    }

    [Fact]
    public void Convert_FeatureLayer_ServiceLayerIdBecomesLayerId()
    {
        var converter = NewConverter(out _, out _);
        var cimLayer = Cim.FeatureLayer(serviceLayerId: 42, featureTable: Cim.FeatureTable());
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var layer = (FeatureLayer)map.MapElements[0];
        Assert.Equal(42, layer.ID);
    }

    [Fact]
    public void Convert_FeatureLayer_TitleComesFromDataConnectionDataset()
    {
        var converter = NewConverter(out _, out _);
        var cimLayer = Cim.FeatureLayer(featureTable: Cim.FeatureTable(dataset: "GIS.DBO.Strommast"));
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var layer = (FeatureLayer)map.MapElements[0];
        Assert.Equal("GIS.DBO.Strommast", layer.Title);
        Assert.Equal("GIS.DBO.Strommast", layer.FeatureClass!.Name);
    }

    // -----------------------------------------------------------------------
    // Definition query precedence
    // -----------------------------------------------------------------------

    [Fact]
    public void Convert_DefinitionExpression_FeatureTableTakesPriorityOverLayerLevel()
    {
        var converter = NewConverter(out _, out _);
        var cimLayer = Cim.FeatureLayer(
            definitionExpression: "LAYER_LEVEL = 1",
            featureTable: Cim.FeatureTable(definitionExpression: "TABLE_LEVEL = 1"));
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var layer = (FeatureLayer)map.MapElements[0];
        Assert.Equal("TABLE_LEVEL = 1", layer.FilterQuery.WhereClause);
    }

    [Fact]
    public void Convert_DefinitionExpression_FallsBackToLayerLevelWhenFeatureTableHasNone()
    {
        var converter = NewConverter(out _, out _);
        var cimLayer = Cim.FeatureLayer(
            definitionExpression: "LAYER_LEVEL = 1",
            featureTable: Cim.FeatureTable(definitionExpression: null));
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var layer = (FeatureLayer)map.MapElements[0];
        Assert.Equal("LAYER_LEVEL = 1", layer.FilterQuery.WhereClause);
    }

    [Fact]
    public void Convert_NoDefinitionExpression_FilterQueryIsNull()
    {
        var converter = NewConverter(out _, out _);
        var cimLayer = Cim.FeatureLayer(featureTable: Cim.FeatureTable(definitionExpression: null));
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var layer = (FeatureLayer)map.MapElements[0];
        Assert.Null(layer.FilterQuery);
    }

    // -----------------------------------------------------------------------
    // Group layer nesting
    // -----------------------------------------------------------------------

    [Fact]
    public void Convert_GroupLayer_IsAddedWithChildren()
    {
        var converter = NewConverter(out _, out _);
        var group = Cim.GroupLayer(name: "Strom", children: [Cim.FeatureLayer(name: "Mast", featureTable: Cim.FeatureTable())]);
        var result = new AprxMapResult(Cim.Map(), [group]);

        var map = converter.Convert(result);

        Assert.Equal(2, map.MapElements.Count); // the group itself + its one child, both flattened into MapElements
        var groupLayer = Assert.IsType<GroupLayer>(map.MapElements[0]);
        Assert.Equal("Strom", groupLayer.Title);
        Assert.Single(groupLayer.ChildLayers);
    }

    [Fact]
    public void Convert_GroupLayer_ChildFeatureLayerTocNameIsSet()
    {
        var converter = NewConverter(out _, out _);
        var group = Cim.GroupLayer(name: "Strom", children: [Cim.FeatureLayer(name: "Mast", featureTable: Cim.FeatureTable())]);
        var result = new AprxMapResult(Cim.Map(), [group]);

        var map = converter.Convert(result);

        var groupLayer = (GroupLayer)map.MapElements[0];
        var child = (FeatureLayer)groupLayer.ChildLayers[0];
        Assert.Equal("Mast", map.TOC.GetTOCElement(child)?.Name);
    }

    [Fact]
    public void Convert_NestedGroupLayers_AreFlattenedIntoMapElementsButKeepHierarchy()
    {
        var converter = NewConverter(out _, out _);
        var innerGroup = Cim.GroupLayer(name: "Inner", children: [Cim.FeatureLayer(name: "Leaf", featureTable: Cim.FeatureTable())]);
        var outerGroup = Cim.GroupLayer(name: "Outer", children: [innerGroup]);
        var result = new AprxMapResult(Cim.Map(), [outerGroup]);

        var map = converter.Convert(result);

        Assert.Equal(3, map.MapElements.Count); // Outer, Inner, Leaf

        var outer = Assert.IsType<GroupLayer>(map.MapElements[0]);
        Assert.Equal("Outer", outer.Title);

        var inner = Assert.IsType<GroupLayer>(outer.ChildLayers[0]);
        Assert.Equal("Inner", inner.Title);

        var leaf = Assert.IsType<FeatureLayer>(inner.ChildLayers[0]);
        Assert.Equal("Leaf", map.TOC.GetTOCElement(leaf)?.Name);
    }

    [Fact]
    public void Convert_LayersAreAddedInDocumentOrder()
    {
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(Cim.Map(), [
            Cim.FeatureLayer(name: "First", featureTable: Cim.FeatureTable()),
            Cim.FeatureLayer(name: "Second", featureTable: Cim.FeatureTable()),
        ]);

        var map = converter.Convert(result);

        Assert.Equal("First", map.TOC.GetTOCElement((FeatureLayer)map.MapElements[0])?.Name);
        Assert.Equal("Second", map.TOC.GetTOCElement((FeatureLayer)map.MapElements[1])?.Name);
    }
}

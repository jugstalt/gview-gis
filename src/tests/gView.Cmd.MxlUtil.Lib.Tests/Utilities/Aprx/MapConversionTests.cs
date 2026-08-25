using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;
using gView.Framework.Cartography;
using gView.Framework.Cartography.Rendering;
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
    public void Convert_NoReferenceScaleInCim_DisablesReferenceScaleSymbolSizing()
    {
        // ArcGIS Pro only ties symbol/text sizes to ground distance when the author
        // explicitly sets a reference scale. Most authored maps never do, and forcing an
        // arbitrary one (e.g. always 1:1000) makes symbols render at the wrong size compared
        // to ArcGIS Pro as soon as the map is viewed at any other scale.
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(Cim.Map(), []);

        var map = converter.Convert(result);

        Assert.True(map.Display.ReferenceScale <= 0);
    }

    [Fact]
    public void Convert_ReferenceScaleInCim_IsCopiedToDisplay()
    {
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(Cim.Map(referenceScale: 2500), []);

        var map = converter.Convert(result);

        Assert.Equal(2500, map.Display.ReferenceScale);
    }

    [Fact]
    public void Convert_NoSpatialReference_MapUnitsDefaultToMeters()
    {
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(Cim.Map(), []);

        var map = converter.Convert(result);

        Assert.Equal(GeoUnits.Meters, map.Display.DisplayUnits);
        Assert.Equal(GeoUnits.Meters, map.Display.MapUnits);
    }

    [Fact]
    public void Convert_ProjectedSpatialReference_MapUnitsMatchSpatialReference()
    {
        // EPSG:25832 (ETRS89 / UTM zone 32N) is a projected, metric CRS.
        var converter = NewConverter(out _, out _);
        var result = new AprxMapResult(Cim.Map(spatialReference: Cim.SpatialReference(wkid: 25832)), []);

        var map = converter.Convert(result);

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
    public void Convert_GroupLayer_ServiceLayerIdBecomesLayerId()
    {
        var converter = NewConverter(out _, out _);
        var group = Cim.GroupLayer(name: "Strom", serviceLayerId: 12, children: [Cim.FeatureLayer(featureTable: Cim.FeatureTable())]);
        var result = new AprxMapResult(Cim.Map(), [group]);

        var map = converter.Convert(result);

        var groupLayer = Assert.IsType<GroupLayer>(map.MapElements[0]);
        Assert.Equal(12, groupLayer.ID);
    }

    [Fact]
    public void Convert_NestedGroupLayer_ServiceLayerIdBecomesLayerId()
    {
        var converter = NewConverter(out _, out _);
        var inner = Cim.GroupLayer(name: "Inner", serviceLayerId: 34, children: [Cim.FeatureLayer(featureTable: Cim.FeatureTable())]);
        var outer = Cim.GroupLayer(name: "Outer", serviceLayerId: 12, children: [inner]);
        var result = new AprxMapResult(Cim.Map(), [outer]);

        var map = converter.Convert(result);

        var outerLayer = Assert.IsType<GroupLayer>(map.MapElements[0]);
        Assert.Equal(12, outerLayer.ID);
        var innerLayer = Assert.IsType<GroupLayer>(outerLayer.ChildLayers[0]);
        Assert.Equal(34, innerLayer.ID);
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

    [Fact]
    public void Convert_UnsupportedLayerType_IsSkippedWithWarning()
    {
        // A layer type not in CimBaseLayer's [JsonDerivedType] list (e.g. CIMRasterLayer)
        // deserializes down to the bare base type - AddLayer's switch doesn't have a case for
        // it and must not just drop it silently.
        var converter = NewConverter(out var warnings, out _);
        var result = new AprxMapResult(Cim.Map(), [
            new CimBaseLayer { Name = "Ortho" },
            Cim.FeatureLayer(name: "Kept", featureTable: Cim.FeatureTable()),
        ]);

        var map = converter.Convert(result);

        var layer = Assert.Single(map.MapElements);
        Assert.Equal("Kept", map.TOC.GetTOCElement((FeatureLayer)layer)?.Name);
        Assert.Contains(warnings, w => w.Contains("Ortho"));
    }

    // -----------------------------------------------------------------------
    // Annotation layers
    // -----------------------------------------------------------------------

    [Fact]
    public void Convert_AnnotationLayer_BecomesGroupWithOneSubLayerPerAnnotationClass()
    {
        // ArcGIS Pro/Server publish an annotation layer as a group named after the layer
        // itself, with the actual rendering happening in a child layer per annotation class
        // (e.g. "FW-Text" > "Standard") - not as one flat layer.
        var converter = NewConverter(out var warnings, out _);
        var cimLayer = new CimAnnotationLayer
        {
            Name = "FW-Text",
            Visibility = true,
            ServiceLayerId = 57,
            FeatureTable = Cim.FeatureTable(dataset: "FW_A_ZUTEXT"),
            SubLayers = [new CimAnnotationSubLayer { Name = "Standard", SubLayerId = "0", ServiceLayerId = 56 }]
        };
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var groupLayer = Assert.IsType<GroupLayer>(Assert.Single(map.MapElements, e => e is GroupLayer));
        Assert.Equal("FW-Text", map.TOC.GetTOCElement(groupLayer)?.Name);
        Assert.Equal(57, groupLayer.ID); // must match ArcGIS Server's published group layer ID, not just its child's

        var layer = Assert.IsType<FeatureLayer>(Assert.Single(groupLayer.ChildLayers));
        Assert.Equal("Standard", map.TOC.GetTOCElement(layer)?.Name);
        Assert.Equal(56, layer.ID);
        Assert.Null(layer.FeatureRenderer); // the (usually invisible) annotation geometry itself is never drawn
        Assert.Equal("AnnotationClassID = 0", layer.FilterQuery?.WhereClause);

        var renderer = Assert.IsType<SimpleLabelRenderer>(layer.LabelRenderer);
        Assert.Equal("TextString", renderer.FieldName);
        Assert.False(renderer.UseExpression);
        Assert.Equal(RenderLabelPriority.Always, renderer.LabelPriority);
        Assert.Equal("Angle", renderer.SymbolRotation.RotationFieldName);
        Assert.NotNull(renderer.TextSymbol);
        Assert.Empty(warnings);
    }

    [Fact]
    public void Convert_AnnotationLayer_MultipleSubLayers_EachGetsOwnFilteredChildLayer()
    {
        var converter = NewConverter(out _, out _);
        var cimLayer = new CimAnnotationLayer
        {
            Name = "FW-Text",
            SubLayers =
            [
                new CimAnnotationSubLayer { Name = "Standard", SubLayerId = "0", ServiceLayerId = 56 },
                new CimAnnotationSubLayer { Name = "Klein", SubLayerId = "1", ServiceLayerId = 58 },
            ]
        };
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var groupLayer = Assert.IsType<GroupLayer>(Assert.Single(map.MapElements, e => e is GroupLayer));
        Assert.Equal(2, groupLayer.ChildLayers.Count);

        var standard = Assert.IsType<FeatureLayer>(groupLayer.ChildLayers[0]);
        Assert.Equal("AnnotationClassID = 0", standard.FilterQuery?.WhereClause);

        var klein = Assert.IsType<FeatureLayer>(groupLayer.ChildLayers[1]);
        Assert.Equal("AnnotationClassID = 1", klein.FilterQuery?.WhereClause);
    }

    [Fact]
    public void Convert_AnnotationLayer_DefinitionExpression_CombinedWithAnnotationClassFilter()
    {
        var converter = NewConverter(out _, out _);
        var cimLayer = new CimAnnotationLayer
        {
            Name = "FW-Text",
            FeatureTable = Cim.FeatureTable(definitionExpression: "Status = 1"),
            SubLayers = [new CimAnnotationSubLayer { Name = "Standard", SubLayerId = "0", ServiceLayerId = 56 }]
        };
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var groupLayer = Assert.IsType<GroupLayer>(Assert.Single(map.MapElements, e => e is GroupLayer));
        var layer = Assert.IsType<FeatureLayer>(Assert.Single(groupLayer.ChildLayers));
        Assert.Equal("Status = 1 AND AnnotationClassID = 0", layer.FilterQuery?.WhereClause);
    }

    [Fact]
    public void Convert_AnnotationLayer_NoSubLayers_FallsBackToSingleChildLayerNoFilter()
    {
        var converter = NewConverter(out _, out _);
        var cimLayer = new CimAnnotationLayer { Name = "FW-Text", SubLayers = null };
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var groupLayer = Assert.IsType<GroupLayer>(Assert.Single(map.MapElements, e => e is GroupLayer));
        var layer = Assert.IsType<FeatureLayer>(Assert.Single(groupLayer.ChildLayers));
        Assert.Equal("FW-Text", map.TOC.GetTOCElement(layer)?.Name);
        Assert.Null(layer.FilterQuery);
    }

    [Fact]
    public void Convert_AnnotationLayer_SubLayerVisibilityIsCopied()
    {
        var converter = NewConverter(out _, out _);
        var cimLayer = new CimAnnotationLayer
        {
            Name = "FW-Text",
            SubLayers = [new CimAnnotationSubLayer { Name = "Standard", SubLayerId = "0", Visibility = false }]
        };
        var result = new AprxMapResult(Cim.Map(), [cimLayer]);

        var map = converter.Convert(result);

        var groupLayer = Assert.IsType<GroupLayer>(Assert.Single(map.MapElements, e => e is GroupLayer));
        var layer = Assert.IsType<FeatureLayer>(Assert.Single(groupLayer.ChildLayers));
        Assert.False(layer.Visible);
    }

    [Fact]
    public void Convert_AnnotationLayer_InsideGroupLayer_NestsAnnotationGroupInsideParentGroup()
    {
        var converter = NewConverter(out _, out _);
        var group = Cim.GroupLayer(name: "Beschriftung", children: [
            new CimAnnotationLayer
            {
                Name = "FW-Text",
                SubLayers = [new CimAnnotationSubLayer { Name = "Standard", SubLayerId = "0" }]
            }
        ]);
        var result = new AprxMapResult(Cim.Map(), [group]);

        var map = converter.Convert(result);

        var parentGroup = Assert.IsType<GroupLayer>(map.MapElements[0]);
        var annotationGroup = Assert.IsType<GroupLayer>(Assert.Single(parentGroup.ChildLayers));
        Assert.Equal("FW-Text", map.TOC.GetTOCElement(annotationGroup)?.Name);

        var child = Assert.IsType<FeatureLayer>(Assert.Single(annotationGroup.ChildLayers));
        Assert.Equal("Standard", map.TOC.GetTOCElement(child)?.Name);
    }
}

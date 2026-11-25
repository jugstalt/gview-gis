using gView.Framework.Data;
using gView.Server.EndPoints.GeoJsonService;
using gView.Server.EndPoints.GeoJsonService.Extensions;

namespace gView.Server.Tests;

/// <summary>
/// The layer ids of GetMap and identify: a group shows its layers, -id/+id only touch that layer (or group)
/// </summary>
public class EnumerableExtensionsTests
{
    private static (GroupLayer group, FeatureLayer a, FeatureLayer b, FeatureLayer other) Layers()
    {
        var group = new GroupLayer("group") { ID = 3 };
        var a = new FeatureLayer() { ID = 0, GroupLayer = group };
        var b = new FeatureLayer() { ID = 1, GroupLayer = group };
        var other = new FeatureLayer() { ID = 5 };
        return (group, a, b, other);
    }

    [Fact]
    public void ExcludeOneLayer_KeepsTheOtherLayersOfItsGroup()
    {
        var (_, a, b, _) = Layers();
        string[] ids = ["-0"];

        Assert.Equal(MapLayerVisibility.Exclude, ids.LayerOrParentIsInArray(a, null));
        Assert.Equal(MapLayerVisibility.Invisible, ids.LayerOrParentIsInArray(b, null));  // before: excluded by its group
    }

    [Fact]
    public void Group_ShowsAndHidesItsLayers()
    {
        var (_, a, b, other) = Layers();

        Assert.Equal(MapLayerVisibility.Visible, new[] { "3" }.LayerOrParentIsInArray(b, null));
        Assert.Equal(MapLayerVisibility.Exclude, new[] { "-3" }.LayerOrParentIsInArray(a, null));
        Assert.Equal(MapLayerVisibility.Invisible, new[] { "3" }.LayerOrParentIsInArray(other, null));
    }

    [Theory]
    [InlineData(null, MapLayerVisibilityPattern.Defaults)]
    [InlineData(new[] { "0", "1" }, MapLayerVisibilityPattern.Normal)]
    [InlineData(new[] { "-0", "+2" }, MapLayerVisibilityPattern.IncludeExclude)]
    public void Pattern(string[]? ids, MapLayerVisibilityPattern pattern)
        => Assert.Equal(pattern, ids!.GetVisibilityPattern());
}

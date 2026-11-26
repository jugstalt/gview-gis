using gView.Framework.Core.Exceptions;
using gView.Framework.Geometry;
using gView.GeoJsonService.DTOs;
using gView.Server.EndPoints.GeoJsonService.Extensions;

namespace gView.Server.Tests;

/// <summary>Coordinates and coordinate systems of GeoJson service requests</summary>
public class GeometryExtensionsTests
{
    [Fact]
    public void CheckLatitudes_ProjectedCoordinatesInWgs84_Are400()
    {
        var wgs84 = SpatialReference.FromID("epsg:4326")!;

        var ex = Assert.Throws<MapServerException>(() => new Point(15155, 300038).CheckLatitudes(wgs84, "geometry"));
        Assert.Equal(400, ex.StatusCode);
        Assert.StartsWith("geometry: latitude out of range", ex.Message);
    }

    [Fact]
    public void CheckLatitudes_ValidOrWrappedLongitudeOrProjected_Pass()
    {
        var wgs84 = SpatialReference.FromID("epsg:4326")!;
        var gk = SpatialReference.FromID("epsg:31256")!;

        Assert.NotNull(new Point(16.5, 47.8).CheckLatitudes(wgs84, "geometry"));
        Assert.NotNull(new Envelope(-400, 40, 400, 50).CheckLatitudes(wgs84, "spatialFilter"));  // across the date line
        Assert.NotNull(new Point(15155, 300038).CheckLatitudes(gk, "geometry"));                // projected crs: no check
        Assert.Null(((Point?)null).CheckLatitudes(wgs84, "geometry"));
    }

    [Theory]
    [InlineData("4326", "EPSG:4326")]
    [InlineData("EPSG:31256", "EPSG:31256")]
    [InlineData("urn:ogc:def:crs:EPSG::3857", "EPSG:3857")]
    [InlineData("urn:ogc:def:crs:OGC:1.3:CRS84", "EPSG:4326")]
    [InlineData("WGS84", "")]
    public void CoordinateReferenceSystem_Names(string name, string expected)
        => Assert.Equal(expected, CoordinateReferenceSystem.CreateByName(name).ToSpatialReferenceName());
}

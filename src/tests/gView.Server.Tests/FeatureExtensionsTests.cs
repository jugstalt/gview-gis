using gView.Framework.Data;
using DataFeature = gView.Framework.Core.Data.Feature;
using gView.Framework.Core.Data;
using gView.Framework.Geometry;
using gView.GeoJsonService.DTOs;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using System.Text.Json;

namespace gView.Server.Tests;

/// <summary>How query, identify and find write geometries: bbox, centroid, generalized, rounded</summary>
public class FeatureExtensionsTests
{
    private static DataFeature Square(double size, int pointsPerSide = 1)
    {
        var ring = new Ring();
        void Side(double x0, double y0, double x1, double y1)
        {
            for (int i = 0; i < pointsPerSide; i++)
            {
                double t = (double)i / pointsPerSide;
                ring.AddPoint(new Point(x0 + t * (x1 - x0) + 0.123456, y0 + t * (y1 - y0) + 0.654321));
            }
        }
        Side(0, 0, size, 0); Side(size, 0, size, size); Side(size, size, 0, size); Side(0, size, 0, 0);
        var polygon = new Polygon();
        polygon.AddRing(ring);

        var feature = new DataFeature { Shape = polygon, OID = 7 };
        feature.Fields.Add(new FieldValue("OBJECTID", 7));
        return feature;
    }

    private static double[] PointOf(gView.GeoJsonService.DTOs.Feature feature)
        => JsonSerializer.Deserialize<double[]>(JsonSerializer.Serialize(feature.Geometry!.Coordinates))!;

    [Fact]
    public void Centroid_OfAPolygon_IsInside()
    {
        var feature = Square(100).ToGeoJsonFeature("OBJECTID", GeometryResult.Centroid);

        Assert.Equal(GeometryType.Point, feature.Geometry!.Type);
        var xy = PointOf(feature);
        Assert.InRange(xy[0], 0, 101);
        Assert.InRange(xy[1], 0, 101);
        Assert.Equal(7, feature.Oid);
    }

    [Fact]
    public void Precision_RoundsTheCoordinates()
    {
        var feature = Square(100).ToGeoJsonFeature("OBJECTID", GeometryResult.Centroid, new GeometryOptions(Precision: 0));

        Assert.All(PointOf(feature), v => Assert.Equal(Math.Round(v), v));
    }

    [Fact]
    public void MaxAllowableOffset_DropsVertices()
    {
        string Coordinates(GeometryOptions? options)
            => JsonSerializer.Serialize(Square(100, pointsPerSide: 50).ToGeoJsonFeature("OBJECTID", GeometryResult.Geometry, options).Geometry!.Coordinates);

        Assert.True(Coordinates(new GeometryOptions(MaxAllowableOffset: 5)).Length < Coordinates(null).Length);
    }

    [Fact]
    public void None_HasNoGeometry_BBoxIsAnEnvelope()
    {
        Assert.Null(Square(10).ToGeoJsonFeature("OBJECTID", GeometryResult.None).Geometry);
        Assert.NotNull(Square(10).ToGeoJsonFeature("OBJECTID", GeometryResult.BBox).BBox);
    }
}

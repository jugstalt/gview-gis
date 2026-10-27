using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Engine;
using Path = gView.Framework.Geometry.Path;

namespace gView.Framework.Geometry.Tests.Engine;

/// <summary>
/// Tests for SpatialRelation.Check on SpatialNET (ADR 0002, phase 2, step 1): the comparison of gView's
/// own implementation (<c>CheckClassic</c>) with the SpatialNET variant (<see cref="SpatialNetBridge.Check"/>),
/// and the cases where both differ on purpose. The tests call both variants directly, so they do not depend
/// on the global switch.
/// </summary>
public class SpatialRelationTests
{
    private static readonly spatialRelation[] Relations =
    {
        spatialRelation.SpatialRelationIntersects,
        spatialRelation.SpatialRelationContains,
        spatialRelation.SpatialRelationWithin,
    };

    #region Geometry helpers

    private static Ring RingOf(params (double X, double Y)[] points) =>
        new(points.Select(p => (IPoint)new Point(p.X, p.Y)).ToList());

    private static Polygon PolygonOf(params Ring[] rings)
    {
        var polygon = new Polygon();
        foreach (var ring in rings)
        {
            polygon.AddRing(ring);
        }

        return polygon;
    }

    private static Polygon Rectangle(double minX, double minY, double maxX, double maxY) =>
        PolygonOf(RingOf((minX, minY), (maxX, minY), (maxX, maxY), (minX, maxY)));

    private static Polyline Line(params (double X, double Y)[] points) =>
        new(new Path(points.Select(p => (IPoint)new Point(p.X, p.Y)).ToList()));

    #endregion

    #region Comparison old against new

    /// <summary>
    /// Random, deterministic geometries in general position: no vertex lies exactly on another geometry's
    /// boundary, so both implementations must agree on every relation.
    /// </summary>
    [Theory]
    [InlineData(0.0, 0.0, 100.0)]                // plain coordinates
    [InlineData(15.0, 47.0, 0.01)]               // geographic, degrees
    [InlineData(533000.0, 5212000.0, 1000.0)]    // projected, metres (UTM 33N)
    public void Classic_And_SpatialNet_Agree_InGeneralPosition(double originX, double originY, double extent)
    {
        var random = new Random(4711);
        (double X, double Y) RandomPoint() =>
            (originX + random.NextDouble() * extent, originY + random.NextDouble() * extent);

        IGeometry RandomTriangle() => PolygonOf(RingOf(RandomPoint(), RandomPoint(), RandomPoint()));

        IGeometry RandomEnvelope()
        {
            var (x1, y1) = RandomPoint();
            var (x2, y2) = RandomPoint();
            return new Envelope(Math.Min(x1, x2), Math.Min(y1, y2), Math.Max(x1, x2), Math.Max(y1, y2));
        }

        IGeometry RandomSquareWithHole()
        {
            var (x, y) = RandomPoint();
            double size = extent * 0.4, hole = size * 0.4, offset = size * 0.3;
            return PolygonOf(
                RingOf((x, y), (x + size, y), (x + size, y + size), (x, y + size)),
                RingOf((x + offset, y + offset), (x + offset + hole, y + offset), (x + offset + hole, y + offset + hole), (x + offset, y + offset + hole)));
        }

        var filters = new List<IGeometry>();
        var candidates = new List<IGeometry>();
        for (int i = 0; i < 30; i++)
        {
            filters.Add(RandomTriangle());
            filters.Add(RandomEnvelope());
            filters.Add(RandomSquareWithHole());

            var (px, py) = RandomPoint();
            candidates.Add(new Point(px, py));
            candidates.Add(Line(RandomPoint(), RandomPoint(), RandomPoint()));
            candidates.Add(RandomTriangle());
            candidates.Add(RandomSquareWithHole());
        }

        var mismatches = new List<string>();
        var hits = Relations.ToDictionary(r => r, _ => 0);
        int knownDeviations = 0, checks = 0;
        foreach (var relation in Relations)
        {
            foreach (var filter in filters)
            {
                foreach (var candidate in candidates)
                {
                    bool classic = SpatialRelation.CheckClassic(relation, filter, candidate);
                    bool? spatialNet = SpatialNetBridge.Check(relation, filter, candidate);
                    checks++;

                    if (spatialNet == classic)
                    {
                        hits[relation] += classic ? 1 : 0;
                    }
                    else if (IsKnownClassicDeviation(relation, filter, candidate))
                    {
                        knownDeviations++;
                    }
                    else
                    {
                        mismatches.Add($"{relation}: filter {filter.GeometryType} {filter.Envelope.ToBBoxString()}, "
                            + $"candidate {candidate.GeometryType} {candidate.Envelope.ToBBoxString()}: classic {classic}, SpatialNET {spatialNet}");
                    }
                }
            }
        }

        Assert.Empty(mismatches);
        // the comparison is only meaningful if every relation also agrees on positive results
        Assert.All(hits, hit => Assert.True(hit.Value > 0, $"{hit.Key}: no positive result"));
        Assert.True(knownDeviations < checks / 100, $"{knownDeviations} known deviations in {checks} checks");
    }

    // Classic Contains only checks that all candidate vertices are inside and no edges cross. A filter hole
    // that lies completely inside the candidate polygon is missed; SpatialNET (DE-9IM) gets it right.
    private static bool IsKnownClassicDeviation(spatialRelation relation, IGeometry filter, IGeometry candidate)
    {
        (IGeometry container, IGeometry contained) = relation switch
        {
            spatialRelation.SpatialRelationContains => (filter, candidate),
            spatialRelation.SpatialRelationWithin => (candidate, filter),
            _ => (null!, null!),
        };

        return container is IPolygon outer && contained is IPolygon inner
            && outer.Holes.Any(hole => inner.Envelope.Contains(hole.Envelope));
    }

    #endregion

    #region Intended differences

    [Fact]
    public void ContainingPolygon_WithHoleInsideTheCandidate_DoesNotContainIt()
    {
        var filter = PolygonOf(
            RingOf((0, 0), (10, 0), (10, 10), (0, 10)),
            RingOf((4, 4), (6, 4), (6, 6), (4, 6)));
        var candidate = Rectangle(2, 2, 8, 8);

        Assert.True(SpatialRelation.CheckClassic(spatialRelation.SpatialRelationContains, filter, candidate));
        Assert.False(SpatialNetBridge.Check(spatialRelation.SpatialRelationContains, filter, candidate));
    }

    [Fact]
    public void TouchingPolygons_Intersect()
    {
        // DE-9IM: a shared edge is an intersection
        var filter = Rectangle(0, 0, 10, 10);
        var candidate = Rectangle(10, 0, 20, 10);

        Assert.True(SpatialNetBridge.Check(spatialRelation.SpatialRelationIntersects, filter, candidate));
    }

    [Fact]
    public void PolygonInside_TouchingTheBoundaryFromInside_IsContained()
    {
        var filter = Rectangle(0, 0, 10, 10);
        var candidate = Rectangle(0, 2, 5, 8);

        Assert.True(SpatialNetBridge.Check(spatialRelation.SpatialRelationContains, filter, candidate));
    }

    #endregion

    #region Fallback to the classic implementation

    [Theory]
    [InlineData(spatialRelation.SpatialRelationEnvelopeIntersects)]
    [InlineData(spatialRelation.SpatialRelationMapEnvelopeIntersects)]
    public void EnvelopeRelations_AreLeftToClassic(spatialRelation relation)
    {
        // the render path (map envelope) stays on gView's box test
        Assert.Null(SpatialNetBridge.Check(relation, Rectangle(0, 0, 10, 10), new Point(5, 5)));
    }

    [Fact]
    public void DegenerateEnvelope_IsLeftToClassic()
    {
        var clickPoint = new Envelope(5, 5, 5, 5);

        Assert.Null(SpatialNetBridge.Check(spatialRelation.SpatialRelationIntersects, clickPoint, Rectangle(0, 0, 10, 10)));
    }

    [Fact]
    public void AggregateGeometry_IsLeftToClassic()
    {
        var aggregate = new AggregateGeometry();
        aggregate.AddGeometry(new Point(5, 5));

        Assert.Null(SpatialNetBridge.Check(spatialRelation.SpatialRelationIntersects, Rectangle(0, 0, 10, 10), aggregate));
    }

    #endregion

    #region Prepared filter cache

    [Fact]
    public void SameFilter_IsPreparedOnce_AndGivesTheSameResults()
    {
        var filter = Rectangle(0, 0, 10, 10);
        var inside = new Point(5, 5);
        var outside = new Point(15, 5);

        for (int i = 0; i < 3; i++)
        {
            Assert.True(SpatialNetBridge.Check(spatialRelation.SpatialRelationIntersects, filter, inside));
            Assert.False(SpatialNetBridge.Check(spatialRelation.SpatialRelationIntersects, filter, outside));
        }
    }

    #endregion

    #region Switch

    [Theory]
    [InlineData(null, new string[0])]
    [InlineData("", new string[0])]
    [InlineData("relation", new[] { "relation" })]
    [InlineData(" Relation ; buffer,", new[] { "relation", "buffer" })]
    [InlineData("all", new[] { "all" })]
    public void EnvironmentVariable_IsParsedIntoAreas(string? value, string[] expected)
    {
        Assert.Equal(expected.OrderBy(a => a), GeometryEngineOptions.ParseAreas(value!).OrderBy(a => a));
    }

    #endregion
}

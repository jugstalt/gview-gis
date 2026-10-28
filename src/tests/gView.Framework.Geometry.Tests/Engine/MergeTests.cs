using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Engine;
using gView.Framework.Geometry.GeoProcessing;
using gView.Framework.Geometry.GeoProcessing.Clipper;

namespace gView.Framework.Geometry.Tests.Engine;

/// <summary>
/// Tests for merging polygons on SpatialNET (ADR 0002, phase 2, step 3): the comparison with gView's Clipper
/// merge and gView's conventions. Both variants are called directly, so these tests do not depend on the
/// global switch; <see cref="MergeSwitchTests"/> checks the switch itself.
/// </summary>
public class MergeTests
{
    #region Geometry helpers

    private static Ring RingOf(params (double X, double Y)[] points) =>
        new(points.Select(p => (IPoint)new Point(p.X, p.Y)).ToList());

    internal static Polygon Square(double x, double y, double size) =>
        new(RingOf((x, y), (x + size, y), (x + size, y + size), (x, y + size)));

    internal static double Area(IPolygon polygon) =>
        SpatialNET.Measures.Area((SpatialNET.IPolygon2D)polygon);

    #endregion

    #region Comparison old against new

    // Each set is built at an origin and a scale
    private static readonly (string Name, Func<double, double, double, List<IPolygon>> Polygons)[] Sets =
    {
        ("two overlapping squares", (x, y, s) => new List<IPolygon> { Square(x, y, 10 * s), Square(x + 5 * s, y + 5 * s, 10 * s) }),
        ("chain of ten squares", (x, y, s) => Enumerable.Range(0, 10).Select(i => (IPolygon)Square(x + i * 7 * s, y + i * 3 * s, 10 * s)).ToList()),
        ("disjoint squares", (x, y, s) => new List<IPolygon> { Square(x, y, 10 * s), Square(x + 30 * s, y, 10 * s), Square(x, y + 30 * s, 5 * s) }),
        ("squares sharing edges", (x, y, s) => new List<IPolygon> { Square(x, y, 10 * s), Square(x + 10 * s, y, 10 * s), Square(x, y + 10 * s, 10 * s) }),
        ("frame around a hole", (x, y, s) => new List<IPolygon>
        {
            // eight squares around an empty one: the union has a hole
            Square(x, y, 10 * s), Square(x + 20 * s, y, 10 * s), Square(x, y + 20 * s, 10 * s), Square(x + 20 * s, y + 20 * s, 10 * s),
            Square(x + 10 * s, y, 10 * s), Square(x + 10 * s, y + 20 * s, 10 * s), Square(x, y + 10 * s, 10 * s), Square(x + 20 * s, y + 10 * s, 10 * s),
        }),
    };

    public static TheoryData<string> SetNames()
    {
        var data = new TheoryData<string>();
        foreach (var set in Sets)
        {
            data.Add(set.Name);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(SetNames))]
    public void Classic_And_SpatialNet_Merge_CoverTheSameArea(string name)
    {
        var (_, polygons) = Sets.Single(s => s.Name == name);

        // plain and UTM coordinates; integer coordinates are exact on Clipper's 0.001 grid
        foreach (var (x, y, s) in new[] { (0.0, 0.0, 1.0), (533000.0, 5212000.0, 10.0) })
        {
            var classic = polygons(x, y, s).Merge();
            var spatialNet = SpatialNetBridge.MergePolygons(polygons(x, y, s));

            double area = Area(spatialNet);
            double deviation = Area((IPolygon)SpatialNET.Overlays.SymDifference(
                (SpatialNET.IPolygon2D)classic, (SpatialNET.IPolygon2D)spatialNet, factory: GViewGeometryFactory.Instance));

            Assert.True(deviation < 1e-9 * area,
                $"{name} at scale {s}: classic {Area(classic):G9}, SpatialNET {area:G9}, symmetric difference {deviation:G6}");
        }
    }

    [Fact]
    public void Frame_GivesShellAndHole()
    {
        var frame = new List<IPolygon>
        {
            Square(0, 0, 10), Square(20, 0, 10), Square(0, 20, 10), Square(20, 20, 10),   // corners
            Square(10, 0, 10), Square(10, 20, 10), Square(0, 10, 10), Square(20, 10, 10), // edges
        };

        var merged = SpatialNetBridge.MergePolygons(frame);

        Assert.Equal(2, merged.RingCount);
        Assert.Single(merged.Holes);
        Assert.Equal(800, Area(merged), 9);
    }

    #endregion

    #region Geographic coordinates

    [Fact]
    public void Merge_InDegrees_IsExact()
    {
        // two overlapping squares of 0.0002° at 47° N: far below Clipper's 0.001 grid
        var polygons = new List<IPolygon> { Square(15.4, 47.0, 0.0002), Square(15.4001, 47.0001, 0.0002) };

        var merged = SpatialNetBridge.MergePolygons(polygons);

        double exact = 2 * 0.0002 * 0.0002 - 0.0001 * 0.0001;
        Assert.Equal(exact, Area(merged), exact * 1e-6);
    }

    #endregion

    #region Ring orientation

    [Fact]
    public void InnerRing_WithSameOrientation_IsAHole()
    {
        // both rings counter-clockwise and the inner one not typed as Hole: even-odd nesting makes it a hole
        static List<IPolygon> Input()
        {
            var withHole = new Polygon();
            withHole.AddRing(RingOf((0, 0), (10, 0), (10, 10), (0, 10)));
            withHole.AddRing(RingOf((4, 4), (6, 4), (6, 6), (4, 6)));
            return new List<IPolygon> { withHole, Square(20, 0, 10) };
        }

        Assert.Equal(196, Area(SpatialNetBridge.MergePolygons(Input())), 9);

        // the classic merge gets it right too: gView orients the rings before Clipper's positive fill rule
        Assert.Equal(196, Area(Input().Merge()), 9);
    }

    #endregion

    #region gView conventions

    [Fact]
    public void NoPolygons_GiveNull()
    {
        Assert.Null(SpatialNetBridge.MergePolygons(null!));
        Assert.Null(SpatialNetBridge.MergePolygons(new List<IPolygon>()));
        Assert.Null(SpatialNetBridge.MergePolygons(new List<IPolygon> { null! }));
    }

    [Fact]
    public void SinglePolygon_IsReturnedUnchanged()
    {
        var polygon = Square(0, 0, 10);

        Assert.Same(polygon, SpatialNetBridge.MergePolygons(new List<IPolygon> { polygon }));
    }

    #endregion
}

[Collection(nameof(GlobalSwitchCollection))]
public class MergeSwitchTests
{
    [Fact]
    public void Switch_RoutesMergeAndUnionToSpatialNet()
    {
        bool before = GeometryEngineOptions.UseSpatialNetForMerge;
        try
        {
            GeometryEngineOptions.UseSpatialNetForMerge = true;

            // Clipper would round these degree coordinates to its 0.001 grid
            var a = MergeTests.Square(15.4, 47.0, 0.0002);
            var b = MergeTests.Square(15.4001, 47.0001, 0.0002);
            double exact = 2 * 0.0002 * 0.0002 - 0.0001 * 0.0001;

            Assert.Equal(exact, MergeTests.Area(Algorithm.MergePolygons(new List<IPolygon> { a, b })), exact * 1e-6);
            Assert.Equal(exact, MergeTests.Area(Algorithm.FastMergePolygon(new List<IPolygon> { a, b }, null, null)), exact * 1e-6);

            a.Union(b);
            Assert.Equal(exact, MergeTests.Area(a), exact * 1e-6);
        }
        finally
        {
            GeometryEngineOptions.UseSpatialNetForMerge = before;
        }
    }
}

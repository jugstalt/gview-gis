using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;

namespace gView.Framework.Geometry.Tests;

public class PointCollectionTests
{
    [Fact]
    public void GetEnumerator_Generic_EnumeratesAllPoints()
    {
        var multiPoint = new MultiPoint();
        multiPoint.AddPoint(new Point(1, 2));
        multiPoint.AddPoint(new Point(3, 4));

        var points = ((IEnumerable<IPoint>)multiPoint).ToList();

        Assert.Equal(2, points.Count);
        Assert.Equal(3, points[1].X);
        Assert.Equal(4, points[1].Y);
    }
}

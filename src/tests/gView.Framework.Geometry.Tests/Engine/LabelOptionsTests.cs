using gView.Framework.Geometry.Engine;

namespace gView.Framework.Geometry.Tests.Engine;

public class LabelOptionsTests
{
    [Fact]
    public void LabelSpatialNetMinVertices_DefaultIsZero_AllPolygonsUseSpatialNet()
    {
        // not set by the environment variable in the test run
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(GeometryEngineOptions.LabelMinVerticesEnvironmentVariable)))
        {
            Assert.Equal(0, GeometryEngineOptions.LabelSpatialNetMinVertices);
        }
    }
}

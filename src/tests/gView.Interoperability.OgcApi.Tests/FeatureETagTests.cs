using gView.Framework.Geometry;
using gView.Interoperability.OgcApi.Features;

namespace gView.Interoperability.OgcApi.Tests;

public class FeatureETagTests
{
    private static OgcFeature Feature(double x = 16.37, double y = 48.21, string name = "Vienna", int id = 7)
    {
        var feature = new OgcFeature() { Id = id, Geometry = new Point(x, y) };
        feature.Properties.Add(new("NAME", name));
        feature.Properties.Add(new("POP", 2_000_000));
        return feature;
    }

    [Fact]
    public void Compute_SameFeature_SameStrongQuotedTag()
    {
        var a = FeatureETag.Compute(Feature());
        var b = FeatureETag.Compute(Feature());

        Assert.Equal(a, b);
        Assert.StartsWith("\"", a.Value);
        Assert.EndsWith("\"", a.Value);
        Assert.DoesNotContain("W/", a.Value);
    }

    [Fact]
    public void Compute_ChangedProperty_DifferentTag()
        => Assert.NotEqual(FeatureETag.Compute(Feature()), FeatureETag.Compute(Feature(name: "Wien")));

    [Fact]
    public void Compute_ChangedGeometry_DifferentTag()
        => Assert.NotEqual(FeatureETag.Compute(Feature()), FeatureETag.Compute(Feature(x: 16.38)));

    [Fact]
    public void Compute_OtherId_DifferentTag()
        => Assert.NotEqual(FeatureETag.Compute(Feature()), FeatureETag.Compute(Feature(id: 8)));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("*")]
    [InlineData(" * ")]
    public void MatchesIfMatch_NoHeaderOrAny_True(string? ifMatch)
        => Assert.True(FeatureETag.Compute(Feature()).MatchesIfMatch(ifMatch));

    [Fact]
    public void MatchesIfMatch_SameTag_True()
    {
        var tag = FeatureETag.Compute(Feature());

        Assert.True(tag.MatchesIfMatch(tag.Value));
        Assert.True(tag.MatchesIfMatch($"\"other\", {tag.Value}"));
    }

    [Fact]
    public void MatchesIfMatch_OtherOrWeakTag_False()
    {
        var tag = FeatureETag.Compute(Feature());

        Assert.False(tag.MatchesIfMatch(FeatureETag.Compute(Feature(name: "Wien")).Value));
        Assert.False(tag.MatchesIfMatch($"W/{tag.Value}"));
        Assert.False(tag.MatchesIfMatch(tag.Value.Trim('"')));
    }
}

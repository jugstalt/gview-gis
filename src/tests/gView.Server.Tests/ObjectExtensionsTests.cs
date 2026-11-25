using gView.Framework.Core.Exceptions;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using System.Text.Json;

namespace gView.Server.Tests;

public class ObjectExtensionsTests
{
    [Theory]
    [InlineData("5", 5)]
    [InlineData(" 7 ", 7)]
    public void ToObjectId_Text(string text, int expected)
        => Assert.Equal(expected, ((object)text.Trim()).ToObjectId());

    [Fact]
    public void ToObjectId_JsonNumberAndString()
    {
        var ids = JsonSerializer.Deserialize<object[]>("""[1, "2"]""")!;

        Assert.Equal([1, 2], ids.Select(id => id.ToObjectId()));
    }

    [Theory]
    [InlineData("x")]
    [InlineData("1.5")]
    public void ToObjectId_Invalid_IsBadRequest(string text)
    {
        Assert.Throws<MapServerException>(() => ((object)text).ToObjectId());
        Assert.Throws<MapServerException>(() => JsonSerializer.Deserialize<object>($"\"{text}\"").ToObjectId());
    }
}

using gView.Framework.Core.Exceptions;
using gView.GeoJsonService.DTOs;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using System.Text.Json;

namespace gView.Server.Tests;

public class AttributeFilterExtensionsTests
{
    private static AttributeFilter Filter(string where, string parametersJson)
        => new()
        {
            WhereClause = where,
            // as the request body gives them: JsonElement values
            Parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson)
        };

    [Fact]
    public void Resolve_SpecExample_KeyWithoutAt()
    {
        var where = Filter("field1 = @value1 AND field2 > @value2", """{ "value1": "someValue", "value2": 100 }""")
                        .WhereClauseWithResolveParameters();

        Assert.Equal("field1 = 'someValue' AND field2 > 100", where);
    }

    [Fact]
    public void Resolve_KeyWithAt_StillWorks()
    {
        Assert.Equal("ID = 101", Filter("ID = @id", """{ "@id": 101 }""").WhereClauseWithResolveParameters());
    }

    [Fact]
    public void Resolve_WholeNamesOnly_AndQuotesDoubled()
    {
        var where = Filter("A = @id AND B = @id2", """{ "id": "O'Brien", "id2": 2 }""").WhereClauseWithResolveParameters();

        Assert.Equal("A = 'O''Brien' AND B = 2", where);
    }

    [Fact]
    public void Resolve_StringLiteralsAndUnknownNames_StayAsTheyAre()
    {
        var where = Filter("MAIL = 'x@id.at' AND N = @id AND M = @other", """{ "id": 1 }""").WhereClauseWithResolveParameters();

        Assert.Equal("MAIL = 'x@id.at' AND N = 1 AND M = @other", where);
    }

    [Fact]
    public void Resolve_BoolAndNull()
    {
        Assert.Equal("A = 1 AND B IS NULL OR B = NULL",
            Filter("A = @a AND B IS NULL OR B = @b", """{ "a": true, "b": null }""").WhereClauseWithResolveParameters());
    }

    [Fact]
    public void Resolve_ObjectValue_IsBadRequest()
    {
        Assert.Throws<MapServerException>(() => Filter("A = @a", """{ "a": { "x": 1 } }""").WhereClauseWithResolveParameters());
    }
}

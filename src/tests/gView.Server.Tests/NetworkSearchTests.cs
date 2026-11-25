using gView.Server.EndPoints.Network;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace gView.Server.Tests;

public class NetworkSearchTests
{
    private static HttpRequest JsonRequest(string body, string query = "")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.QueryString = new QueryString(query);
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        return context.Request;
    }

    [Fact]
    public async Task Parameters_JsonBody_IncludesF()
    {
        var parameters = await NetworkSearch.Parameters(JsonRequest("""{ "f": "json", "start": "Knoten#1", "maxCost": 100 }"""));

        Assert.Equal("json", parameters["f"]);
        Assert.Equal("Knoten#1", parameters["start"]);
        Assert.Equal("100", parameters["maxCost"]);
        Assert.True(NetworkSearch.HasSearchParameters(parameters));
    }

    [Theory]
    [InlineData("")]
    [InlineData("?token=abc")]
    [InlineData("?start=")]
    public async Task HasSearchParameters_NothingButTheToken_ShowsTheForm(string query)
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString(query);

        Assert.False(NetworkSearch.HasSearchParameters(await NetworkSearch.Parameters(context.Request)));
    }

    [Theory]
    [InlineData("?f=json")]
    [InlineData("?start=Knoten%231")]
    public async Task HasSearchParameters_FOrASearchParameter_RunsTheSearch(string query)
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString(query);

        Assert.True(NetworkSearch.HasSearchParameters(await NetworkSearch.Parameters(context.Request)));
    }
}

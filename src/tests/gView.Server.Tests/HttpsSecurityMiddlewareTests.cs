using gView.Server.Middleware;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;

namespace gView.Server.Tests;

public class HttpsSecurityMiddlewareTests
{
    private static async Task<(HttpContext context, bool nextCalled)> Run(SecurityOptions options, string scheme, string host, string path = "/gview-server/geoservices/rest/services", string query = "?f=json")
    {
        bool nextCalled = false;
        var middleware = new HttpsSecurityMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, options);

        var context = new DefaultHttpContext();
        context.Request.Scheme = scheme;
        context.Request.Host = new HostString(host);
        context.Request.Path = path;
        context.Request.QueryString = new QueryString(query);

        await middleware.InvokeAsync(context);

        return (context, nextCalled);
    }

    [Fact]
    public async Task HttpsRedirect_HttpRequest_RedirectsToHttps()
    {
        var (context, nextCalled) = await Run(new SecurityOptions { HttpsRedirect = true }, "http", "maps.example.com:8080");

        Assert.False(nextCalled);
        Assert.Equal(308, context.Response.StatusCode);
        Assert.Equal("https://maps.example.com/gview-server/geoservices/rest/services?f=json", context.Response.Headers.Location.ToString());
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1:5000")]
    public async Task HttpsRedirect_Localhost_IsNotRedirected(string host)
    {
        var (_, nextCalled) = await Run(new SecurityOptions { HttpsRedirect = true }, "http", host);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task NoRedirect_ByDefault()
    {
        var (context, nextCalled) = await Run(new SecurityOptions(), "http", "maps.example.com");

        Assert.True(nextCalled);
        Assert.False(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
    }

    [Fact]
    public async Task Hsts_HttpsRequest_IsSent()
    {
        var (context, _) = await Run(new SecurityOptions(), "https", "maps.example.com");

        Assert.Equal("max-age=31536000", context.Response.Headers.StrictTransportSecurity.ToString());
    }

    [Fact]
    public async Task Hsts_ForceHttps_IsSentEvenIfRequestLooksLikeHttp()
    {
        var (context, nextCalled) = await Run(new SecurityOptions { ForceHttps = true }, "http", "maps.example.com");

        Assert.True(nextCalled);
        Assert.True(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
    }

    [Theory]
    [InlineData("https", "localhost")]
    public async Task Hsts_Localhost_IsNotSent(string scheme, string host)
    {
        var (context, _) = await Run(new SecurityOptions { ForceHttps = true }, scheme, host);

        Assert.False(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
    }

    [Fact]
    public async Task Hsts_MaxAgeZero_IsNotSent()
    {
        var (context, _) = await Run(new SecurityOptions { HstsMaxAgeDays = 0 }, "https", "maps.example.com");

        Assert.False(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
    }
}

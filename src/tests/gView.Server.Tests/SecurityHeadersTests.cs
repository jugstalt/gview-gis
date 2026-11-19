using gView.Server.AppCode;
using gView.Server.Middleware;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace gView.Server.Tests;

public class SecurityHeadersTests
{
    [Theory]
    [InlineData("/manage", true)]
    [InlineData("/Manage/Login", true)]
    [InlineData("/browseservices/index", true)]
    [InlineData("/token/login", true)]
    [InlineData("/AddMap/folder/name", true)]
    [InlineData("/managed", false)]
    [InlineData("/geoservices/rest/services", false)]
    [InlineData("/geoservices/tokens/generateToken", false)]
    [InlineData("/ogc/service", false)]
    [InlineData("/", false)]
    public void AdminPaths_IsAdminPath(string path, bool expected)
    {
        Assert.Equal(expected, AdminPaths.IsAdminPath(new PathString(path)));
    }

    private static async Task<HttpContext> Run(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        await new SecurityHeadersMiddleware(_ => Task.CompletedTask).InvokeAsync(context);

        return context;
    }

    [Fact]
    public async Task ServicePath_GetsNosniffAndReferrerPolicyOnly()
    {
        var context = await Run("/geoservices/rest/services");

        Assert.Equal("nosniff", context.Response.Headers.XContentTypeOptions.ToString());
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"].ToString());
        Assert.False(context.Response.Headers.ContainsKey("X-Frame-Options"));
        Assert.False(context.Response.Headers.ContainsKey("Content-Security-Policy"));
    }

    [Fact]
    public async Task AdminPath_GetsNoFramingAndCsp()
    {
        var context = await Run("/manage/index");

        Assert.Equal("DENY", context.Response.Headers.XFrameOptions.ToString());
        Assert.Contains("frame-ancestors 'none'", context.Response.Headers.ContentSecurityPolicy.ToString());
        Assert.Contains("default-src 'self'", context.Response.Headers.ContentSecurityPolicy.ToString());
    }

    [Theory]
    [InlineData("POST", null, true, true)]          // browser form/ajax with cookies
    [InlineData("DELETE", null, true, true)]
    [InlineData("GET", null, true, false)]
    [InlineData("POST", "Basic dXNlcjpwdw==", true, false)]  // Carto, scripts
    [InlineData("POST", "Bearer abc", true, false)]
    [InlineData("POST", null, false, false)]        // no cookie: nothing to forge
    public void AntiforgeryForCookieAuth_RequiresValidation(string method, string? authorization, bool withCookie, bool expected)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        if (authorization is not null)
        {
            context.Request.Headers.Authorization = authorization;
        }
        if (withCookie)
        {
            context.Request.Headers.Cookie = "gview5-auth-token=abc";
        }

        Assert.Equal(expected, AntiforgeryForCookieAuthAttribute.RequiresValidation(context.Request));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("*", null)]
    [InlineData("https://maps.example.com/", "https://maps.example.com")]
    public void SecurityOptions_CorsAllowedOrigins(string? origin, string? expected)
    {
        var options = SecurityOptions.FromConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["security:cors:allowed-origins:0"] = origin })
            .Build());

        if (expected is null)
        {
            Assert.Null(options.CorsAllowedOrigins);
        }
        else
        {
            Assert.Equal([expected], options.CorsAllowedOrigins!);
        }
    }
}

using gView.Framework.Core.Exceptions;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;

namespace gView.Server.Tests;

public class SecurityOptionsTests
{
    private static SecurityOptions Options(Dictionary<string, string?> config)
        => SecurityOptions.FromConfiguration(new ConfigurationBuilder().AddInMemoryCollection(config).Build());

    [Fact]
    public void FromConfiguration_Empty_TrustsLoopbackAndPrivateNetworks()
    {
        var options = Options(new());

        Assert.False(options.ForceHttps);
        Assert.False(options.HttpsRedirect);
        Assert.Empty(options.KnownProxies);
        Assert.Contains(options.KnownNetworks, n => n.Contains(IPAddress.Parse("10.1.2.3")));
        Assert.Contains(options.KnownNetworks, n => n.Contains(IPAddress.Parse("::ffff:192.168.1.1")));
        Assert.DoesNotContain(options.KnownNetworks, n => n.Contains(IPAddress.Parse("8.8.8.8")));
    }

    [Fact]
    public void FromConfiguration_KnownProxies_ReplaceDefaultNetworks()
    {
        var options = Options(new()
        {
            ["security:known-proxies:0"] = "10.0.0.5"
        });

        Assert.Empty(options.KnownNetworks);
        Assert.Contains(IPAddress.Parse("10.0.0.5"), options.KnownProxies);
        Assert.Contains(IPAddress.Parse("::ffff:10.0.0.5"), options.KnownProxies);
    }

    [Theory]
    [InlineData("10.0.0.0/8", "10.200.0.1", true)]
    [InlineData("10.0.0.0/8", "11.0.0.1", false)]
    [InlineData("10.0.0.7", "10.0.0.7", true)]
    [InlineData("10.0.0.7", "10.0.0.8", false)]
    [InlineData("2001:db8::/32", "2001:db8::1", true)]
    public void ParseNetwork_ParsesCidrAndSingleAddresses(string network, string address, bool contains)
    {
        Assert.Equal(contains, SecurityOptions.ParseNetwork(network).Contains(IPAddress.Parse(address)));
    }

    [Theory]
    [InlineData("10.0.0.0/33")]
    [InlineData("no-network")]
    public void FromConfiguration_InvalidNetwork_Throws(string network)
    {
        Assert.Throws<MapServerException>(() => Options(new() { ["security:known-networks:0"] = network }));
    }

    [Theory]
    [InlineData("true", null, true)]
    [InlineData(null, "true", true)]
    [InlineData("false", "true", false)]
    [InlineData(null, null, false)]
    public void FromConfiguration_ForceHttps_SectionWinsOverTopLevel(string? section, string? topLevel, bool expected)
    {
        var options = Options(new()
        {
            ["security:force-https"] = section,
            ["force-https"] = topLevel
        });

        Assert.Equal(expected, options.ForceHttps);
    }

    [Theory]
    [InlineData("10.0.0.5", "https", "203.0.113.9")]  // trusted proxy: forwarded values are used
    [InlineData("203.0.113.50", "http", "203.0.113.50")]  // client on the internet: headers are ignored
    public async Task ForwardedHeaders_OnlyFromTrustedProxies(string remoteIp, string expectedScheme, string expectedClientIp)
    {
        var security = Options(new());
        var forwardedOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = null
        };
        forwardedOptions.KnownProxies.Clear();
        forwardedOptions.KnownIPNetworks.Clear();
        foreach (var network in security.KnownNetworks)
        {
            forwardedOptions.KnownIPNetworks.Add(network);
        }

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);
        context.Request.Scheme = "http";
        context.Request.Headers["X-Forwarded-For"] = "203.0.113.9";
        context.Request.Headers["X-Forwarded-Proto"] = "https";

        var middleware = new ForwardedHeadersMiddleware(_ => Task.CompletedTask, NullLoggerFactory.Instance, Microsoft.Extensions.Options.Options.Create(forwardedOptions));
        await middleware.Invoke(context);

        Assert.Equal(expectedScheme, context.Request.Scheme);
        Assert.Equal(expectedClientIp, context.Connection.RemoteIpAddress!.ToString());
    }
}

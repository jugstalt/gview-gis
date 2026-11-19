using gView.Server.AppCode;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace gView.Server.Tests;

public class AdminNetworkOnlyAttributeTests
{
    private static SecurityOptions Options(Dictionary<string, string?>? config = null)
        => SecurityOptions.FromConfiguration(new ConfigurationBuilder().AddInMemoryCollection(config ?? new()).Build());

    private static IActionResult? Run(SecurityOptions options, string clientIp)
    {
        var services = new ServiceCollection()
            .AddSingleton(options)
            .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(clientIp);
        httpContext.Request.Path = "/manage";

        var context = new ResourceExecutingContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>(),
            new List<IValueProviderFactory>());

        new AdminNetworkOnlyAttribute().OnResourceExecuting(context);

        return context.Result;
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    [InlineData("10.20.30.40")]
    [InlineData("192.168.1.10")]
    [InlineData("::ffff:172.16.5.5")]
    public void Default_LocalAndPrivateClients_AreAllowed(string ip)
    {
        Assert.Null(Run(Options(), ip));
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("2001:db8::1")]
    [InlineData("::ffff:8.8.8.8")]
    public void Default_InternetClients_Get404(string ip)
    {
        Assert.IsType<NotFoundResult>(Run(Options(), ip));
    }

    [Fact]
    public void Configured_OnlyListedNetworks()
    {
        var options = Options(new() { ["security:manage:allowed-networks:0"] = "10.1.5.0/24" });

        Assert.Null(Run(options, "10.1.5.9"));
        Assert.IsType<NotFoundResult>(Run(options, "10.1.6.9"));
        Assert.IsType<NotFoundResult>(Run(options, "127.0.0.1"));
    }

    [Fact]
    public void Configured_Star_AllowsEverywhere()
    {
        Assert.Null(Run(Options(new() { ["security:manage:allowed-networks:0"] = "*" }), "8.8.8.8"));
    }

    [Fact]
    public void Oidc_WithoutConfiguredNetworks_AllowsEverywhere()
    {
        Assert.Null(Run(Options(new() { ["Authentication:Type"] = "oidc" }), "8.8.8.8"));
    }

    [Fact]
    public void Oidc_WithConfiguredNetworks_IsRestricted()
    {
        var options = Options(new()
        {
            ["Authentication:Type"] = "oidc",
            ["security:manage:allowed-networks:0"] = "10.0.0.0/8"
        });

        Assert.IsType<NotFoundResult>(Run(options, "8.8.8.8"));
    }

    [Fact]
    public void NoClientAddress_IsNotAllowed()
    {
        Assert.False(Options().IsManageAllowed(null!));
    }

    [Fact]
    public void ConfigurationWarnings_RestrictedAdminNetworksWithDefaultProxies_Warns()
    {
        Assert.Single(Options(new() { ["security:manage:allowed-networks:0"] = "10.1.5.0/24" }).ConfigurationWarnings());
    }

    [Fact]
    public void ConfigurationWarnings_WithKnownProxies_NoWarning()
    {
        var options = Options(new()
        {
            ["security:manage:allowed-networks:0"] = "10.1.5.0/24",
            ["security:known-proxies:0"] = "10.0.0.5"
        });

        Assert.Empty(options.ConfigurationWarnings());
        Assert.Empty(Options().ConfigurationWarnings());
    }
}

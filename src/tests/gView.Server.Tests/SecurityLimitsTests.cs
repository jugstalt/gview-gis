using gView.Server.Extensions.DependencyInjection;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace gView.Server.Tests;

public class SecurityLimitsTests
{
    private static SecurityOptions Options(Dictionary<string, string?> config)
        => SecurityOptions.FromConfiguration(new ConfigurationBuilder().AddInMemoryCollection(config).Build());

    [Theory]
    [InlineData(null, 50L * 1024 * 1024)]
    [InlineData("10", 10L * 1024 * 1024)]
    [InlineData("0", null)]
    [InlineData("abc", 50L * 1024 * 1024)]
    public void MaxRequestBody_FromConfiguration(string? mb, long? expected)
    {
        Assert.Equal(expected, Options(new() { ["security:max-request-body-mb"] = mb }).MaxRequestBodyBytes);
    }

    [Fact]
    public void RateLimit_IsOffByDefault()
    {
        var options = Options(new());

        Assert.False(options.RateLimitEnabled);
        Assert.Equal(600, options.RateLimitRequestsPerMinute);
    }

    private static HttpContext Request(string path, string ip)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        return context;
    }

    [Fact]
    public void ServiceLimiter_LimitsPerClientIp()
    {
        using var limiter = SecurityLimitsExtensions.CreateServiceLimiter(3);

        for (int i = 0; i < 3; i++)
        {
            Assert.True(limiter.AttemptAcquire(Request("/geoservices/rest/services", "8.8.8.8")).IsAcquired);
        }

        Assert.False(limiter.AttemptAcquire(Request("/geoservices/rest/services", "8.8.8.8")).IsAcquired);
        Assert.True(limiter.AttemptAcquire(Request("/geoservices/rest/services", "9.9.9.9")).IsAcquired);
    }

    [Fact]
    public void ServiceLimiter_DoesNotLimitAdminPages()
    {
        using var limiter = SecurityLimitsExtensions.CreateServiceLimiter(1);

        for (int i = 0; i < 5; i++)
        {
            Assert.True(limiter.AttemptAcquire(Request("/manage", "8.8.8.8")).IsAcquired);
        }
    }
}

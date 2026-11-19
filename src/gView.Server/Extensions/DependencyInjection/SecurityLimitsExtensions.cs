using gView.Server.AppCode;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.RateLimiting;

namespace gView.Server.Extensions.DependencyInjection;

/// <summary>
/// Request body size and the optional rate limit of the services (see <see cref="SecurityOptions"/>).
/// </summary>
static internal class SecurityLimitsExtensions
{
    static public IServiceCollection AddSecurityLimits(this IServiceCollection services, SecurityOptions options)
    {
        services.Configure<KestrelServerOptions>(o => o.Limits.MaxRequestBodySize = options.MaxRequestBodyBytes);
        services.Configure<IISServerOptions>(o => o.MaxRequestBodySize = options.MaxRequestBodyBytes);
        services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = options.MaxRequestBodyBytes ?? long.MaxValue);

        if (options.RateLimitEnabled)
        {
            services.AddRateLimiter(limiter =>
            {
                limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                limiter.GlobalLimiter = CreateServiceLimiter(options.RateLimitRequestsPerMinute);
            });
        }

        return services;
    }

    static public IApplicationBuilder UseSecurityLimits(this IApplicationBuilder app, SecurityOptions options)
    {
        if (options.RateLimitEnabled)
        {
            app.UseRateLimiter();
        }

        return app;
    }

    /// <summary>
    /// A fixed window of one minute per client ip (after the forwarded headers);
    /// admin and login pages are not limited.
    /// </summary>
    static internal PartitionedRateLimiter<HttpContext> CreateServiceLimiter(int requestsPerMinute)
        => PartitionedRateLimiter.Create<HttpContext, string>(context =>
            AdminPaths.IsAdminPath(context.Request.Path)
                ? RateLimitPartition.GetNoLimiter("admin")
                : RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = requestsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
}

using gView.Server.AppCode;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.RateLimiting;

namespace gView.Server.Services.Security;

/// <summary>
/// The optional rate limit of the services (<c>security:rate-limit</c>, see <see cref="SecurityOptions"/>).
/// </summary>
static internal class ServiceRateLimiter
{
    /// <summary>
    /// A fixed window of one minute per client ip (after the forwarded headers);
    /// admin and login pages are not limited.
    /// </summary>
    static public PartitionedRateLimiter<HttpContext> Create(int requestsPerMinute)
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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace gView.Server.Extensions;

static internal class WebApplicationExtensions
{
    public static WebApplication LogStartupInformation(
            this WebApplication app,
            WebApplicationBuilder builder,
            ILogger<Startup> logger
        )
    {
        foreach (var url in builder.Configuration["urls"]?.Split(';') ?? [])
        {
            logger.LogInformation("Listen to {url}", url);
        }

        return app;
    }
}

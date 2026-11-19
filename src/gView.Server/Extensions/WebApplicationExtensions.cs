using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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

    /// <summary>
    /// Writes configuration warnings of the security section to the log.
    /// </summary>
    public static WebApplication LogSecurityWarnings(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(AppCode.AdminNetworkOnlyAttribute.LoggerCategory);

        foreach (var warning in app.Services.GetRequiredService<SecurityOptions>().ConfigurationWarnings())
        {
            logger.LogWarning("{warning}", warning);
        }

        return app;
    }

    /// <summary>
    /// Creates the first admin from the environment or prints the setup code
    /// (see <see cref="AdminSetupService"/>).
    /// </summary>
    public static WebApplication InitializeAdminSetup(this WebApplication app)
    {
        if (app.Services.GetRequiredService<MapServiceManager>().Options.IsValid)
        {
            using var scope = app.Services.CreateScope();

            app.Services.GetRequiredService<AdminSetupService>()
                .Initialize(scope.ServiceProvider.GetRequiredService<LoginManager>());
        }

        return app;
    }
}

using gView.Framework.Data.Sql;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

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
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(SecurityLog.Category);

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

    /// <summary>
    /// Functions allowed in where clauses of public requests in addition to the defaults
    /// (mapserver.json: security:sql:allowed-functions, array or comma separated list).
    /// </summary>
    public static WebApplication InitializeSqlSecurity(this WebApplication app, ILogger<Startup> logger)
    {
        var section = app.Configuration.GetSection("security:sql:allowed-functions");
        var functions = (section.Value?.Split(',') ?? section.GetChildren().Select(c => c.Value))
                            .Where(f => !String.IsNullOrWhiteSpace(f))
                            .Select(f => f!.Trim())
                            .ToArray();

        SqlWhereClauseValidator.AdditionalAllowedFunctions = functions;

        if (functions.Length > 0)
        {
            logger.LogInformation("Additional SQL functions allowed in request where clauses: {functions}", String.Join(", ", functions));
        }

        return app;
    }
}

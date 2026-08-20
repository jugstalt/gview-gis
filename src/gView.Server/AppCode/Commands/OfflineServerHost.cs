using gView.Framework.Common;
using gView.Server.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace gView.Server.AppCode.Commands;

/// <summary>
/// Shared bootstrap for all "offline" CLI commands (--publish, --remove,
/// --catalog, --get-metadata, --set-metadata): builds the exact same DI
/// container the real gView.Server web host uses (config, plugins via
/// Setup(args)/PlugInManager.Init(), MapServiceManager, AccessControlService,
/// MapServiceDeploymentManager, ...) but never calls startup.Configure(app)
/// or app.Run()/app.Start() - so Kestrel never binds a port and no
/// IHostedService is ever started. Used to run map-service admin operations
/// against a gView.Server configuration while the server itself is not
/// running (e.g. offline map preparation pipelines).
/// </summary>
static internal class OfflineServerHost
{
    async static public Task<WebApplication> BuildAsync(string[] args)
    {
        var builder = WebApplication
                            .CreateBuilder(args)
                            .Setup(args);

        builder.Logging.AddConsole();
        builder.Configuration.AddJsonFile(
                    "_config/mapserver.json",
                    optional: true,
                    reloadOnChange: false
               );

        var startup = new Startup(builder.Configuration, builder.Environment);
        startup.ConfigureServices(builder.Services);

        // Intentionally no startup.Configure(app) and no app.Run()/app.Start().
        return await Task.FromResult(builder.Build());
    }

    /// <summary>
    /// A process that can run gView.Server.exe locally already has full
    /// access to the server configuration/ServicesPath - trust it like an
    /// administrator, no credential prompt needed.
    /// </summary>
    static public Identity TrustedIdentity(string userName = "gview-server-cli")
        => new Identity(userName, isAdministrator: true);
}

using gView.Framework.Core.Exceptions;
using gView.Server.Services.MapServer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace gView.Server.AppCode.Commands;

/// <summary>
/// CLI remove mode for gView.Server.exe: removes a map service (.mxl/.svc/.meta
/// under ServicesPath) via the same DI-hosted <see cref="MapServiceDeploymentManager"/>
/// used by the HTTP RemoveMap endpoint - without starting Kestrel. See
/// <see cref="OfflineServerHost"/>.
///
/// Usage:
///   gView.Server.exe --remove --service &lt;folder/servicename&gt;
///
/// Exit code 0 = removed successfully, non-zero = error (message on stderr).
/// </summary>
static public class RemoveCommand
{
    async static public Task<int> RunAsync(string[] args)
    {
        var service = CliArgs.GetValue(args, "--service");

        if (String.IsNullOrWhiteSpace(service))
        {
            Console.Error.WriteLine("Usage: gView.Server.exe --remove --service <folder/servicename>");
            return 1;
        }

        try
        {
            await using var app = await OfflineServerHost.BuildAsync(args);

            var deploymentManager = app.Services.GetRequiredService<MapServiceDeploymentManager>();
            var identity = OfflineServerHost.TrustedIdentity();

            var success = await deploymentManager.RemoveMap(service, identity);

            if (!success)
            {
                Console.Error.WriteLine($"Removing service '{service}' failed.");
                return 1;
            }

            Console.WriteLine($"Service '{service}' successfully removed.");
            return 0;
        }
        catch (MapServerException ex)
        {
            Console.Error.WriteLine("Error removing service:");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Unexpected error removing service:");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}

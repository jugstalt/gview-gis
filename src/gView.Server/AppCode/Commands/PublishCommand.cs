using gView.Framework.Core.Exceptions;
using gView.Server.Services.MapServer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.Server.AppCode.Commands;

/// <summary>
/// CLI publish mode for gView.Server.exe: registers a map service from an
/// .mxl file directly via the same DI-hosted <see cref="MapServiceDeploymentManager"/>
/// used by the HTTP publish endpoints (validation, renaming the first map to
/// "{folder}/{servicename}", writing ServicesPath/.meta) - but without ever
/// starting the Kestrel/HTTP listener or any hosted background service.
/// See <see cref="OfflineServerHost"/>.
///
/// Intended for offline map preparation workflows where gView.Server is not
/// running as a service (e.g. gView.Cmd.exe based APRX -> MXL -> offline FDB
/// pipelines).
///
/// Usage:
///   gView.Server.exe --publish --mxl &lt;path-to-mxl&gt; --service &lt;folder/servicename&gt;
///
/// Exit code 0 = published successfully, non-zero = error (message on stderr).
/// </summary>
static public class PublishCommand
{
    async static public Task<int> RunAsync(string[] args)
    {
        var mxlPath = CliArgs.GetValue(args, "--mxl");
        var service = CliArgs.GetValue(args, "--service");

        if (String.IsNullOrWhiteSpace(mxlPath) || String.IsNullOrWhiteSpace(service))
        {
            Console.Error.WriteLine("Usage: gView.Server.exe --publish --mxl <path-to-mxl> --service <folder/servicename>");
            return 1;
        }

        if (!File.Exists(mxlPath))
        {
            Console.Error.WriteLine($"Mxl file not found: {mxlPath}");
            return 1;
        }

        try
        {
            await using var app = await OfflineServerHost.BuildAsync(args);

            var deploymentManager = app.Services.GetRequiredService<MapServiceDeploymentManager>();
            var identity = OfflineServerHost.TrustedIdentity();

            var mapXml = await File.ReadAllTextAsync(mxlPath);

            var success = await deploymentManager.AddMap(service, mapXml, identity);

            if (!success)
            {
                Console.Error.WriteLine($"Publishing service '{service}' failed.");
                return 1;
            }

            Console.WriteLine($"Service '{service}' successfully published from '{mxlPath}'.");
            return 0;
        }
        catch (MapServerException ex)
        {
            Console.Error.WriteLine("Error publishing service:");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Unexpected error publishing service:");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}

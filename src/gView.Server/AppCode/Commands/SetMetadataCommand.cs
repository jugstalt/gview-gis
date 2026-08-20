using gView.Framework.Core.Exceptions;
using gView.Server.Services.MapServer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.Server.AppCode.Commands;

/// <summary>
/// CLI set-metadata mode for gView.Server.exe: writes ServicesPath/*.meta XML
/// for a map service via the same DI-hosted <see cref="MapServiceDeploymentManager"/>
/// used by the HTTP SetMetadata endpoint - without starting Kestrel. See
/// <see cref="OfflineServerHost"/>.
///
/// Usage:
///   gView.Server.exe --set-metadata --service &lt;folder/servicename&gt; --metadata &lt;path-to-xml&gt;
///
/// Exit code 0 = metadata written successfully, non-zero = error (message on
/// stderr).
/// </summary>
static public class SetMetadataCommand
{
    async static public Task<int> RunAsync(string[] args)
    {
        var service = CliArgs.GetValue(args, "--service");
        var metadataPath = CliArgs.GetValue(args, "--metadata");

        if (String.IsNullOrWhiteSpace(service) || String.IsNullOrWhiteSpace(metadataPath))
        {
            Console.Error.WriteLine("Usage: gView.Server.exe --set-metadata --service <folder/servicename> --metadata <path-to-xml>");
            return 1;
        }

        if (!File.Exists(metadataPath))
        {
            Console.Error.WriteLine($"Metadata file not found: {metadataPath}");
            return 1;
        }

        try
        {
            await using var app = await OfflineServerHost.BuildAsync(args);

            var deploymentManager = app.Services.GetRequiredService<MapServiceDeploymentManager>();
            var identity = OfflineServerHost.TrustedIdentity();

            var metadata = await File.ReadAllTextAsync(metadataPath);

            var success = await deploymentManager.SetMetadata(service, metadata, identity);

            if (!success)
            {
                Console.Error.WriteLine($"Setting metadata for service '{service}' failed.");
                return 1;
            }

            Console.WriteLine($"Metadata for service '{service}' successfully set from '{metadataPath}'.");
            return 0;
        }
        catch (MapServerException ex)
        {
            Console.Error.WriteLine("Error setting metadata:");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Unexpected error setting metadata:");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}

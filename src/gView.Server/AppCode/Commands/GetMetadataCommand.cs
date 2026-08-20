using gView.Framework.Core.Exceptions;
using gView.Server.Services.MapServer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.Server.AppCode.Commands;

/// <summary>
/// CLI get-metadata mode for gView.Server.exe: reads the ServicesPath/*.meta
/// XML of a map service via the same DI-hosted <see cref="MapServiceDeploymentManager"/>
/// used by the HTTP GetMetadata endpoint - without starting Kestrel. See
/// <see cref="OfflineServerHost"/>.
///
/// Usage:
///   gView.Server.exe --get-metadata --service &lt;folder/servicename&gt; [--out &lt;path&gt;]
///
/// Without --out, the metadata XML is printed to stdout. Exit code 0 =
/// completed (an empty result just means no metadata / no such service),
/// non-zero = error (message on stderr).
/// </summary>
static public class GetMetadataCommand
{
    async static public Task<int> RunAsync(string[] args)
    {
        var service = CliArgs.GetValue(args, "--service");
        var outPath = CliArgs.GetValue(args, "--out");

        if (String.IsNullOrWhiteSpace(service))
        {
            Console.Error.WriteLine("Usage: gView.Server.exe --get-metadata --service <folder/servicename> [--out <path>]");
            return 1;
        }

        try
        {
            await using var app = await OfflineServerHost.BuildAsync(args);

            var deploymentManager = app.Services.GetRequiredService<MapServiceDeploymentManager>();
            var identity = OfflineServerHost.TrustedIdentity();

            var metadata = await deploymentManager.GetMetadata(service, identity);

            if (String.IsNullOrEmpty(metadata))
            {
                Console.Error.WriteLine($"No metadata found for service '{service}' (service missing or has no .meta file).");
                return 0;
            }

            if (!String.IsNullOrWhiteSpace(outPath))
            {
                await File.WriteAllTextAsync(outPath, metadata);
                Console.WriteLine($"Metadata for '{service}' written to '{outPath}'.");
            }
            else
            {
                Console.WriteLine(metadata);
            }

            return 0;
        }
        catch (MapServerException ex)
        {
            Console.Error.WriteLine("Error reading metadata:");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Unexpected error reading metadata:");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}

using gView.Framework.Common.Extensions;
using gView.Framework.Core.MapServer;
using gView.Server.Models;
using gView.Server.Services.MapServer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace gView.Server.AppCode.Commands;

/// <summary>
/// CLI catalog mode for gView.Server.exe: lists the map services currently
/// registered under ServicesPath (root level + one folder level, same
/// discovery logic MapServiceManager uses) without starting Kestrel and
/// without the HTTP catalog's per-visitor access filtering - this is a local
/// admin tool, it lists everything. See <see cref="OfflineServerHost"/>.
///
/// Usage:
///   gView.Server.exe --catalog [--format text|xml|json]   (default: text)
///
/// Exit code 0 = catalog printed successfully, non-zero = error.
/// </summary>
static public class CatalogCommand
{
    async static public Task<int> RunAsync(string[] args)
    {
        var format = CliArgs.GetValue(args, "--format");
        if (String.IsNullOrWhiteSpace(format))
        {
            format = "text";
        }

        if (!"text".Equals(format, StringComparison.OrdinalIgnoreCase) &&
            !"xml".Equals(format, StringComparison.OrdinalIgnoreCase) &&
            !"json".Equals(format, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Usage: gView.Server.exe --catalog [--format text|xml|json]");
            return 1;
        }

        try
        {
            await using var app = await OfflineServerHost.BuildAsync(args);

            var mapServiceManager = app.Services.GetRequiredService<MapServiceManager>();

            // one folder level deep, matching MapServiceManager's "folder/name" convention
            foreach (var folder in mapServiceManager.MapServices.Where(s => s.Type == MapServiceType.Folder).ToList())
            {
                mapServiceManager.ReloadServices(folder.Name, true);
            }

            var services = mapServiceManager.MapServices
                .Where(s => s.Type != MapServiceType.Folder)
                .OrderBy(s => s.Fullname)
                .Select(s => new ServiceModel { Name = s.Fullname, Type = s.Type.ToInvariantString() })
                .ToList();

            if ("json".Equals(format, StringComparison.OrdinalIgnoreCase))
            {
                var model = new ServicesModel { Services = services };
                Console.WriteLine(JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true }));
            }
            else if ("xml".Equals(format, StringComparison.OrdinalIgnoreCase))
            {
                var sb = new StringBuilder();
                sb.Append("<RESPONSE><SERVICES>");
                foreach (var service in services)
                {
                    sb.Append("<SERVICE ");
                    sb.Append($"NAME='{service.Name}' ");
                    sb.Append($"name='{service.Name}' ");
                    sb.Append($"type='{service.Type}' ");
                    sb.Append("/>");
                }
                sb.Append("</SERVICES></RESPONSE>");
                Console.WriteLine(sb.ToString());
            }
            else
            {
                foreach (var service in services)
                {
                    Console.WriteLine($"{service.Name} ({service.Type})");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Unexpected error listing catalog:");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}

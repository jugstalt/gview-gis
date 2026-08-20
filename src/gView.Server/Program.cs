using gView.Server;
using gView.Server.AppCode.Commands;
using gView.Server.Extensions;
using gView.Server.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;

// Offline/CLI admin modes: read the full server configuration (DI, plugins,
// ServicesPath, ...) but never bind Kestrel to a port - see OfflineServerHost.
if (args.Contains("--publish"))
{
    return await PublishCommand.RunAsync(args);
}
if (args.Contains("--remove"))
{
    return await RemoveCommand.RunAsync(args);
}
if (args.Contains("--catalog"))
{
    return await CatalogCommand.RunAsync(args);
}
if (args.Contains("--get-metadata"))
{
    return await GetMetadataCommand.RunAsync(args);
}
if (args.Contains("--set-metadata"))
{
    return await SetMetadataCommand.RunAsync(args);
}

var builder = WebApplication
                    .CreateBuilder(args)
                    .Setup(args);

#if DEBUG
// Aspire
builder.AddServiceDefaults();
#endif

builder.Logging.AddConsole();
builder.Configuration.AddJsonFile(
            "_config/mapserver.json",
            optional: true,
            reloadOnChange: false
       );

var startup = new Startup(builder.Configuration, builder.Environment);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

#if DEBUG
// Aspire
app.MapDefaultEndpoints();
#endif

startup.Configure(app);

app.LogStartupInformation(
        builder,
        app.Services.GetRequiredService<ILogger<Startup>>()
   )
   .Run();

return 0;

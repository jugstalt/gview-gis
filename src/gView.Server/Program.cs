using gView.Server;
using gView.Server.Extensions;
using gView.Server.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

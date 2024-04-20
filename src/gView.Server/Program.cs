using gView.Framework.Common;
using gView.Server;
using gView.Server.AppCode;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

#region Init the global PluginManager

PlugInManager.Init();

#endregion

#region First Start => init configuration

new Setup().TrySetup(args);

#endregion

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Configuration.AddJsonFile(
            "_config/mapserver.json",
            optional: true,
            reloadOnChange: false
       );

var startup = new Startup(builder.Configuration, builder.Environment);

startup.ConfigureServices(builder.Services);
var app = builder.Build();
startup.Configure(app);

foreach (var url in builder.Configuration["urls"]?.Split(';') ?? [])
{
    Console.WriteLine($"Listen to {url}");
}

app.Run();

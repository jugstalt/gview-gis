using gView.Blazor.Core.Extensions.DependencyInjection;
using gView.Carto.Plugins.Extensions.DependencyInjection;
using gView.Carto.Razor.Extensions.DependencyInjection;
using gView.DataExplorer.Plugins.Extensions.DependencyInjection;
using gView.Framework.Common;
using gView.Framework.Db.Extensions;
using gView.GraphicsEngine.Abstraction;
using gView.Razor.Extensions.DependencyInjection;
using gView.Razor.Leaflet.Extensions.DependencyInjection;
using gView.WebApps.Components;
using gView.WebApps.Extensions;
using gView.WebApps.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using System.Configuration;

gView.Framework.Common.SystemInfo.RegisterProj4Lib(gView.Framework.Geometry.GeometricTransformerFactory.PROJ_LIB);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("_config/gview-webapps.config", true);

#region SqlServer

builder.Configuration
    .GetSection("SqlServer:AppendParameters")
    .Get<string[]>()
    .SetSqlServerParametersToAppend();


#endregion

// Aspire
builder.AddServiceDefaults();

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddEnvironmentService(config =>
    {
        config.RepositoryPath = builder.Configuration.RepositoryPath();
    })
    .AddWebScopeContextService()
    .AddAuth(builder.Configuration)
    .AddDrives(builder.Configuration)
    .AddPublishServers(builder.Configuration)
    .AddCustomTiles(builder.Configuration.Bind);

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;

    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddApplicationScopeFactory();

builder.Services.AddCartoDesktopApplicationService();
builder.Services.AddCartoInteropServices();
builder.Services.AddKnownCartoDialogsServices();
builder.Services.AddCartoApplicationScopeService(
    config =>
    {
        config.ConfigRootPath = Path.Combine(builder.Configuration.RepositoryPath(), "gview-carto");
    },
    restoreConfig =>
    {
        restoreConfig.RestoreRootPath = Path.Combine(builder.Configuration.RepositoryPath(), "gview-carto", "_restore");
    });

builder.Services.AddExplorerDesktopApplicationService();
builder.Services.AddExplorerApplicationScopeService(config =>
{
    config.ConfigRootPath = Path.Combine(builder.Configuration.RepositoryPath(), "gview-explorer");
});

builder.Services.AddKnownExplorerDialogsServices();
builder.Services.AddFrameworkServices();
builder.Services.AddIconService();
builder.Services.AddSettingsService(config =>
{
    config.Path = Path.Combine(builder.Configuration.RepositoryPath(), "gview-web-settings.db");
});
builder.Services.AddMapControlBackgroundTilesService(config =>
{
    config.Default = builder.Configuration["MapControl:Tiles"] ?? "osm"; // "basemap_at";
});
builder.Services.AddMapControlCrsService(config =>
{
    config.Default = builder.Configuration["MapControl:Crs"] ?? "webmercator"; // "webmercator_at";
});
builder.Services.AddLeafletService();

var app = builder.Build();

// Aspire
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseGViewWebBasePath();
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.AddAuth(builder.Configuration);

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

//gView.GraphicsEngine.Current.UseSecureDisposingOnUserInteractiveUIs = true;
//SystemInfo.RegisterDefaultGraphicEngines();

// Gdi+
//gView.GraphicsEngine.Current.Engine = new gView.GraphicsEngine.GdiPlus.GdiGraphicsEngine(96.0f);
//gView.GraphicsEngine.Current.Encoder = new gView.GraphicsEngine.GdiPlus.GdiBitmapEncoding();

// Skia
//gView.GraphicsEngine.Current.Engine = new gView.GraphicsEngine.Skia.SkiaGraphicsEngine(96.0f);
//gView.GraphicsEngine.Current.Encoder = new gView.GraphicsEngine.Skia.SkiaBitmapEncoding();

#region Graphics Engine

gView.GraphicsEngine.Current.UseSecureDisposingOnUserInteractiveUIs = true;
SystemInfo.RegisterDefaultGraphicEngines(96.0f);

switch (builder.Configuration["graphics:rendering"]?.ToString())
{
    case "gdi":
    case "gdiplus":
        gView.GraphicsEngine.Current.Engine = new gView.GraphicsEngine.GdiPlus.GdiGraphicsEngine(96.0f);
        break;
    default:
        gView.GraphicsEngine.Current.Engine = new gView.GraphicsEngine.Skia.SkiaGraphicsEngine(96.0f);
        break;
}

switch (builder.Configuration["graphics:encoding"]?.ToString())
{
    case "skia":
    case "skiasharp":
        gView.GraphicsEngine.Current.Encoder = new gView.GraphicsEngine.Skia.SkiaBitmapEncoding();
        break;
    default:
        gView.GraphicsEngine.Current.Encoder = new gView.GraphicsEngine.Default.BitmapEncoding();
        break;
}

#endregion

app.Run();

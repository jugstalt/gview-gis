using gView.Blazor.Core.Extensions.DependencyInjection;
using gView.Carto.Plugins.Extensions.DependencyInjection;
using gView.Carto.Razor.Extensions.DependencyInjection;
using gView.DataExplorer.Plugins.Extensions.DependencyInjection;
using gView.Razor.Extensions.DependencyInjection;
using gView.Razor.Leaflet.Extensions.DependencyInjection;
using gView.Web.Components;
using gView.Web.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using MudBlazor.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddDefaultUserIdentifyService()
    .AddWebScopeContextService();

builder.Services
    .AddAuthentication()
    /*.AddBearerToken(IdentityConstants.BearerScheme)*/;

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
builder.Services.AddCartoApplicationScopeService(config =>
{
    config.ConfigRootPath = Path.Combine("C:\\temp", "gview-explorer");
});

builder.Services.AddExplorerDesktopApplicationService();
builder.Services.AddExplorerApplicationScopeService(config =>
{
    config.ConfigRootPath = Path.Combine("C:\\temp", "gview-explorer");
});

builder.Services.AddKnownExplorerDialogsServices();
builder.Services.AddFrameworkServices();
builder.Services.AddIconService();
builder.Services.AddSettingsService(config =>
{
    config.Path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "gview-web-settings.db");
});
builder.Services.AddMapControlBackgroundTilesService(config =>
{
    config.Default = "basemap_at";
});
builder.Services.AddMapControlCrsService(config =>
{
    config.Default = "webmercator_at";
});
builder.Services.AddLeafletService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

gView.GraphicsEngine.Current.UseSecureDisposingOnUserInteractiveUIs = true;

// Gdi+
//gView.GraphicsEngine.Current.Engine = new gView.GraphicsEngine.GdiPlus.GdiGraphicsEngine(96.0f);
gView.GraphicsEngine.Current.Encoder = new gView.GraphicsEngine.GdiPlus.GdiBitmapEncoding();

// Skia
gView.GraphicsEngine.Current.Engine = new gView.GraphicsEngine.Skia.SkiaGraphicsEngine(96.0f);
//gView.GraphicsEngine.Current.Encoder = new gView.GraphicsEngine.Skia.SkiaBitmapEncoding();

app.Run();

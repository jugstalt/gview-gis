using gView.Blazor.Core.Services;
using gView.Framework.Core.Data;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace gView.Blazor.Core.Extensions.DependencyInjection;

public static class ServicesExtensions
{
    static public IServiceCollection AddFrameworkServices(this IServiceCollection services)
        => services
            .AddTransient<SpatialReferenceService>()
            .AddTransient<GeoTransformerService>()
            .AddTransient<PluginManagerService>()
            .AddScoped<MapRenderService>();
    

    static public IServiceCollection AddIconService(this IServiceCollection services)
        => services.AddTransient<IconService>();   
    

    static public IServiceCollection AddPersistentSettingsService(
        this IServiceCollection services,
        Action<PersistentSettingsServiceOptions> setupAction)
        => services.Configure(setupAction)
                   .AddTransient<PersistentSettingsService>();

    static public IServiceCollection AddMapControlCrsService(
        this IServiceCollection services,
        Action<MapControlCrsServiceOptions> setupAction)
        => services
                .Configure(setupAction)
                .AddSingleton<MapControlCrsService>();

    static public IServiceCollection AddMapControlBackgroundTilesService(
        this IServiceCollection services,
        Action<MapControlBackgroundTilesServiceOptions> setupAction)
        => services
                .Configure(setupAction)
                .AddSingleton<MapControlBackgroundTilesService>();
}

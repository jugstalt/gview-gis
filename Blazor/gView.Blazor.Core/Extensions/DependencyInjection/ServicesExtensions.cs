using gView.Blazor.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace gView.Blazor.Core.Extensions.DependencyInjection;

public static class ServicesExtensions
{
    static public IServiceCollection AddFrameworkServices(this IServiceCollection services)
    {
        return services
            .AddTransient<SpatialReferenceService>()
            .AddTransient<GeoTransformerService>()
            .AddTransient<PluginManagerService>();
    }

    static public IServiceCollection AddIconService(this IServiceCollection services)
    {
        return services.AddTransient<IconService>();   
    }
}

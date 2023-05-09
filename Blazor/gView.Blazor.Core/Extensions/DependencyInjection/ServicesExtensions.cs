using gView.Blazor.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace gView.Blazor.Core.Extensions.DependencyInjection;

public static class ServicesExtensions
{
    static public IServiceCollection AddPluginMangerService(this IServiceCollection services)
    {
        return services.AddTransient<PluginManagerService>();
    }
}

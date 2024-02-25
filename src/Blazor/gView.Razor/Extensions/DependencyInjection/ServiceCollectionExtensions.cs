using gView.Framework.Blazor.Services.Abstraction;
using gView.Razor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace gView.Razor.Extensions.DependencyInjection;

static public class ServiceCollectionExtensions
{
    static public IServiceCollection AddApplicationScopeFactory(this IServiceCollection services)
        => services.AddScoped<IApplicationScopeFactory, ApplicationScopeFactory>();

    static public IServiceCollection AddCustomTiles(
            this IServiceCollection services,
            Action<CustomTilesServiceOptions> setupAction
        )
        => services
                .Configure(setupAction)
                .AddSingleton<CustomTilesService>();
                
}

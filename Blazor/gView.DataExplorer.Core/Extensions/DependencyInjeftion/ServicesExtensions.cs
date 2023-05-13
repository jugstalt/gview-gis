using gView.DataExplorer.Core.Services;
using gView.Framework.DataExplorer.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace gView.DataExplorer.Core.Extensions.DependencyInjeftion;

static public class ServicesExtensions
{
    static public IServiceCollection AddApplicationScopeService(this IServiceCollection services)
    {
        return services
            .AddScoped<IExplorerApplicationScope, ExplorerApplicationScopeService>()
            .AddScoped<EventBusService>();
    }
}

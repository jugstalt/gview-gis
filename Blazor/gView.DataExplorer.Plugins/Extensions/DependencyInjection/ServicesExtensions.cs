using gView.DataExplorer.Core.Services.Abstraction;
using gView.DataExplorer.Plugins.Services;
using Microsoft.Extensions.DependencyInjection;

namespace gView.DataExplorer.Plugins.Extensions.DependencyInjection;
static public class ServicesExtensions
{
    static public IServiceCollection AddExplorerDesktopApplicationService(this IServiceCollection services)
    {
        return services.AddSingleton<IExplorerApplicationService, ExplorerDesktopApplicationService>();
    }
}

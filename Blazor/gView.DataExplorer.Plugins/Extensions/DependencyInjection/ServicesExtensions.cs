using gView.Blazor.Core.Services.Abstraction;
using gView.DataExplorer.Core.Extensions.DependencyInjeftion;
using gView.DataExplorer.Core.Services.Abstraction;
using gView.DataExplorer.Plugins.Services;
using gView.DataExplorer.Plugins.Services.Dialogs;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace gView.DataExplorer.Plugins.Extensions.DependencyInjection;
static public class ServicesExtensions
{
    static public IServiceCollection AddExplorerDesktopApplicationService(this IServiceCollection services)
    {
        return services.AddSingleton<IExplorerApplicationService, ExplorerDesktopApplicationService>();
    }

    static public IServiceCollection AddExplorerApplicationScopeService(this IServiceCollection services) 
    {
        return services
            .AddEventBus()
            .AddScoped<IApplicationScope, ExplorerApplicationScopeService>();
    }

    static public IServiceCollection AddKnownExplorerDialogsServices(this IServiceCollection services)
    {
        return services
            .AddTransient<IKnownDialogService, ExplorerDialogService>()
            .AddTransient<IKnownDialogService, SpatialReferenceDialogService>()
            .AddTransient<IKnownDialogService, GeographicsProjectionSelectorDialogService>()
            .AddTransient<IKnownDialogService, GeographicsDatumSelectorDialogService>();
    }
}

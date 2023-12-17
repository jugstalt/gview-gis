using gView.Carto.Core.Extensions.DependencyInjection;
using gView.Carto.Core.Services;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.PropertyGridEditors;
using gView.Carto.Plugins.Services;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Razor.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace gView.Carto.Plugins.Extensions.DependencyInjection;
static public class ServicesExtensions
{
    static public IServiceCollection AddCartoDesktopApplicationService(this IServiceCollection services)
        => services
                .AddSingleton<ICartoApplicationService, CartoDesktopApplicationService>();

    static public IServiceCollection AddCartoApplicationScopeService(this IServiceCollection services,
                                                                     Action<CartoApplicationScopeServiceOptions> configureOptions)
    {
        return services
            .AddTransient<IPropertyGridEditor, SymbolPropertyEditor>()
            .AddTransient<IPropertyGridEditor, FontPropertyEditor>()
            .AddTransient<IPropertyGridEditor, CharakterSelectorEditor>()
            .AddTransient<IPropertyGridEditor, FontNameSelectorEditor>()
            .AddTransient<IPropertyGridEditor, ColorGradientPropertyEditor>()
            .AddTransient<IPropertyGridEditor, ResourcesPickerPropertyEditor>()
            .AddTransient<ICartoDocumentService, CartoDocumentService>()
            .Configure(configureOptions)
            .AddEventBus()
            .AddScoped<ICartoApplicationScopeService, CartoApplicationScopeService>();
    }
}

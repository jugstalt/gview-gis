using gView.Framework.Blazor.Services.Abstraction;
using gView.Rator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace gView.Razor.Extensions.DependencyInjection;

static public class ServiceCollectionExtensions
{
    static public IServiceCollection AddApplicationScopeFactory(this IServiceCollection services)
        => services.AddScoped<IApplicationScopeFactory, ApplicationScopeFactory>();
}

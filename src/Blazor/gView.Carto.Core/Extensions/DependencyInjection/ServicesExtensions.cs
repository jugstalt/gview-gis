using gView.Carto.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace gView.Carto.Core.Extensions.DependencyInjection;
static public class ServicesExtensions
{
    static public IServiceCollection AddEventBus(this IServiceCollection services)
        => services.AddScoped<CartoEventBusService>();

    static public IServiceCollection AddDataTables(this IServiceCollection services)
        => services.AddScoped<CartoDataTableService>();

    static public IServiceCollection AddDisplayService(this IServiceCollection services)
        => services.AddScoped<CartoDisplayService>();
}

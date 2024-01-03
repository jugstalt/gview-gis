using gView.Carto.Razor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace gView.Carto.Razor.Extensions.DependencyInjection;
public static class ServicesExtensions
{
    public static IServiceCollection AddCartoInteropServices(this IServiceCollection services)
        => services.AddScoped<CartoInteropService>();
}

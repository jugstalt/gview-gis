using gView.Blazor.Core.Services.Abstraction;
using gView.Web.Services;

namespace gView.Web.Extensions.DependencyInjection;

static public class ServicesExtensions
{
    static public IServiceCollection AddDefaultUserIdentifyService(this IServiceCollection services)
        => services
            .AddHttpContextAccessor()
            .AddScoped<IUserIdentityService, UserIdentityService>();
}

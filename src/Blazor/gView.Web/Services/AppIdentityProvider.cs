using gView.Blazor.Core.Models;
using gView.Blazor.Core.Services.Abstraction;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

namespace gView.Web.Services;

public class AppIdentityProvider : IAppIdentityProvider
{
    private readonly AppIdentityProviderOptions _options;
    private readonly HttpContext? _httpContext;
    private readonly AppIdentity _identity;

    public AppIdentityProvider(
            AuthenticationStateProvider authProvider,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AppIdentityProviderOptions> options
        )
    {
        _httpContext = httpContextAccessor.HttpContext;
        _options = options.Value;

        _identity = _httpContext is null
            ? new AppIdentity("", false, false)
            : new AppIdentity(
                _httpContext.User.Identity?.Name ?? "",
                _httpContext.User.IsInRole(_options.AdminRoleName),
                _httpContext.User.IsInRole(_options.AdminRoleName) || _httpContext.User.IsInRole(_options.UserRoleName)
            );
    }

    public AppIdentity Identity => _identity;

    public async Task Logout()
    {
        if(_httpContext is not null)
        {
            await _httpContext.SignOutAsync();
        }
    }
}

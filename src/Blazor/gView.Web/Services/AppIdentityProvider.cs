using gView.Blazor.Core.Models;
using gView.Blazor.Core.Services.Abstraction;
using gView.Web.Extensions;
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

        AdministratorsRoleName = options.Value.AdminRoleName;
        UsersRoleName = options.Value.UserRoleName;

        _identity = _httpContext is null
            ? new AppIdentity("", false, false)
            : new AppIdentity(
                _httpContext.User.Identity?.Name ?? "",
                _httpContext.User.IsInRoleOrHasRoleClaim(_options.AdminRoleName),
                _httpContext.User.IsInRoleOrHasRoleClaim(_options.AdminRoleName) || _httpContext.User.IsInRoleOrHasRoleClaim(_options.UserRoleName)
            );
    }

    public AppIdentity Identity => _identity;

    public string AdministratorsRoleName { get; }

    public string UsersRoleName { get; }

    public async Task Logout()
    {
        if(_httpContext is not null)
        {
            await _httpContext.SignOutAsync();
        }
    }
}

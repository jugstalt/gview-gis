using gView.Blazor.Core.Models;
using gView.Blazor.Core.Services.Abstraction;
using Microsoft.Extensions.Options;

namespace gView.Web.Services;

public class AppLocalIdentityProvider : IAppIdentityProvider
{
    private readonly AppIdentity _identity;

    public AppLocalIdentityProvider(IOptions<AppIdentityProviderOptions> options)
    {
        _identity=new AppIdentity(Environment.UserName, true, true, false);

        AdministratorsRoleName = options.Value.AdminRoleName;
        UsersRoleName = options.Value.UserRoleName;
    }

    public AppIdentity Identity => _identity;

    public string AdministratorsRoleName { get; }

    public string UsersRoleName { get; }

    public Task Logout() => Task.CompletedTask;
}

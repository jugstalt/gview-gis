using gView.Blazor.Core.Models;
using gView.Blazor.Core.Services.Abstraction;

namespace gView.Web.Services;

public class AppLocalIdentityProvider : IAppIdentityProvider
{
    private readonly AppIdentity _identity;

    public AppLocalIdentityProvider()
    {
        _identity=new AppIdentity(Environment.UserName, true, true);
    }

    public AppIdentity Identity => _identity;

    public Task Logout() => Task.CompletedTask;
}

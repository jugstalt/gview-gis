using gView.Blazor.Core.Models;
using System.Threading.Tasks;

namespace gView.Blazor.Core.Services.Abstraction;

public interface IAppIdentityProvider
{
    AppIdentity Identity { get; }
    Task Logout();
}

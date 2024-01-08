using System.Collections.Generic;

namespace gView.Blazor.Core.Services.Abstraction;
public interface IUserIdentityService
{
    string UserName { get; }
    IEnumerable<string> UserRoles { get; }
}

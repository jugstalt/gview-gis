using gView.Blazor.Core.Services.Abstraction;

namespace gView.Web.Services;

public class UserIdentityService : IUserIdentityService
{
    public UserIdentityService(IHttpContextAccessor httpContextAccessor)
    {
        this.UserName = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "";
        this.UserRoles = [];
    }

    public string UserName { get; }

    public IEnumerable<string> UserRoles { get; }   
}

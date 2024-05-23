using System.Security.Claims;

namespace gView.WebApps.Extensions;

static internal class ClaimsPrincipalExtensions
{
    static public bool IsInRoleOrHasRoleClaim(this ClaimsPrincipal claimsPrincipal, string role)
        => claimsPrincipal.IsInRole(role)
        || claimsPrincipal.GetRoles().Contains(role);
    

    static private IEnumerable<string> GetRoles(this ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal?.Claims == null)
        {
            return new string[0];
        }

        var roles = claimsPrincipal
                .Claims
                .Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                .Select(c => c.Value)
                .ToArray();

        return roles;
    }
}

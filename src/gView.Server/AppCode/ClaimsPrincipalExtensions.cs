using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace gView.Server.AppCode;

public static class ClaimsPrincipalExtensions
{
    static public IEnumerable<string> GetRoles(this ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal?.Claims is null)
        {
            return new string[0];
        }

        var roles = claimsPrincipal
                .Claims
                .Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                .Select(c => c.Value)
                .ToArray();

        if (roles == null || roles.Length == 0)
        {
            var roleClaim = claimsPrincipal
                  .Claims
                  .Where(c => c.Type == "role")
                  .FirstOrDefault();

            if (roleClaim != null && roleClaim.Value != null && roleClaim.Value.StartsWith("["))
            {
                try
                {
                    return JsonSerializer.Deserialize<string[]>(roleClaim.Value);
                }
                catch { }
            }
        }

        return roles;
    }
}

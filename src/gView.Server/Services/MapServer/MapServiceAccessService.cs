using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using System;
using System.Linq;

namespace gView.Server.Services.MapServer;

public class MapServiceAccessService
{
    private readonly MapServerConfiguration _mapServerConfiguration;

    public MapServiceAccessService(MapServerConfiguration mapServerConfiguration)
    {
        _mapServerConfiguration = mapServerConfiguration;
    }

    public string AdminAlias =>
     _mapServerConfiguration.HasExternalLoginAuthority
     ? "_manager"
     : "";

    public void CheckAccess(
            IServiceRequestContext context, 
            IMapServiceSettings settings
        )
    {
        if (settings?.AccessRules == null || settings.AccessRules.Length == 0)  // No Settings -> free service
        {
            return;
        }

        string userName = Username(context.ServiceRequest?.Identity);

        var accessRule = settings
            .AccessRules
            .Where(r => r.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
            .FirstOrDefault();

        // if user not found, use rules for anonymous
        if (accessRule == null)
        {
            accessRule = settings
                .AccessRules
                .Where(r => r.Username.Equals(Identity.AnonyomousUsername, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();
        }

        if (accessRule == null || accessRule.ServiceTypes == null)
        {
            throw new TokenRequiredException("forbidden (user:" + userName + ")");
        }

        if (!accessRule.ServiceTypes.Contains("_all") && !accessRule.ServiceTypes.Contains("_" + context.ServiceRequestInterpreter.IdentityName.ToLower()))
        {
            throw new NotAuthorizedException(context.ServiceRequestInterpreter.IdentityName + " interface forbidden (user: " + userName + ")");
        }

        var accessTypes = context.ServiceRequestInterpreter.RequiredAccessTypes(context);
        foreach (AccessTypes accessType in Enum.GetValues(typeof(AccessTypes)))
        {
            if (accessType != AccessTypes.None && accessTypes.HasFlag(accessType))
            {
                if (!accessRule.ServiceTypes.Contains(accessType.ToString().ToLower()))
                {
                    throw new NotAuthorizedException("Forbidden: " + accessType.ToString() + " access required (user: " + userName + ")");
                }
            }
        }
    }

    public void CheckAccess(
                IIdentity identity, 
                IServiceRequestInterpreter interpreter, 
                IMapServiceSettings settings
        )
    {
        if (settings?.AccessRules == null || settings.AccessRules.Length == 0)  // No Settings -> free service
        {
            return;
        }

        string userName = Username(identity);

        var accessRule = settings
            .AccessRules
            .Where(r => r.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
            .FirstOrDefault();

        // if user not found, use rules for anonymous
        if (accessRule == null)
        {
            accessRule = settings
                .AccessRules
                .Where(r => r.Username.Equals(Identity.AnonyomousUsername, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();
        }

        if (accessRule == null || accessRule.ServiceTypes == null)
        {
            throw new TokenRequiredException("forbidden (user:" + userName + ")");
        }

        if (!accessRule.ServiceTypes.Contains("_all") && !accessRule.ServiceTypes.Contains("_" + interpreter.IdentityName.ToLower()))
        {
            throw new NotAuthorizedException(interpreter.IdentityName + " interface forbidden (user: " + userName + ")");
        }
    }

    public string Username(IIdentity identity)
    {
        string userName = identity?.UserName ?? Identity.AnonyomousUsername;

        if (identity.IsAdministrator == true
           && !String.IsNullOrEmpty(this.AdminAlias))
        {
            userName = this.AdminAlias;
        }

        return userName;    
    }
}

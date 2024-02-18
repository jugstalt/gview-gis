using gView.Blazor.Core.Models;
using gView.Framework.Common.Reflection;
using gView.Framework.Core.Common;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace gView.Blazor.Core.Extensions;

static public class AppIdentityExtensions
{
    static public string UserNameAndRole(this AppIdentity? appIdentity)
        => (appIdentity?.Username, appIdentity?.IsAdministrator) switch
        {
            ("" or null, false) => "Not authenticated",
            ("", true) => "unknown (admin)",
            (_, true) => $"{appIdentity.Username} (admin)",
            _ => appIdentity?.Username ?? "Unknown"
        };

    static public bool IsAuthorizedFor(this AppIdentity? appIdentity, Type type)
        => type.GetCustomAttribute<AuthorizedPluginAttribute>() switch
        {
            null => true,
            { RequireAdminRole: true } => appIdentity?.IsAdministrator == true,
            _ => true
        };

    static public string UsernameWithoutSpatialSigns(
        this AppIdentity? appIdentity, char replaceWith='_')
    {
        if (appIdentity == null) return "";

        var username= appIdentity.Username;
        foreach(char sign in new char[] { '@', '/', '\\' })
        {
            username = username.Replace(sign, replaceWith);
        }
        return username;
    }
}

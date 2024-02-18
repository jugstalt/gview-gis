using gView.Blazor.Core.Extensions;
using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.Common;
using System.Collections.Generic;
using System.Linq;

namespace gView.Blazor.Core.Services;

public class PluginManagerService
{
    private readonly PlugInManager _plugInManager;
    private readonly IAppIdentityProvider _identityProvider;

    public PluginManagerService(IAppIdentityProvider identityProvider)
    {
        _plugInManager = new PlugInManager();
        _identityProvider = identityProvider;
    }

    public IEnumerable<T> GetPlugins<T>(Plugins.Type pluginType)
        => _plugInManager
            .GetPlugins(pluginType)
            .Where(t => _identityProvider.Identity.IsAuthorizedFor(t))
            .Select(t => _plugInManager.CreateInstance<T>(t))
            .ToArray();


    public IEnumerable<System.Type> GetPluginTypes(Plugins.Type pluginType)
        => _plugInManager
            .GetPlugins(pluginType)
            .Where(t => _identityProvider.Identity.IsAuthorizedFor(t))
            .ToArray();
}

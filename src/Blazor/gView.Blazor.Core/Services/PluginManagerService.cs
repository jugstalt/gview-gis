using gView.Blazor.Core.Extensions;
using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.Common;
using System;
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

    public Guid PluginGuid(object plugin)
        => PlugInManager.PlugInID(plugin);

    public IEnumerable<T> GetPlugins<T>(gView.Framework.Common.Plugins.Type pluginType)
        => _plugInManager
            .GetPlugins(pluginType)
            .Where(t => _identityProvider.Identity.IsAuthorizedFor(t))
            .Select(t => _plugInManager.CreateInstance<T>(t))
            .ToArray();


    public IEnumerable<System.Type> GetPluginTypes(gView.Framework.Common.Plugins.Type pluginType)
        => _plugInManager
            .GetPlugins(pluginType)
            .Where(t => _identityProvider.Identity.IsAuthorizedFor(t))
            .ToArray();
}

using gView.Framework.Common;
using gView.Pipe.Abstraction.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace gView.Pipe.Plugins.Gis;

/// <summary>
/// Points gView at the folder this plugin was loaded from: the PlugInManager looks for the data sources
/// there instead of in the folder of the host (gview-pipe ADR 0044).
/// </summary>
public sealed class GisPluginStartup : IPipePluginStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        PlugInManager.PluginDirectory = Path.GetDirectoryName(typeof(GisPluginStartup).Assembly.Location);
        PlugInManager.InitSilent = true;
        SystemInfo.RegisterApplicationEnvironment(SystemInfo.App.Command);
    }
}

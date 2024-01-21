using gView.Blazor.Core.Services;
using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Carto.Abstraction;

namespace gView.Carto.Plugins.Services;

public class CartoInteractiveToolService : ICartoInteractiveToolService
{
    private readonly ICartoInteractiveTool[] _scopedTools;
    public CartoInteractiveToolService(PluginManagerService pluginManager)
    {
        _scopedTools = pluginManager.GetPlugins<ICartoButton>(gView.Framework.Common.Plugins.Type.ICartoButton)
                                    .Where(t => t is ICartoInteractiveTool)
                                    .Select(t => (ICartoInteractiveTool)t)
                                    .ToArray();
    }

    public ICartoInteractiveTool? CurrentTool { get; set; }
    public IEnumerable<ICartoInteractiveTool> ScopedTools => _scopedTools;
}

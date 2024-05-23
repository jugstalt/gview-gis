using gView.Blazor.Core.Services;
using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Carto.Abstraction;

namespace gView.Carto.Plugins.Services;

public class CartoInteractiveToolService : ICartoInteractiveToolService
{
    private readonly ICartoInteractiveTool[] _scopedTools;
    private ICartoInteractiveTool? _currentTool;

    public CartoInteractiveToolService(PluginManagerService pluginManager)
    {
        _scopedTools = pluginManager.GetPlugins<ICartoButton>(gView.Framework.Common.Plugins.Type.ICartoButton)
                                    .Where(t => t is ICartoInteractiveTool)
                                    .Select(t => (ICartoInteractiveTool)t)
                                    .ToArray();
    }

    public ICartoInteractiveTool? CurrentTool 
    {
        get => _currentTool;
        set
        {
            _currentTool = value switch
            {
                null => null,
                _ => _scopedTools
                    .Where(t => t.GetType().Equals(value.GetType()))
                    .FirstOrDefault()
            };
        }

    }

    public T? GetCurrentToolContext<T>()
        where T : class
        => _currentTool?.ToolContext as T;

    public IEnumerable<ICartoInteractiveTool> ScopedTools => _scopedTools;  
}

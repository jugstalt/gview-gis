using gView.Carto.Core;
using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("FF437512-0CE4-44C1-888A-D1ECE06FED19")]
internal class Identify : ICartoTool
{
    public string Name => "Identify";

    public string ToolTip => "Identify Geo Object";

    public ToolType ToolType => ToolType.Rubberband;

    public string Icon => "webgis:identify";

    public CartoToolTarget Target => CartoToolTarget.Tools;

    public int SortOrder => 0;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope) => true;

    public Task<bool> OnEvent(ICartoApplicationScopeService scope)
    {
        throw new NotImplementedException();
    }
}

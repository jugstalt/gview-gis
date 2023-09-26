using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.system;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("540E07A0-B783-4D6D-AF65-3BC63740A3E9")]
public class SaveMap : ICartoTool
{
    public string Name => "Save Map";

    public string ToolTip => "Save the current map";

    public ToolType ToolType => ToolType.Command;

    public string Icon => "basic:disk-white";

    public CartoToolTarget Target => CartoToolTarget.General;

    public int SortOrder => 2;

    public void Dispose()
    {
        
    }

    public bool IsEnabled(IApplicationScope scope)
    {
        return true;
    }

    public Task<bool> OnEvent(IApplicationScope scope)
    {
        return Task.FromResult(true);
    }
}

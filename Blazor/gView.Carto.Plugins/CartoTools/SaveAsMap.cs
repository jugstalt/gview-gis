using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.system;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("1CA4CC29-FA96-4E0B-862F-C1D8CEDA7335")]
public class SaveAsMap : ICartoTool
{
    public string Name => "Save As ...";

    public string ToolTip => "Save the current map under a new filename";

    public ToolType ToolType => ToolType.Command;

    public string Icon => "basic:disks-white";

    public CartoToolTarget Target => CartoToolTarget.General;

    public int SortOrder => 3;

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

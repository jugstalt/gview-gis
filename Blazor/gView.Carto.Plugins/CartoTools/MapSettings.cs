using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("20BB9506-D6AE-4A81-AC2D-733DEE4465A4")]
internal class MapSettings : ICartoTool
{
    public string Name => "Map Settings";

    public string ToolTip => "";

    public ToolType ToolType => ToolType.Click;

    public string Icon => "basic:settings";

    public CartoToolTarget Target => CartoToolTarget.Map;

    public int SortOrder => 99;

    public void Dispose()
    {
        
    }

    public bool IsEnabled(IApplicationScope scope) => true;

    public Task<bool> OnEvent(IApplicationScope scope)
    {
        return Task.FromResult(false);
    }
}

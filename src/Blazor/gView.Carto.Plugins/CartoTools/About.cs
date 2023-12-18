using gView.Carto.Plugins.Extensions;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Razor.Dialogs.Models;
using gView.Framework.Carto;
using gView.Framework.Common;
using gView.Carto.Core.Services.Abstraction;

namespace gView.Carto.Plugins.CartoTools;


[RegisterPlugIn("C88FB907-852E-4799-8788-C34E04A73FFB")]
internal class About : ICartoTool
{
    public string Name => "About";

    public string ToolTip => "";

    public ToolType ToolType => ToolType.Click;

    public string Icon => "basic:help";

    public CartoToolTarget Target => CartoToolTarget.General;

    public int SortOrder => 999;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope) => true;

    async public Task<bool> OnEvent(ICartoApplicationScopeService scope)
    {
        await scope.ShowModalDialog(typeof(gView.Razor.Dialogs.AboutDialog),
                                     "About",
                                     new AboutDialogModel()
                                     {
                                         Title = "gView GIS Carto",
                                         Version = SystemInfo.Version
                                     });

        return true;
    }
}

using gView.Carto.Core;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Razor.Dialogs.Models;

namespace gView.Carto.Plugins.CartoTools;


[RegisterPlugIn("C88FB907-852E-4799-8788-C34E04A73FFB")]
internal class About : ICartoButton
{
    public string Name => "About";

    public string ToolTip => "";

    public string Icon => "basic:help";

    public CartoToolTarget Target => CartoToolTarget.General;

    public int SortOrder => 999;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope) => true;

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
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

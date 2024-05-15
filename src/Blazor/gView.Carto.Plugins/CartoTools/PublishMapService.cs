using gView.Carto.Core;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Carto.Razor.Components.Dialogs;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("362F584A-DD46-4269-A3D2-BD7EE82F5DDC")]
[AuthorizedPlugin(RequireAdminRole = true)]
public class PublishMapService : ICartoButton
{
    public string Name => "Publish Map";
    public string ToolTip => "Publish Map as gView.Server map service...";

    public string Icon => "basic:cloud-upload";

    public CartoToolTarget Target => CartoToolTarget.File;

    public int SortOrder => 99;

    public bool IsEnabled(ICartoApplicationScopeService scope)
    {
        return true;
    }

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        var _ = await scope.ShowModalDialog(
                typeof(PublishServiceDialog),
                "Publish Map Service",
                new PublishServiceModel() { Mxl = "empty" });

        return true;
    }
}

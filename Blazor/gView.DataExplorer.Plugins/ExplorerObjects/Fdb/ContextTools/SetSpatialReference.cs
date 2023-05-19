using gView.DataExplorer.Plugins.Extensions;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.Geometry;
using gView.Razor.Base;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.ContextTools;

internal class SetSpatialReference : IExplorerObjectContextTool
{
    public string Name => "Spatial Reference";
    public string Icon => "basic:globe";

    public bool IsEnabled(IApplicationScope scope, IExplorerObject exObject)
    {
        return true;
    }

    async public Task<bool> OnEvent(IApplicationScope scope, IExplorerObject exObject)
    {
        var model = await scope.ToScopeService().ShowKnownDialog(
            Framework.Blazor.KnownDialogs.SpatialReferenceDialog,
            model: new BaseDialogModel<ISpatialReference>()
                           {
                               Value = new SpatialReference("epsg:31256")
                           }
            );

        return true;
    }
}

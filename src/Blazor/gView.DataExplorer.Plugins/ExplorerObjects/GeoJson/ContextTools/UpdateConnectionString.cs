using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Models.Extensions;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.GeoJson.ContextTools;
internal class UpdateConnectionString : IExplorerObjectContextTool
{
    #region IExplorerObjectContextTool

    public string Name => "Connection String";

    public string Icon => "basic:edit-database";

    public bool IsEnabled(IExplorerApplicationScopeService scope, IExplorerObject exObject)
    {
        return exObject is GeoJsonServiceExplorerObject;
    }

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope, IExplorerObject exObject)
    {
        var connectionString = ((GeoJsonServiceExplorerObject)exObject).GetConnectionString();

        var model = await scope.ShowModalDialog(typeof(gView.DataExplorer.Razor.Components.Dialogs.GeoJsonConnectionDialog),
                                                                 "GeoJson Connection",
                                                                 connectionString.ToGeoJsonConnectionModel());

        if (model != null)
        {
            return await ((GeoJsonServiceExplorerObject)exObject).UpdateConnectionString(model.ToConnectionString());
        }

        return false;
    }

    #endregion
}

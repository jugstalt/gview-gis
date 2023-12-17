using gView.DataExplorer.Plugins.Extensions;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using System.Threading.Tasks;
using gView.DataExplorer.Razor.Components.Dialogs.Models.Extensions;
using gView.Framework.DataExplorer.Services.Abstraction;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Tiles.Vector.ContextTools;

public class UpdateConnectionString : IExplorerObjectContextTool
{
    #region IExplorerObjectContextTool

    public string Name => "Connection String";

    public string Icon => "basic:edit-database";

    public bool IsEnabled(IExplorerApplicationScopeService scope, IExplorerObject exObject)
    {
        return exObject is VectorTileCacheDatasetExplorerObject;
    }

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope, IExplorerObject exObject)
    {
        var connectionString = ((VectorTileCacheDatasetExplorerObject)exObject).GetConnectionString();

        var model = await scope.ShowModalDialog(typeof(gView.DataExplorer.Razor.Components.Dialogs.VectorTileCacheConnectionDialog),
                                                                 "Vector-Tile-Cache Connection",
                                                                 connectionString.ToVectorTileCacheConnectionModel());

        if (model != null)
        {
            return await ((VectorTileCacheDatasetExplorerObject)exObject).UpdateConnectionString(model.ToConnectionString());
        }

        return false;
    }

    #endregion
}

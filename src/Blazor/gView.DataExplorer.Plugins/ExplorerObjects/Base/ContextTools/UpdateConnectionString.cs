using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Base.ContextTools;
public class UpdateConnectionString : IExplorerObjectContextTool
{
    #region IExplorerObjectContextTool

    public string Name => "Connection String";

    public string Icon => "basic:edit-database";

    public bool IsEnabled(IExplorerApplicationScopeService scope, IExplorerObject exObject)
    {
        return exObject is IUpdateConnectionString;
    }

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope, IExplorerObject exObject)
    {
        var dbConnectionString = ((IUpdateConnectionString)exObject).GetDbConnectionString();

        var model = await scope.ShowKnownDialog(Framework.Blazor.KnownDialogs.ConnectionString,
                                                                 model: new ConnectionStringModel(dbConnectionString));

        if (model != null)
        {
            return await ((IUpdateConnectionString)exObject).UpdateDbConnectionString(model.DbConnectionString);
        }

        return false;
    }

    #endregion
}

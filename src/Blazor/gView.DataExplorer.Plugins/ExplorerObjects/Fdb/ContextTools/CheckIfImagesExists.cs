using gView.Cmd.Core.Abstraction;
using gView.Cmd.Fdb.Lib;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.DataSources.Fdb.MSSql;
using gView.DataSources.Fdb.PostgreSql;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.Blazor;
using gView.Framework.Core.Data;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.ContextTools;
internal class CheckIfImagesExists : IExplorerObjectContextTool
{
    public string Name => "Check, if images exists";

    public string Icon => "basic:check";

    public bool IsEnabled(IExplorerApplicationScopeService scope, IExplorerObject exObject) => true;

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope, IExplorerObject exObject)
    {
        var instance = await exObject.GetInstanceAsync() as IRasterCatalogClass;

        IDictionary<string, object> parameters = new Dictionary<string, object>();
        ICommand command = new ImageDatasetUtilCommand();

        parameters.Add("fdb", instance switch
        {
            SqlFDBImageCatalogClass => "sql",
            pgImageCatalogClass => "postgres",
            SQLiteFDBImageCatalogClass => "sqlite",
            _ => throw new System.Exception($"Can't remove images from {instance?.GetType().Name}")
        });
        parameters.Add("connstr", instance.Dataset.ConnectionString);
        parameters.Add("job", "check");

        if (await scope.ShowKnownDialog(
                    KnownDialogs.ExecuteCommand,
                    this.Name,
                    new ExecuteCommandModel()
                    {
                        CommandItems = new[]
                        {
                            new CommandItem()
                            {
                                Command = command,
                                Parameters = parameters
                            }
                        }
                    }) != null)
        {
            await scope.ForceContentRefresh();
        }

        return true;
    }
}

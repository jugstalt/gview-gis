using gView.Blazor.Core.Exceptions;
using gView.DataExplorer.Plugins.ExplorerObjects;
using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Data;
using gView.Framework.DataExplorer.Abstraction;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerTools;

[gView.Framework.system.RegisterPlugIn("7cbbed5e-c071-46de-b30a-9c6140dafd75")]
internal class AddNetworkDirectory : IExplorerTool
{
    #region IExplorerTool

    public string Name => "Map (network) folder...";

    public bool Enabled => true;

    public string ToolTip => "";

    public string Icon => "basic:open-in-window";

    public async Task<bool> OnEvent(IExplorerApplicationScope scope)
    {
        MapNetworkFolderModel? model = null;
        var scopeService = scope.ToScopeService();

        if(scopeService.CurrentExplorerObject is DirectoryObject)
        {
            model = new MapNetworkFolderModel()
            {
                FolderPath = ((DirectoryObject)scopeService.CurrentExplorerObject).FullName
            };
        }

        model = await scopeService.ShowModalDialog(
            typeof(Razor.Components.Dialogs.MapNetworkFolderDialog),
            this.Name,
            model);

        if (model == null)
        {
            throw new GeneralException("Dilaog Canceled");
        }

        throw new GeneralException($"The Result is: {model.FolderPath}");

        return true;
    }

    #endregion

    #region IOrder

    public int SortOrder => 25;

    #endregion

    #region IDisposable

    public void Dispose()
    {

    }

    #endregion
}

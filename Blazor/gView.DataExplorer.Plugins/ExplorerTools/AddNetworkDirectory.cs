using gView.DataExplorer.Core.Extensions;
using gView.Framework.DataExplorer.Abstraction;
using System;
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

    public Task<bool> OnEvent(IExplorerApplicationScope scope)
    {
        int i=new Random().Next();

        scope.ToScopeService().ShowModalDialog(
            typeof(gView.DataExplorer.Razor.Components.Dialogs.MapNetworkFolderDialog),
            (data) =>
            {
                i++;
            });

        return Task.FromResult(true);
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

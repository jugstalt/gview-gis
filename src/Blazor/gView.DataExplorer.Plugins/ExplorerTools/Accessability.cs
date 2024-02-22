using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerTools;

[RegisterPlugIn("38AAB3A0-7E30-4F56-95A0-978638C6162B")]
[AuthorizedPlugin(RequireAdminRole = true)]
public class Accessability : IExplorerTool
{
    public string Name => "Accessability";

    public string ToolTip => "Set object accessability";

    public string Icon => "basic:admin";

    public ExplorerToolTarget Target => ExplorerToolTarget.SelectedContextExplorerObjects;


    public bool IsEnabled(IExplorerApplicationScopeService scope)
    {
        return scope.ContextExplorerObjects?
            .Where(e => e is IExplorerObjectAccessability)
            .Count() == 1;
    }

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope)
    {
        var exObject = scope.ContextExplorerObjects?
            .Where(e => e is IExplorerObjectAccessability)
            .FirstOrDefault();
    
        if(exObject is not null)
        {
            var model = await scope.ShowModalDialog(
                   typeof(Razor.Components.Dialogs.ObjectAccessabilityDialog),
                   this.Name,
                   new ObjectAccessabilityModel() { ExplorerObject = exObject });

            if (model != null)
            {
                (exObject as IExplorerObjectAccessability)!
                             .Accessability = model.Accessability;

                //await scope.ForceContentRefresh();
            }
        }

        return true;
    }

    #region IDisposable

    public void Dispose()
    {

    }

    #endregion

    #region IOrder

    public int SortOrder => 99;

    #endregion
}

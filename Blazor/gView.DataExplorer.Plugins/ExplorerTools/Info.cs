using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.system;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using gView.Framework.Data;
using System.Reflection.Metadata.Ecma335;

namespace gView.DataExplorer.Plugins.ExplorerTools;

[gView.Framework.system.RegisterPlugIn("6B0B060E-45BC-4686-BEDE-23E61C849814")]
internal class Info : IExplorerTool
{
    #region IExplorerTool

    public string Name => "Info";

    public string ToolTip => "";

    public string Icon => "webgis:identify";

    public ExplorerToolTarget Target => ExplorerToolTarget.SelectedContextExplorerObjects;

    public bool IsEnabled(IApplicationScope scope)
    {
        var scopeService = scope.ToScopeService();

        //if (scopeService.ContextExplorerObjects != null &&
        //    scopeService.ContextExplorerObjects.Count() == 1)
        //{
        //    var pluginAttribute = scopeService.ContextExplorerObjects.First()
        //                                    .ObjectType
        //                                    .GetCustomAttribute<RegisterPlugInAttribute>();

        //    return pluginAttribute != null;
        //}

        return true;
    }

    async public Task<bool> OnEvent(IApplicationScope scope)
    {
        var scopeService = scope.ToScopeService();

        if (scopeService.ContextExplorerObjects is not null &&
            scopeService.ContextExplorerObjects.Count() == 1)
        {
            var exObject = scopeService.ContextExplorerObjects.First();
            var instance = await exObject.GetInstanceAsync();

            var pluginAttribute = instance?.GetType()
                                           .GetCustomAttribute<RegisterPlugInAttribute>();

            if (pluginAttribute is not null)
            {
                var dict = new Dictionary<string, string>()
                {
                    { "Type", exObject.GetType().ToString() },
                    { "Guid", pluginAttribute.Value.ToString() }
                };

                
                if(instance is IDataset)
                {
                    dict.Add("ConnectionString", ((IDataset)instance).ConnectionString);
                }
            }
        }

        //var exObject = scopeService.ContextExplorerObjects?
        //    .Where(e => e is IExplorerObjectRenamable)
        //    .FirstOrDefault();

        //if (exObject != null)
        //{
        //    var model = new RenameObjectModel() { ExplorerObject = exObject };

        //    model = await scopeService.ShowModalDialog(
        //           typeof(Razor.Components.Dialogs.RenameObjectDialog),
        //           this.Name,
        //           model);

        //    if (model != null)
        //    {
        //        await (exObject as IExplorerObjectRenamable)!
        //                     .RenameExplorerObject(model.NewName);

        //        await scopeService.EventBus.FireFreshContentAsync();
        //    }
        //}

        return true;
    }

    #endregion

    #region IOrder

    public int SortOrder => 16;

    #endregion

    #region IDisposable

    public void Dispose()
    {

    }

    #endregion
}

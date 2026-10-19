using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.RasterNet.Cog;

/// <summary>"New Connection...": asks for one or more URLs, checks them and stores every one as a connection.</summary>
[RegisterPlugIn("D07AB703-16E1-40C6-AAD0-C18AB9D420EE")]
public class CogNewConnectionExplorerObject : ExplorerObjectCls<CogGroupExplorerObject, UnknownObjectType>,
                                              IExplorerSimpleObject,
                                              IExplorerObjectDoubleClick,
                                              IExplorerObjectCreatable
{
    public CogNewConnectionExplorerObject() : base() { }

    public CogNewConnectionExplorerObject(CogGroupExplorerObject parent) : base(parent, 1) { }

    #region IExplorerObject Members

    public string Icon => "basic:round-plus";

    public string Name => "New Connection...";

    public string FullName => "";

    public string Type => "New Cloud Optimized GeoTIFF Connection";

    public void Dispose() { }

    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(null);

    #endregion

    #region IExplorerObjectDoubleClick Members

    async public Task ExplorerObjectDoubleClick(IExplorerApplicationScopeService appScope, ExplorerObjectEventArgs e)
    {
        var model = await appScope.ShowModalDialog(typeof(gView.DataExplorer.Razor.Components.Dialogs.CogConnectionDialog),
                                                   "Cloud Optimized GeoTIFF",
                                                   new CogConnectionModel { Check = CogConnections.CheckAsync });
        if (model == null)
        {
            return;
        }

        var config = CogConnections.Open(this, appScope);
        var taken = config.Connections.Keys.ToHashSet();
        foreach (var url in model.Lines)
        {
            string name = CogConnections.UniqueName(url, taken);
            taken.Add(name);
            config.Add(name, url);
            // The view reloads the group from the stored connections, so all new ones show up.
            e.NewExplorerObject ??= new CogConnectionExplorerObject(this.TypedParent, name, url);
        }
    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
        => Task.FromResult(cache?.Contains(FullName) == true ? cache[FullName] : null);

    #endregion

    #region IExplorerObjectCreatable Member

    public bool CanCreate(IExplorerObject parentExObject) => parentExObject is CogGroupExplorerObject;

    async public Task<IExplorerObject?> CreateExplorerObjectAsync(IExplorerApplicationScopeService appScope, IExplorerObject parentExObject)
    {
        var e = new ExplorerObjectEventArgs();
        await ExplorerObjectDoubleClick(appScope, e);
        return e.NewExplorerObject;
    }

    #endregion
}

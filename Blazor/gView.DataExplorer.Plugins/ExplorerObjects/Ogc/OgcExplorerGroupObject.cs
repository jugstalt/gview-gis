using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.system;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Ogc;

[gView.Framework.system.RegisterPlugIn("D6038DDE-DCB9-4cab-ADAC-C80EA323527D")]
public class OGCExplorerGroupObject : ExplorerParentObject, IExplorerGroupObject
{
    public OGCExplorerGroupObject() : base(null, null, 0) { }

    #region IExplorerGroupObject Members

    public string Icon => "basic:globe-table";

    #endregion

    #region IExplorerObject Members

    public string Name => "OGC";

    public string FullName => "OGC";

    public string Type => "OGC Connections";

    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(null);

    #endregion

    #region IExplorerParentObject Members

    async public override Task<bool> Refresh()
    {
        await base.Refresh();

        PlugInManager compMan = new PlugInManager();
        foreach (var compType in compMan.GetPlugins(gView.Framework.system.Plugins.Type.IExplorerObject))
        {
            IExplorerObject exObject = compMan.CreateInstance<IExplorerObject>(compType);
            if (!(exObject is IOgcGroupExplorerObject))
            {
                continue;
            }

            base.AddChildObject(exObject);
        }

        return true;
    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
    {
        if (cache.Contains(FullName))
        {
            return Task.FromResult<IExplorerObject?>(cache[FullName]);
        }

        if (this.FullName == FullName)
        {
            OGCExplorerGroupObject exObject = new OGCExplorerGroupObject();
            cache.Append(exObject);
            return Task.FromResult<IExplorerObject?>(exObject);
        }

        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion
}

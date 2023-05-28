using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Esri;

[gView.Framework.system.RegisterPlugIn("3DB3E792-AF34-4539-B68A-07007CE9BAEE")]
public class EsriExplorerGroupObject : 
        ExplorerParentObject, 
        IExplorerGroupObject
{
    public EsriExplorerGroupObject() : base() { }

    #region IExplorerGroupObject Members

    public string Icon => "basic:globe-table";

    public void SetParentExplorerObject(IExplorerObject parentExplorerObject)
    {
        base.Parent = parentExplorerObject;
    }

    #endregion

    #region IExplorerObject Members

    public string Name => "ESRI";

    public string FullName => "ESRI";

    public string Type => "ESRI Connections";

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
            if (!(exObject is IEsriGroupExplorerObject))
            {
                continue;
            }

            ((IEsriGroupExplorerObject)exObject).SetParentExplorerObject(this);

            base.AddChildObject(exObject);
        }

        return true;
    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache!=null && cache.Contains(FullName))
        {
            return Task.FromResult<IExplorerObject?>(cache[FullName]);
        }

        if (this.FullName == FullName)
        {
            EsriExplorerGroupObject exObject = new EsriExplorerGroupObject();
            cache?.Append(exObject);
            return Task.FromResult<IExplorerObject?>(exObject);
        }

        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion
}

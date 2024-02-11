using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.Framework.Common;
using gView.Framework.DataExplorer.Abstraction;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects;
internal class StartObject : ExplorerParentObject,
                             IExplorerRootObject
{
    public StartObject()
        : base() => (FileFilter) = (null);

    public StartObject(string fileFilter)
        : base() => (FileFilter) = (fileFilter);

    #region IExplorerObject Member

    public string Name
    {
        get { return "Start"; }
    }

    public string FullName
    {
        get { return ""; }
    }

    public string? Type
    {
        get { return ""; }
    }

    public string Icon => "basic:home";

    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(null);

    #endregion

    #region IExplorerRootObject

    public string? FileFilter { get; }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (FullName == String.Empty)
        {
            return Task.FromResult<IExplorerObject?>(new StartObject());
        }

        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion

    #region IExplorerParentObject Member

    async public override Task<bool> Refresh()
    {
        await base.Refresh();


        PlugInManager compMan = new PlugInManager();

        foreach (var exObjectType in compMan.GetPlugins(Framework.Common.Plugins.Type.IExplorerObject))
        {
            var exObject = compMan.CreateInstance<IExplorerObject>(exObjectType);

            if (!(exObject is IExplorerGroupObject))
            {
                continue;
            }

            ((IExplorerGroupObject)exObject).SetParentExplorerObject(this);
            base.AddChildObject(exObject);
        }

        return true;
    }

    #endregion
}

using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Tiles.Raster;

//[RegisterPlugIn("b279a036-56c9-499c-99a7-2ec490988be6")]
public class TileCacheNewConnectionObject : ExplorerObjectCls<TileCacheGroupExplorerObject>,
                                            IExplorerSimpleObject,
                                            IExplorerObjectDoubleClick,
                                            IExplorerObjectCreatable
{
    public TileCacheNewConnectionObject()
        : base() { }
    public TileCacheNewConnectionObject(TileCacheGroupExplorerObject parent)
        : base(parent, 0) { }

    #region IExplorerSimpleObject Members

    public string Icon => "basic:round-plus";

    #endregion

    #region IExplorerObject Members

    public string Name => "New Connection...";

    public string FullName => "";

    public string Type => "New Raster Tile Cache Connection";

    public void Dispose()
    {

    }

    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(null);

    #endregion

    #region IExplorerObjectDoubleClick Members

    public Task ExplorerObjectDoubleClick(IApplicationScope appScope, ExplorerObjectEventArgs e)
    {
        return Task.CompletedTask;
        /*
        FormTileCacheConnection dlg = new FormTileCacheConnection();

        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ConfigConnections connStream = new ConfigConnections("TileCache", "b9d6ae5b-9ca1-4a52-890f-caa4009784d4");

            string connectionString = dlg.ConnectionString;
            string id = dlg.TileCacheName;
            id = connStream.GetName(id);

            connStream.Add(id, connectionString);
            e.NewExplorerObject = new TileCacheDatasetExplorerObject(this.ParentExplorerObject, id, connectionString);
        }
        */
    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache?.Contains(FullName) == true)
        {
            return Task.FromResult<IExplorerObject?>(cache[FullName]);
        }

        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion

    #region IExplorerObjectCreatable Member

    public bool CanCreate(IExplorerObject parentExObject)
    {
        return (parentExObject is TileCacheGroupExplorerObject);
    }

    async public Task<IExplorerObject?> CreateExplorerObjectAsync(IApplicationScope appScope, IExplorerObject parentExObject)
    {
        ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
        await ExplorerObjectDoubleClick(appScope, e);
        return e.NewExplorerObject;
    }

    #endregion
}

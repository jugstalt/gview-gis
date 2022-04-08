using gView.Framework.IO;
using gView.Framework.system.UI;
using gView.Framework.UI;
using gView.Win.DataSources.VectorTileCache.UI.Explorer.Dialogs;
using System.Threading.Tasks;

namespace gView.Win.DataSources.VectorTileCache.UI.Explorer
{
    [gView.Framework.system.RegisterPlugIn("4C43C243-43D7-4B57-AD52-504659561C6E")]
    public class VectorTileCacheNewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new Icons.VectorTileCacheNewConnectionIcon();

        public VectorTileCacheNewConnectionObject()
            : base(null, null, 0) { }
        public VectorTileCacheNewConnectionObject(IExplorerObject parent)
            : base(parent, null, 0) { }

        #region IExplorerSimpleObject Members

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get { return "New Connection..."; }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return "New Tile Cache Connection"; }
        }

        public void Dispose()
        {

        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(null);
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }
        #endregion

        #region IExplorerObjectDoubleClick Members

        public void ExplorerObjectDoubleClick(ExplorerObjectEventArgs e)
        {
            var dlg = new FormVectorTileCacheConnection();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ConfigConnections connStream = new ConfigConnections("VectorTileCache", "b9d6ae5b-9ca1-4a52-890f-caa4009784d4");

                string connectionString = dlg.ConnectionString;
                string id = dlg.VectorTileCacheName;
                id = connStream.GetName(id);

                connStream.Add(id, connectionString);
                e.NewExplorerObject = new VectorTileCacheDatasetExplorerObject(this.ParentExplorerObject, id, connectionString);
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
            {
                return Task.FromResult<IExplorerObject>(cache[FullName]);
            }

            return Task.FromResult<IExplorerObject>(null);
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return (parentExObject is VectorTileCacheGroupExplorerObject);
        }

        public Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return Task.FromResult<IExplorerObject>(e.NewExplorerObject);
        }

        #endregion
    }
}

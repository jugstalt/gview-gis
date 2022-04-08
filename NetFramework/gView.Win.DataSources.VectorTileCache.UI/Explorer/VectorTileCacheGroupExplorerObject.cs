using gView.Framework.IO;
using gView.Framework.system.UI;
using gView.Framework.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Win.DataSources.VectorTileCache.UI.Explorer
{
    [gView.Framework.system.RegisterPlugIn("AD895DDE-6951-4B9B-87B5-298767AF06D2")]
    public class VectorTileCacheGroupExplorerObject : ExplorerParentObject, IExplorerGroupObject
    {
        private IExplorerIcon _icon = new Icons.VectorTileCacheGroupIcon();

        public VectorTileCacheGroupExplorerObject()
            : base(null, null, 0)
        {
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return "Vector Tile Caches"; }
        }

        public string FullName
        {
            get { return "VectorTileCache"; }
        }

        public string Type
        {
            get { return "Vector Tile Cache Connections"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(null);
        }

        #endregion

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
            {
                return Task.FromResult<IExplorerObject>(cache[FullName]);
            }

            if (this.FullName == FullName)
            {
                VectorTileCacheGroupExplorerObject exObject = new VectorTileCacheGroupExplorerObject();
                cache.Append(exObject);

                return Task.FromResult<IExplorerObject>(exObject);
            }

            return Task.FromResult<IExplorerObject>(null);
        }

        #endregion

        #region IExplorerParentObject Members

        async public override Task<bool> Refresh()
        {
            await base.Refresh();

            base.AddChildObject(new VectorTileCacheNewConnectionObject(this));

            ConfigConnections conStream = new ConfigConnections("VectorTileCache", "b9d6ae5b-9ca1-4a52-890f-caa4009784d4");
            Dictionary<string, string> connStrings = conStream.Connections;
            foreach (string connString in connStrings.Keys)
            {
                base.AddChildObject(new VectorTileCacheDatasetExplorerObject(this, connString, connStrings[connString]));
            }

            return true;
        }

        #endregion
    }
}

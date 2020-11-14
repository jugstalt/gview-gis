using gView.DataSources.VectorTileCache;
using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.system.UI;
using gView.Framework.UI;
using gView.Win.DataSources.VectorTileCache.UI.Explorer.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Win.DataSources.VectorTileCache.UI.Explorer
{
    [gView.Framework.system.RegisterPlugIn("9F64AC86-4FE0-4E34-85C5-23BFF2DB42D2")]
    public class VectorTileCacheDatasetExplorerObject : ExplorerParentObject, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable, ISerializableExplorerObject, IExplorerObjectContextMenu
    {
        private IExplorerIcon _icon = new Icons.VectorTileCacheDatasetIcon();
        private string _name = String.Empty, _connectionString = String.Empty;
        private ToolStripItem[] _contextItems = null;
        private Dataset _dataset = null;

        public VectorTileCacheDatasetExplorerObject()
            : base(null, null, 0) { }
        public VectorTileCacheDatasetExplorerObject(IExplorerObject parent, string name, string connectionString)
            : base(parent, null, 0)
        {
            _name = name;
            _connectionString = connectionString;

            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            ToolStripMenuItem item = new ToolStripMenuItem("Connection Properties...");
            item.Click += new EventHandler(ConnectionProperties_Click);
            items.Add(item);

            _contextItems = items.ToArray();
        }

        void ConnectionProperties_Click(object sender, EventArgs e)
        {
            var dlg = new FormVectorTileCacheConnection();
            dlg.ConnectionString = _connectionString;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ConfigConnections connStream = new ConfigConnections("VectorTileCache", "b9d6ae5b-9ca1-4a52-890f-caa4009784d4");
                connStream.Add(_name, this.ConnectionString = dlg.ConnectionString);
            }
        }

        internal string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
                _dataset = null;
            }
        }

        #region IExplorerObject Members

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string FullName
        {
            get
            {
                return @"TileCache\" + _name;
            }
        }

        public string Type
        {
            get { return "Vector Tile Cache Dataset"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        new public void Dispose()
        {
            base.Dispose();
        }
        async public Task<object> GetInstanceAsync()
        {
            if (_dataset == null)
            {
                _dataset = new Dataset();

                if (await _dataset.SetConnectionString(_connectionString) && await _dataset.Open())
                    return _dataset;
            }

            return null;
        }

        #endregion

        #region IExplorerParentObject Members

        async public override Task<bool> Refresh()
        {
            await base.Refresh();

            try
            {
                Dataset dataset = new Dataset();
                await dataset.SetConnectionString(_connectionString);
                await dataset.Open();

                var elements = await dataset.Elements();
                if (elements != null)
                {
                    foreach (IDatasetElement element in elements)
                    {
                        if (element.Class is IFeatureClass)
                            base.AddChildObject(new VectorTileCacheLayerExplorerObject(this, element));
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        #endregion

        #region ISerializableExplorerObject Member

        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
                return cache[FullName];

            VectorTileCacheGroupExplorerObject group = new VectorTileCacheGroupExplorerObject();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2)
                return null;

            group = (VectorTileCacheGroupExplorerObject)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

            var childObjects = await group.ChildObjects();
            if (childObjects != null)
            {
                foreach (IExplorerObject exObject in childObjects)
                {
                    if (exObject.FullName == FullName)
                    {
                        cache.Append(exObject);
                        return exObject;
                    }
                }
            }

            return null;
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            ConfigConnections stream = new ConfigConnections("VectorTileCache", "b9d6ae5b-9ca1-4a52-890f-caa4009784d4");
            stream.Remove(_name);

            if (ExplorerObjectDeleted != null)
                ExplorerObjectDeleted(this);

            return Task.FromResult(true);
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed = null;

        public Task<bool> RenameExplorerObject(string newName)
        {
            bool ret = false;
            ConfigConnections stream = new ConfigConnections("VectorTileCache", "b9d6ae5b-9ca1-4a52-890f-caa4009784d4");
            ret = stream.Rename(_name, newName);

            if (ret == true)
            {
                _name = newName;
                if (ExplorerObjectRenamed != null)
                    ExplorerObjectRenamed(this);
            }

            return Task.FromResult(ret);
        }

        #endregion

        #region IExplorerObjectContextMenu Member

        public ToolStripItem[] ContextMenuItems
        {
            get { return _contextItems; }
        }

        #endregion
    }
}

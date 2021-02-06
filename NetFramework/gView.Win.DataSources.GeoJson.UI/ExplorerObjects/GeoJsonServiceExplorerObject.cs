using gView.DataSources.GeoJson;
using gView.Framework.Data;
using gView.Framework.Globalisation;
using gView.Framework.IO;
using gView.Framework.system.UI;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Win.DataSources.GeoJson.UI.ExplorerObjects
{
    public class GeoJsonServiceExplorerObject : ExplorerParentObject, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable, ISerializableExplorerObject, IExplorerObjectContextMenu
    {
        private IExplorerIcon _icon = new GeoJsonServiceConnectionsIcon();
        private string _connectionString = "";
        private string _name = "";
        private IFeatureDataset _dataset;
        private ToolStripItem[] _contextItems = null;

        public GeoJsonServiceExplorerObject() 
            : base(null, typeof(IFeatureDataset), 1) { }

        public GeoJsonServiceExplorerObject(IExplorerObject parent, string name, string connectionString)
            : base(parent, typeof(IFeatureDataset), 1)
        {
            _name = name;
            _connectionString = connectionString;

            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
            ToolStripMenuItem item = new ToolStripMenuItem(LocalizedResources.GetResString("Menu.ConnectionProperties", "Connection Properties..."));
            item.Click += new EventHandler(ConnectionProperties_Click);
            items.Add(item);

            _contextItems = items.ToArray();
        }

        void ConnectionProperties_Click(object sender, EventArgs e)
        {
            if (_connectionString == null)
            {
                return;
            }

            FormGeoJsonConnection dlg = new FormGeoJsonConnection();
            dlg.ConnectionString = _connectionString;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string connectionString = dlg.ConnectionString;

                ConfigConnections connStream = new ConfigConnections(GeoJsonServiceGroupObject.ConfigName, GeoJsonServiceGroupObject.EncKey);
                connStream.Add(_name, connectionString);

                _connectionString = connectionString;
            }
        }

        internal string ConnectionString
        {
            get
            {
                return _connectionString;
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
                return this.ParentExplorerObject.FullName + @"\" + _name;
            }
        }

        public string Type
        {
            get { return "GeoJsonService Connection"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        async public Task<object> GetInstanceAsync()
        {
            if (_dataset != null)
            {
                _dataset.Dispose();
            }

            _dataset = new GeoJsonServiceDataset();
            await _dataset.SetConnectionString(_connectionString);
            await _dataset.Open();

            return _dataset;
        }

        #endregion

        #region IExplorerParentObject Members

        async public override Task<bool> Refresh()
        {
            await base.Refresh();
            if (_connectionString == null)
            {
                return false;
            }

            var dataset = new GeoJsonServiceDataset();
            await dataset.SetConnectionString(_connectionString);
            await dataset.Open();

            List<IDatasetElement> elements = await dataset.Elements();

            if (elements == null)
            {
                return false;
            }

            foreach (IDatasetElement element in elements)
            {
                if (element.Class is IFeatureClass)
                {
                    base.AddChildObject(new GeoJsonServiceFeatureClassExplorerObject(this, element));
                }
            }

            return true;
        }

        #endregion

        #region ISerializableExplorerObject Member

        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
            {
                return cache[FullName];
            }

            GeoJsonServiceGroupObject group = new GeoJsonServiceGroupObject();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2)
            {
                return null;
            }

            group = (GeoJsonServiceGroupObject)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

            foreach (IExplorerObject exObject in await group.ChildObjects())
            {
                if (exObject.FullName == FullName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            //ConfigTextStream stream = new ConfigTextStream("postgis_connections", true, true);
            //stream.Remove(this.Name, _connectionString);
            //stream.Close();
            //if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
            //return true;

            bool ret = false;
            if (_connectionString != null)
            {
                ConfigConnections stream = new ConfigConnections(GeoJsonServiceGroupObject.ConfigName, GeoJsonServiceGroupObject.EncKey);
                ret = stream.Remove(_name);
            }

            if (ret && ExplorerObjectDeleted != null)
            {
                ExplorerObjectDeleted(this);
            }

            return Task.FromResult(ret);
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed = null;

        public Task<bool> RenameExplorerObject(string newName)
        {
            bool ret = false;
            if (_connectionString != null)
            {
                ConfigConnections stream = new ConfigConnections(GeoJsonServiceGroupObject.ConfigName, GeoJsonServiceGroupObject.EncKey);
                ret = stream.Rename(_name, newName);
            }
            if (ret == true)
            {
                _name = newName;
                if (ExplorerObjectRenamed != null)
                {
                    ExplorerObjectRenamed(this);
                }
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

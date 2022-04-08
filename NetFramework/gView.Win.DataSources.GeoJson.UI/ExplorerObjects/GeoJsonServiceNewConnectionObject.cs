using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.system.UI;
using gView.Framework.UI;
using System.Threading.Tasks;

namespace gView.Win.DataSources.GeoJson.UI.ExplorerObjects
{
    [gView.Framework.system.RegisterPlugIn("DAF698C1-54F2-4199-A145-A7A911B882DF")]
    public class GeoJsonServiceNewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new GeoJsonServiceNewConnectionIcon();

        public GeoJsonServiceNewConnectionObject()
            : base(null, null, 1)
        {
        }

        public GeoJsonServiceNewConnectionObject(IExplorerObject parent)
            : base(parent, null, 1)
        {
        }

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
            get { return "New GeoJson Service Connection"; }
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
            FormGeoJsonConnection dlg = new FormGeoJsonConnection();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string connectionString = dlg.ConnectionString;
                string name = connectionString.ExtractConnectionStringParameter("target");

                ConfigConnections connStream = new ConfigConnections(GeoJsonServiceGroupObject.ConfigName, GeoJsonServiceGroupObject.EncKey);
                connStream.Add(name, connectionString);

                e.NewExplorerObject = new GeoJsonServiceExplorerObject(this.ParentExplorerObject, name, connectionString);
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
            return (parentExObject is GeoJsonServiceGroupObject);
        }

        public Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return Task.FromResult(e.NewExplorerObject);
        }

        #endregion
    }
}

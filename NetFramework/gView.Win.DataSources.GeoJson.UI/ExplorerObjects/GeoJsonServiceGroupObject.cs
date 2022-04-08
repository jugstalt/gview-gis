using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.system.UI;
using gView.Framework.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Win.DataSources.GeoJson.UI.ExplorerObjects
{
    [RegisterPlugIn("57AF3D70-6BAD-4284-B2F7-9C0362D5F7F5")]
    public class GeoJsonServiceGroupObject : ExplorerParentObject, IExplorerGroupObject
    {
        internal const string ConfigName = "GeoJsonService";
        internal const string EncKey = "546B0513-D71D-4490-9E27-94CD5D72C64A";

        private GeoJsonServiceConnectionsIcon _icon = new GeoJsonServiceConnectionsIcon();

        public GeoJsonServiceGroupObject()
            : base(null, null, 0)
        {
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return "GeoJson Service Connections"; }
        }

        public string FullName
        {
            get { return "GeoJsonServiceConnections"; }
        }

        public string Type
        {
            get { return "GeoJson Service Connections"; }
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
                GeoJsonServiceGroupObject exObject = new GeoJsonServiceGroupObject();
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

            base.AddChildObject(new GeoJsonServiceNewConnectionObject(this));

            ConfigConnections conStream = new ConfigConnections(ConfigName, EncKey);
            Dictionary<string, string> DbConnectionStrings = conStream.Connections;
            foreach (string name in DbConnectionStrings.Keys)
            {
                var connectionString = DbConnectionStrings[name];
                base.AddChildObject(new GeoJsonServiceExplorerObject(this, name, connectionString));
            }

            return true;
        }

        #endregion
    }
}

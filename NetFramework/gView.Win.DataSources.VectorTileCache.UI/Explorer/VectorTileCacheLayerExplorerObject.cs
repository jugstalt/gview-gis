using gView.Framework.Data;
using gView.Framework.system.UI;
using gView.Framework.UI;
using System.Threading.Tasks;

namespace gView.Win.DataSources.VectorTileCache.UI.Explorer
{
    [gView.Framework.system.RegisterPlugIn("23781302-2B04-4ECF-82A8-246A0C0DCA42")]
    public class VectorTileCacheLayerExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject
    {
        private string _fcname = "";
        private IExplorerIcon _icon = new Icons.VectorTileCacheDatasetIcon();
        private IFeatureClass _fc = null;
        private VectorTileCacheDatasetExplorerObject _parent = null;

        public VectorTileCacheLayerExplorerObject() : base(null, typeof(FeatureClass), 1) { }
        public VectorTileCacheLayerExplorerObject(VectorTileCacheDatasetExplorerObject parent, IDatasetElement element)
            : base(parent, typeof(FeatureClass), 1)
        {
            if (element == null)
            {
                return;
            }

            _parent = parent;
            _fcname = element.Title;

            if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;

            }
        }

        #region IExplorerObject Members

        public string Name
        {
            get { return _fcname; }
        }

        public string FullName
        {
            get
            {
                return _parent.FullName + @"\" + _fcname;
            }
        }
        public string Type
        {
            get { return "Vector Tile Cache Layer"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }
        public void Dispose()
        {
            if (_fc != null)
            {
                _fc = null;
            }
        }
        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(_fc);
        }
        #endregion

        #region ISerializableExplorerObject Member

        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
            {
                return cache[FullName];
            }

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1)
            {
                return null;
            }

            string[] parts = FullName.Split('\\');
            if (parts.Length != 3)
            {
                return null;
            }

            var parent = new VectorTileCacheDatasetExplorerObject();

            parent = await parent.CreateInstanceByFullName(parts[0] + @"\" + parts[1], cache) as VectorTileCacheDatasetExplorerObject;

            if (parent == null)
            {
                return null;
            }

            var childObjects = await parent.ChildObjects();
            if (childObjects != null)
            {
                foreach (IExplorerObject exObject in childObjects)
                {
                    if (exObject.Name == parts[2])
                    {
                        cache.Append(exObject);
                        return exObject;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}

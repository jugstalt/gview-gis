using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system.UI;
using gView.Framework.UI;
using System.Threading.Tasks;

namespace gView.Win.DataSources.GeoJson.UI.ExplorerObjects
{
    [gView.Framework.system.RegisterPlugIn("368020E0-216E-46B8-8DAD-BAAC38D75B92")]
    public class GeoJsonServiceFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject
    {
        private string _fcname = "", _type = "";
        private IExplorerIcon _icon = null;
        private IFeatureClass _fc = null;
        private GeoJsonServiceExplorerObject _parent = null;

        public GeoJsonServiceFeatureClassExplorerObject() : base(null, typeof(IFeatureClass), 1) { }
        public GeoJsonServiceFeatureClassExplorerObject(GeoJsonServiceExplorerObject parent, IDatasetElement element)
            : base(parent, typeof(IFeatureClass), 1)
        {
            if (element == null || !(element.Class is IFeatureClass))
            {
                return;
            }

            _parent = parent;
            _fcname = element.Title;

            if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;
                switch (_fc.GeometryType)
                {
                    case GeometryType.Envelope:
                    case GeometryType.Polygon:
                        _icon = new GeoJsonServicePolygonIcon();
                        _type = "Polygon Featureclass";
                        break;
                    case GeometryType.Multipoint:
                    case GeometryType.Point:
                        _icon = new GeoJsonServicePointIcon();
                        _type = "Point Featureclass";
                        break;
                    case GeometryType.Polyline:
                        _icon = new GeoJsonServiceLineIcon();
                        _type = "Polyline Featureclass";
                        break;
                }
            }
        }

        internal string ConnectionString
        {
            get
            {
                if (_parent == null)
                {
                    return "";
                }

                return _parent.ConnectionString;
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
                if (_parent == null)
                {
                    return "";
                }

                return _parent.FullName + @"\" + this.Name;
            }
        }
        public string Type
        {
            get { return _type; }
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

            string dsName = FullName.Substring(0, lastIndex);
            string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            GeoJsonServiceExplorerObject dsObject = new GeoJsonServiceExplorerObject();
            dsObject = await dsObject.CreateInstanceByFullName(dsName, cache) as GeoJsonServiceExplorerObject;

            var childObjects = await dsObject?.ChildObjects();
            if (childObjects == null)
            {
                return null;
            }

            foreach (IExplorerObject exObject in childObjects)
            {
                if (exObject.Name == fcName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion
    }
}

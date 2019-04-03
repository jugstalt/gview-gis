using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Collections;
using gView.Framework.UI;

namespace gView.Framework.system.UI
{
    public class ExplorerObjectManager : ISerializableExplorerObjectCache
    {
        private List<IExplorerObject> _exObjectsCache = new List<IExplorerObject>();

        public void Dispose()
        {
            foreach (IExplorerObject exObject in _exObjectsCache)
            {
                exObject.Dispose();
            }
            _exObjectsCache.Clear();
        }

        private IExplorerObject GetExObjectFromCache(string FullName)
        {
            foreach (IExplorerObject exObject in _exObjectsCache)
            {
                if (exObject == null) continue;
                if (exObject.FullName == FullName) return exObject;
            }
            return null;
        }
        public IExplorerObject DeserializeExplorerObject(Guid guid, string FullName)
        {
            IExplorerObject cached = GetExObjectFromCache(FullName);
            if (cached != null) return cached;

            PlugInManager compManager = new PlugInManager();
            object obj = compManager.CreateInstance(guid);
            if (!(obj is ISerializableExplorerObject)) return null;

            return ((ISerializableExplorerObject)obj).CreateInstanceByFullName(FullName, this);
        }
        public IExplorerObject DeserializeExplorerObject(IExplorerObjectSerialization exObjectSerialization)
        {
            try
            {
                if (exObjectSerialization == null) return null;

                return DeserializeExplorerObject(
                    exObjectSerialization.Guid,
                    exObjectSerialization.FullName);
            }
            catch { return null; }
        }

        public List<IExplorerObject> DeserializeExplorerObject(IEnumerable<IExplorerObjectSerialization> list)
        {
            List<IExplorerObject> l = new List<IExplorerObject>();
            if (list == null) return l;

            foreach (IExplorerObjectSerialization ser in list)
            {
                IExplorerObject exObject = DeserializeExplorerObject(ser);
                if (exObject == null) continue;

                l.Add(exObject);
            }
            return l;
        }
        static public ExplorerObjectSerialization SerializeExplorerObject(IExplorerObject exObject)
        {
            if (!(exObject is ISerializableExplorerObject))
            {
                return null;
            }
            else
            {
                return new ExplorerObjectSerialization(exObject);
            }
        }

        #region ISerializableExplorerObjectCache Member

        public void Append(IExplorerObject exObject)
        {
            if (exObject == null || Contains(exObject.FullName)) return;
            _exObjectsCache.Add(exObject);
        }

        public bool Contains(string FullName)
        {
            foreach (IExplorerObject exObject in _exObjectsCache)
            {
                if (exObject == null) continue;
                if (exObject.FullName == FullName) return true;
            }
            return false;
        }

        public IExplorerObject this[string FullName]
        {
            get
            {
                return GetExObjectFromCache(FullName);
            }
        }

        #endregion
    }
}

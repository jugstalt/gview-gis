using gView.Framework.Data;
using gView.Framework.system.UI;
using gView.Framework.UI;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.Interoperability.OGC.UI.Dataset.GML
{
    [gView.Framework.system.RegisterPlugIn("7FE05D6F-F480-4ee5-B592-934826A1C17A")]
    public class GMLExplorerObject : ExplorerParentObject, IExplorerFileObject, IExplorerObjectDeletable
    {
        private string _filename = "";
        internal gView.Interoperability.OGC.Dataset.GML.Dataset _dataset = null;
        private GMLIcon _icon = new GMLIcon();

        public GMLExplorerObject() : base(null, typeof(OGC.Dataset.GML.Dataset), 2) { }
        public GMLExplorerObject(IExplorerObject parent, string filename)
            : base(parent, typeof(OGC.Dataset.GML.Dataset), 2)
        {
            _filename = filename;

            _dataset = new gView.Interoperability.OGC.Dataset.GML.Dataset();
        }
        internal GMLExplorerObject(IExplorerObject parent, GMLExplorerObject exObject)
            : base(parent, typeof(OGC.Dataset.GML.Dataset), 2)
        {
            _filename = exObject._filename;
            _dataset = exObject._dataset;
        }

        #region IExplorerFileObject Member

        public string Filter
        {
            get { return "*.gml|*.xml"; }
        }

        async public Task<IExplorerFileObject> CreateInstance(IExplorerObject parent, string filename)
        {
            GMLExplorerObject exObject = new GMLExplorerObject(null, filename);
            if (exObject._dataset.State != DatasetState.opened)
            {
                await exObject._dataset.Open();
            }
            return exObject;
        }

        #endregion

        #region IExplorerObject Member

        public string Name
        {
            get
            {
                try
                {
                    FileInfo fi = new FileInfo(_filename);
                    return fi.Name;
                }
                catch
                {
                    return "???.GML";
                }
            }
        }

        public string FullName
        {
            get { return _filename; }
        }

        public string Type
        {
            get { return "OGC GML File"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        async public Task<object> GetInstanceAsync()
        {
            await _dataset.SetConnectionString(_filename);
            await _dataset.Open();

            return _dataset;
        }

        #endregion

        #region ISerializableExplorerObject Member

        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
            {
                return cache[FullName];
            }

            try
            {
                GMLExplorerObject exObject = new GMLExplorerObject();
                exObject = await exObject.CreateInstance(null, FullName) as GMLExplorerObject;
                if (exObject != null)
                {
                    cache.Append(exObject);
                }
                return exObject;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region IExplorerParentObject Member

        async public override Task<bool> Refresh()
        {
            await base.Refresh();

            if (_dataset == null)
            {
                return false;
            }

            foreach (IDatasetElement element in await _dataset.Elements())
            {
                base.AddChildObject(new GMLFeatureClassExplorerObject(this, element.Title));
            }

            return true;
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted;

        public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            if (_dataset != null)
            {
                if (_dataset.Delete())
                {
                    if (ExplorerObjectDeleted != null)
                    {
                        ExplorerObjectDeleted(this);
                    }

                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        #endregion
    }

    public class GMLFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject
    {
        private string _fcName;
        private GMLExplorerObject _parent;
        private IFeatureClass _fc;
        private GMLFeatureClassIcon _icon = new GMLFeatureClassIcon();

        public GMLFeatureClassExplorerObject(GMLExplorerObject parent, string fcName)
            : base(parent, typeof(IFeatureClass), 1)
        {
            _parent = parent;
            _fcName = fcName;


        }

        async private Task<gView.Interoperability.OGC.Dataset.GML.Dataset> Dataset()
        {
            var instance = await _parent.GetInstanceAsync();
            if (_parent == null || !(instance is gView.Interoperability.OGC.Dataset.GML.Dataset))
            {
                return null;
            }

            return instance as gView.Interoperability.OGC.Dataset.GML.Dataset;
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return _fcName; }
        }

        public string FullName
        {
            get { return _parent.FullName + @"\" + Name; }
        }

        public string Type
        {
            get { return "GML Featureclass"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        async public Task<object> GetInstanceAsync()
        {
            if (_fc != null)
            {
                return _fc;
            }

            var datdaset = await this.Dataset();
            if (this.Dataset() != null)
            {
                IDatasetElement element = await datdaset.Element(_fcName);
                if (element != null)
                {
                    _fc = element.Class as IFeatureClass;
                }
            }

            return _fc;
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            return Task.FromResult<IExplorerObject>(null);
        }

        #endregion
    }

    class GMLIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("920FAAC4-4181-4729-A64B-65B02DF2883B"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.Interoperability.OGC.UI.Properties.Resources.gml; }
        }

        #endregion
    }

    class GMLFeatureClassIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("238B1043-F758-4c90-8539-9988D76AA5D0"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.Interoperability.OGC.UI.Properties.Resources.gml_layer; }
        }

        #endregion
    }
}

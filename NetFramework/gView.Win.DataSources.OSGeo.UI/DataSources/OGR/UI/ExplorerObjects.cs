using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.system.UI;
using gView.Framework.Data;
using System.IO;
using System.Windows.Forms;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.Globalisation;
using gView.Framework.IO;
using System.Threading.Tasks;

namespace gView.DataSources.OGR.UI
{
    [gView.Framework.system.RegisterPlugIn("095A5627-DCC3-4A58-AD81-336873CDC73B")]
    public class PersonalGDBExplorerObject : ExplorerParentObject, IExplorerFileObject, ISerializableExplorerObject, IExplorerObjectDeletable, IPlugInDependencies
    {
        private AccessFDBIcon _icon = new AccessFDBIcon();
        private string _filename = "", _errMsg = "";

        public PersonalGDBExplorerObject() : base(null, null, 2) { }
        public PersonalGDBExplorerObject(IExplorerObject parent, string filename)
            : base(parent, null, 2)
        {
            _filename = filename;
        }
        #region IExplorerObject Members

        public string Filter
        {
            get { return "*.mdb"; }
        }

        public string Name
        {
            get
            {
                try
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(_filename);
                    return fi.Name;
                }
                catch { return ""; }
            }
        }

        public string FullName
        {
            get
            {
                return _filename;
            }
        }

        public string Type
        {
            get { return "ESRI Personal Geodatabase"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(null);
        }

        async public Task<IExplorerFileObject> CreateInstance(IExplorerObject parent, string filename)
        {
            try
            {
                if (!(new FileInfo(filename).Exists)) return null;

                using (Dataset dataset = new Dataset())
                {
                    await dataset.SetConnectionString(filename);
                    if (await dataset.Open() == false)
                        return null;
                }
            }
            catch { return null; }

            return new PersonalGDBExplorerObject(parent, filename);
        }
        #endregion

        async private Task<List<IDatasetElement>> DatasetElements()
        {
            try
            {
                Dataset dataset = new Dataset();
                await dataset.SetConnectionString(_filename);

                List<IDatasetElement> elements = new List<IDatasetElement>();
                await dataset.Open();
                foreach (IDatasetElement element in await dataset.Elements())
                {
                    elements.Add(element);
                }

                dataset.Dispose();
                return elements;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return null;
            }
        }

        #region IExplorerParentObject Members

        async public override Task<bool>  Refresh()
        {
            await base.Refresh();
            List<IDatasetElement> elements = await DatasetElements();
            if (elements != null)
            {
                foreach (IDatasetElement element in elements)
                {
                    base.AddChildObject(new PersonalGDBFeatureClassExplorerObject(this, _filename, element));
                }
            }

            return true;
        }

        #endregion

        #region IExplorerObjectCommandParameters Members

        #endregion

        #region ISerializableExplorerObject Member

        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            IExplorerObject obj = (cache.Contains(FullName)) ? cache[FullName] : await CreateInstance(null, FullName);
            cache.Append(obj);
            return obj;
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            try
            {
                FileInfo fi = new FileInfo(_filename);
                fi.Delete();
                if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return Task.FromResult(false);
            }
        }

        #endregion

        #region IPlugInDependencies Members
        public bool HasUnsolvedDependencies()
        {
            return Dataset.hasUnsolvedDependencies;
        }
        #endregion
    }

    public class PersonalGDBFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject
    {
        private string _filename = "", _fcname = "", _type = "";
        private IExplorerIcon _icon = null;
        private IFeatureClass _fc = null;
        private PersonalGDBExplorerObject _parent = null;

        public PersonalGDBFeatureClassExplorerObject() : base(null, typeof(FeatureClass),1) { }
        public PersonalGDBFeatureClassExplorerObject(PersonalGDBExplorerObject parent, string filename, IDatasetElement element)
            : base(parent, typeof(FeatureClass),1)
        {
            if (element == null) return;

            _parent = parent;
            _filename = filename;
            _fcname = element.Title;

            if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;
                switch (_fc.GeometryType)
                {
                    case geometryType.Envelope:
                    case geometryType.Polygon:
                        _icon = new AccessFDBPolygonIcon();
                        _type = "Polygon Featureclass";
                        break;
                    case geometryType.Multipoint:
                    case geometryType.Point:
                        _icon = new AccessFDBPointIcon();
                        _type = "Point Featureclass";
                        break;
                    case geometryType.Polyline:
                        _icon = new AccessFDBLineIcon();
                        _type = "Polyline Featureclass";
                        break;
                    default:
                        _icon = new AccessFDBLineIcon();
                        _type = "Featureclass";
                        break;
                }
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
                return _filename + ((_filename != "") ? @"\" : "") + _fcname;
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

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            return Task.FromResult<IExplorerObject>(null);
            //if (cache.Contains(FullName)) return cache[FullName];

            //FullName = FullName.Replace("/", @"\");
            //int lastIndex = FullName.LastIndexOf(@"\");
            //if (lastIndex == -1) return null;

            //string dsName = FullName.Substring(0, lastIndex);
            //string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            //AccessFDBDatasetExplorerObject dsObject = new AccessFDBDatasetExplorerObject();
            //dsObject = (AccessFDBDatasetExplorerObject)dsObject.CreateInstanceByFullName(dsName, cache);
            //if (dsObject == null || dsObject.ChildObjects == null) return null;

            //foreach (IExplorerObject exObject in dsObject.ChildObjects)
            //{
            //    if (exObject.Name == fcName)
            //    {
            //        cache.Append(exObject);
            //        return exObject;
            //    }
            //}
            //return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("B15E022B-3ECC-45F9-8E36-E02946B12945")]
    public class DxfExplorerObject : ExplorerObjectCls, IExplorerFileObject, ISerializableExplorerObject, IExplorerObjectDeletable, IPlugInDependencies
    {
        private AccessFDBIcon _icon = new AccessFDBIcon();
        private string _filename = "", _errMsg = "";

        public DxfExplorerObject() : base(null, typeof(FeatureClass), 2) { }
        public DxfExplorerObject(IExplorerObject parent, string filename)
            : base(parent, typeof(FeatureClass), 2)
        {
            _filename = filename;
        }

        #region IExplorerFileObject
        public string Filter
        {
            get { return "*.dxf"; }
        }

        public Task<IExplorerFileObject> CreateInstance(IExplorerObject parent, string filename)
        {
            try
            {
                if (!(new FileInfo(filename).Exists)) return Task.FromResult<IExplorerFileObject>(null);
            }
            catch
            {
                return Task.FromResult<IExplorerFileObject>(null);
            }

            return Task.FromResult<IExplorerFileObject>(new DxfExplorerObject(parent, filename));
        }

        public string Name
        {
            get
            {
                try
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(_filename);
                    return fi.Name;
                }
                catch { return ""; }
            }
        }

        public string FullName
        {
            get
            {
                return _filename;
            }
        }

        public string Type
        {
            get { return "DXF File"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        async public Task<object> GetInstanceAsync()
        {
            List<IDatasetElement> elements = await DatasetElements();
            if (elements.Count == 1)
                return elements[0].Class;

            return null;
        }

        #endregion

        #region ISerializableExplorerObject Members
        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            IExplorerObject obj = (cache.Contains(FullName)) ? cache[FullName] : await CreateInstance(null, FullName);
            cache.Append(obj);
            return obj;
        }
        #endregion

        private List<IDatasetElement> _elements = null;
        async private Task<List<IDatasetElement>> DatasetElements()
        {
            if (_elements != null)
                return _elements;
            try
            {
                Dataset dataset = new Dataset();
                await dataset.SetConnectionString(_filename);

                List<IDatasetElement> elements = new List<IDatasetElement>();
                await dataset.Open();
                foreach (IDatasetElement element in await dataset.Elements())
                {
                    elements.Add(element);
                }

                dataset.Dispose();
                return _elements = elements;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return _elements = null;
            }
        }

        #region IExplorerParentObject Members

        //public override void Refresh()
        //{
        //    base.Refresh();
        //    List<IDatasetElement> elements = DatasetElements;
        //    if (elements != null)
        //    {
        //        foreach (IDatasetElement element in elements)
        //        {
        //            base.AddChildObject(new DxfeatureClassExplorerObject(this, _filename, element));
        //        }
        //    }
        //}

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            try
            {
                FileInfo fi = new FileInfo(_filename);
                fi.Delete();
                if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return Task.FromResult(false);
            }
        }

        #endregion

        public void Dispose() { }

        #region IPlugInDependencies Members
        public bool HasUnsolvedDependencies()
        {
            return Dataset.hasUnsolvedDependencies;
        }
        #endregion
    }

    public class DxfFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject
    {
        private string _filename = "", _fcname = "", _type = "";
        private IExplorerIcon _icon = new AccessFDBLineIcon();
        private IFeatureClass _fc = null;
        private DxfExplorerObject _parent = null;

        public DxfFeatureClassExplorerObject() : base(null, typeof(FeatureClass), 1) { }
        public DxfFeatureClassExplorerObject(DxfExplorerObject parent, string filename, IDatasetElement element)
            : base(parent, typeof(FeatureClass), 1)
        {
            if (element == null) return;

            _parent = parent;
            _filename = filename;
            _fcname = element.Title;

            if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;
                switch (_fc.GeometryType)
                {
                    case geometryType.Envelope:
                    case geometryType.Polygon:
                        _icon = new AccessFDBPolygonIcon();
                        _type = "Polygon Featureclass";
                        break;
                    case geometryType.Multipoint:
                    case geometryType.Point:
                        _icon = new AccessFDBPointIcon();
                        _type = "Point Featureclass";
                        break;
                    case geometryType.Polyline:
                        _icon = new AccessFDBLineIcon();
                        _type = "Polyline Featureclass";
                        break;
                }
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
                return _filename + ((_filename != "") ? @"\" : "") + _fcname;
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

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            return Task.FromResult<IExplorerObject>(null);
            //if (cache.Contains(FullName)) return cache[FullName];

            //FullName = FullName.Replace("/", @"\");
            //int lastIndex = FullName.LastIndexOf(@"\");
            //if (lastIndex == -1) return null;

            //string dsName = FullName.Substring(0, lastIndex);
            //string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            //AccessFDBDatasetExplorerObject dsObject = new AccessFDBDatasetExplorerObject();
            //dsObject = (AccessFDBDatasetExplorerObject)dsObject.CreateInstanceByFullName(dsName, cache);
            //if (dsObject == null || dsObject.ChildObjects == null) return null;

            //foreach (IExplorerObject exObject in dsObject.ChildObjects)
            //{
            //    if (exObject.Name == fcName)
            //    {
            //        cache.Append(exObject);
            //        return exObject;
            //    }
            //}
            //return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("6D0126BD-144B-42CA-96BE-ED0C4BCD18D8")]
    public class KmlExplorerObject : ExplorerParentObject, IExplorerFileObject, ISerializableExplorerObject, IExplorerObjectDeletable, IPlugInDependencies
    {
        private AccessFDBIcon _icon = new AccessFDBIcon();
        private string _filename = "", _errMsg = "";

        public KmlExplorerObject() : base(null, null, 2) { }
        public KmlExplorerObject(IExplorerObject parent, string filename)
            : base(parent, null, 2)
        {
            _filename = filename;
        }

        #region IExplorerFileObject
        public string Filter
        {
            get { return "*.kml"; }
        }

        public Task<IExplorerFileObject> CreateInstance(IExplorerObject parent, string filename)
        {
            try
            {
                if (!(new FileInfo(filename).Exists)) return Task.FromResult<IExplorerFileObject>(null);
            }
            catch { return null; }

            return Task.FromResult<IExplorerFileObject>(new KmlExplorerObject(parent, filename));
        }

        public string Name
        {
            get
            {
                try
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(_filename);
                    return fi.Name;
                }
                catch { return ""; }
            }
        }

        public string FullName
        {
            get
            {
                return _filename;
            }
        }

        public string Type
        {
            get { return "KML File"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public Task<object> GetInstanceAsync()
        {
            //List<IDatasetElement> elements = DatasetElements;
            //if (elements.Count == 1)
            //    return elements[0].Class;
            return Task.FromResult<object>(null);
        }

        #endregion

        #region ISerializableExplorerObject Members
        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            IExplorerObject obj = (cache.Contains(FullName)) ? cache[FullName] : await CreateInstance(null, FullName);
            cache.Append(obj);
            return obj;
        }
        #endregion

        async private Task<List<IDatasetElement>> DatasetElements()
        {
            try
            {
                Dataset dataset = new Dataset();
                await dataset.SetConnectionString(_filename);

                List<IDatasetElement> elements = new List<IDatasetElement>();
                await dataset.Open();
                foreach (IDatasetElement element in await dataset.Elements())
                {
                    elements.Add(element);
                }

                dataset.Dispose();
                return elements;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return null;
            }
        }

        #region IExplorerParentObject Members

        async public override Task<bool> Refresh()
        {
            await base.Refresh();
            List<IDatasetElement> elements = await DatasetElements();
            if (elements != null)
            {
                foreach (IDatasetElement element in elements)
                {
                    base.AddChildObject(new KmlFeatureClassExplorerObject(this, _filename, element));
                }
            }

            return true;
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            try
            {
                FileInfo fi = new FileInfo(_filename);
                fi.Delete();
                if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                return Task.FromResult<bool>(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return Task.FromResult<bool>(false);
            }
        }

        #endregion

        public void Dispose()
        {
            base.Dispose();
        }

        #region IPlugInDependencies Members
        public bool HasUnsolvedDependencies()
        {
            return Dataset.hasUnsolvedDependencies;
        }
        #endregion
    }

    public class KmlFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject
    {
        private string _filename = "", _fcname = "", _type = "";
        private IExplorerIcon _icon = new AccessFDBLineIcon();
        private IFeatureClass _fc = null;
        private KmlExplorerObject _parent = null;

        public KmlFeatureClassExplorerObject() : base(null, typeof(FeatureClass), 1) { }
        public KmlFeatureClassExplorerObject(KmlExplorerObject parent, string filename, IDatasetElement element)
            : base(parent, typeof(FeatureClass), 1)
        {
            if (element == null) return;

            _parent = parent;
            _filename = filename;
            _fcname = element.Title;

            if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;
                switch (_fc.GeometryType)
                {
                    case geometryType.Envelope:
                    case geometryType.Polygon:
                        _icon = new AccessFDBPolygonIcon();
                        _type = "Polygon Featureclass";
                        break;
                    case geometryType.Multipoint:
                    case geometryType.Point:
                        _icon = new AccessFDBPointIcon();
                        _type = "Point Featureclass";
                        break;
                    case geometryType.Polyline:
                        _icon = new AccessFDBLineIcon();
                        _type = "Polyline Featureclass";
                        break;
                }
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
                return _filename + ((_filename != "") ? @"\" : "") + _fcname;
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

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            return Task.FromResult<IExplorerObject>(null);
            //if (cache.Contains(FullName)) return cache[FullName];

            //FullName = FullName.Replace("/", @"\");
            //int lastIndex = FullName.LastIndexOf(@"\");
            //if (lastIndex == -1) return null;

            //string dsName = FullName.Substring(0, lastIndex);
            //string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            //AccessFDBDatasetExplorerObject dsObject = new AccessFDBDatasetExplorerObject();
            //dsObject = (AccessFDBDatasetExplorerObject)dsObject.CreateInstanceByFullName(dsName, cache);
            //if (dsObject == null || dsObject.ChildObjects == null) return null;

            //foreach (IExplorerObject exObject in dsObject.ChildObjects)
            //{
            //    if (exObject.Name == fcName)
            //    {
            //        cache.Append(exObject);
            //        return exObject;
            //    }
            //}
            //return null;
        }

        #endregion
    }

    public class OGRFeatureClassExplorerObject : ExplorerObjectCls, IExplorerFileObject, ISerializableExplorerObject
    {
        private string _filename = "", _type = "";
        private IFeatureClass _fc = null;
        private IExplorerIcon _icon = new AccessFDBLineIcon();

        public OGRFeatureClassExplorerObject() : base(null, typeof(FeatureClass), 1) { }

        private OGRFeatureClassExplorerObject(IExplorerObject parent, string filename)
            : base(parent, typeof(FeatureClass), 1)
        {

        }
        async static public Task<OGRFeatureClassExplorerObject> Create(IExplorerObject parent, string filename)
            
        {
            var exObject = new OGRFeatureClassExplorerObject(parent, filename);

            exObject._filename = filename;

            Dataset ds = new Dataset();
            await ds.SetConnectionString(filename);
            if (!await ds.Open())
            {
                if (ds.LastErrorMessage != String.Empty)
                    MessageBox.Show("ERROR:" + ds.LastErrorMessage);

                return exObject;
            }

            var elements = await ds.Elements();

            if (elements.Count == 1)
                exObject._fc = elements[0].Class as IFeatureClass;

            if (exObject._fc == null)
                return exObject;

            switch (exObject._fc.GeometryType)
            {
                case geometryType.Envelope:
                case geometryType.Polygon:
                    exObject._icon = new AccessFDBPolygonIcon();
                    exObject._type = "OGR Polygon Featureclass";
                    break;
                case geometryType.Multipoint:
                case geometryType.Point:
                    exObject._icon = new AccessFDBPointIcon();
                    exObject._type = "OGR Point Featureclass";
                    break;
                case geometryType.Polyline:
                    exObject._icon = new AccessFDBLineIcon();
                    exObject._type = "OGR Polyline Featureclass";
                    break;
            }

            return exObject;
        }

        #region IExplorerFileObject Member

        public string Filter
        {
            get { return "*.e00|*.gml|*.00d|*.adf"; }
        }

        public Task<IExplorerFileObject> CreateInstance(IExplorerObject parent, string filename)
        {
            return Task.FromResult<IExplorerFileObject>(new OGRFeatureClassExplorerObject(parent, filename));
        }

        #endregion

        #region IExplorerObject Member

        public string Name
        {
            get
            {
                try
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(_filename);
                    return fi.Name;
                }
                catch { return ""; }
            }
        }

        public string FullName
        {
            get { return _filename; }
        }

        public string Type
        {
            get { return _type; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(_fc);
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

    internal class AccessFDBIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("63D4FB22-D7E0-460c-A956-7DF0D6AE5DA5");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.Win.DataSources.GDAL.UI.Properties.Resources.dataset;
            }
        }

        #endregion
    }

    public class AccessFDBPointIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("A713AF39-D76C-4a78-AE84-9147B0E1D26B"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.GDAL.UI.Properties.Resources.field_geom_point; }
        }

        #endregion
    }

    public class AccessFDBLineIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("2FBE97B1-6604-4ee6-BEDF-E5795B9A4F88"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.GDAL.UI.Properties.Resources.field_geom_line; }
        }

        #endregion
    }

    public class AccessFDBPolygonIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("9C242F6F-08A1-4279-9180-1EB4286BEB58"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.GDAL.UI.Properties.Resources.field_geom_polygon; }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("d621846b-959b-4285-8506-908b527f0722")]
    public class OgrDatasetGroupObject : ExplorerParentObject, IPlugInDependencies, IExplorerGroupObject
    {
        private IExplorerIcon _icon = new OgrDatasetGroupIcon();

        public OgrDatasetGroupObject()
            : base(null, null, 0)
        {
        }

        #region IPlugInDependencies Member

        public bool HasUnsolvedDependencies()
        {
            return Dataset.hasUnsolvedDependencies;
        }

        #endregion

        #region IExplorerObject Member

        public string Name
        {
            get { return "OGR Simple Feature Library"; }
        }

        public string FullName
        {
            get { return "OGR"; }
        }

        public string Type
        {
            get { return "OGR Connections"; }
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
                return Task.FromResult<IExplorerObject>(cache[FullName]);

            if (this.FullName == FullName)
            {
                OgrDatasetGroupObject exObject = new OgrDatasetGroupObject();
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
            base.AddChildObject(new OgrNewConnectionObject(this));


            ConfigConnections conStream = new ConfigConnections("OGR", "ca7011b3-0812-47b6-a999-98a900c4087d");
            Dictionary<string, string> connStrings = conStream.Connections;
            foreach (string connString in connStrings.Keys)
            {
                base.AddChildObject(new OgrDatasetExplorerObject(this, connString, connStrings[connString]));
            }

            return true;
        }

        #endregion
    }

    // PG:dbname='postgis' host='localhost' port='5432' user='postgres' password='postgres'
    [gView.Framework.system.RegisterPlugIn("b279a036-56c9-499c-99a7-2ec490988be6")]
    public class OgrNewConnectionObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        private IExplorerIcon _icon = new OgrNewConnectionIcon();

        public OgrNewConnectionObject()
            : base(null, null, 0) { }
        public OgrNewConnectionObject(IExplorerObject parent)
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
            get { return LocalizedResources.GetResString("String.NewConnection", "New Connection..."); }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return "New OGR Connection"; }
        }

        public void Dispose()
        {

        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(null);
        }

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName)
        {
            return Task.FromResult<IExplorerObject>(null);
        }
        #endregion

        #region IExplorerObjectDoubleClick Members

        public void ExplorerObjectDoubleClick(ExplorerObjectEventArgs e)
        {
            FormNewOgrDataset dlg = new FormNewOgrDataset();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ConfigConnections connStream = new ConfigConnections("OGR", "ca7011b3-0812-47b6-a999-98a900c4087d");

                string connectionString = dlg.ConnectionString;
                string id = "OGR Connection";
                id = connStream.GetName(id);

                connStream.Add(id, connectionString);
                //e.NewExplorerObject = new OGRExplorerObject(this.ParentExplorerObject, id, dbConnStr);
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
                return Task.FromResult<IExplorerObject>(cache[FullName]);

            return Task.FromResult<IExplorerObject>(null);
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return (parentExObject is OgrDatasetGroupObject);
        }

        public Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return Task.FromResult(e.NewExplorerObject);
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("cae5aa56-0e41-4566-8031-6f18713d865d")]
    public class OgrDatasetExplorerObject : ExplorerParentObject, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable, ISerializableExplorerObject, IExplorerObjectContextMenu
    {
        private IExplorerIcon _icon = new OgrDatasetIcon();
        private string _name = String.Empty, _connectionString = String.Empty;
        private ToolStripItem[] _contextItems = null;
        private Dataset _dataset = null;

        public OgrDatasetExplorerObject()
            : base(null, null, 0) { }
        public OgrDatasetExplorerObject(IExplorerObject parent, string name, string connectionString)
            : base(parent, null, 0)
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
            FormNewOgrDataset dlg = new FormNewOgrDataset(_connectionString);


            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ConfigConnections connStream = new ConfigConnections("OGR", "ca7011b3-0812-47b6-a999-98a900c4087d");
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
                return @"OGR\" + _name;
            }
        }

        public string Type
        {
            get { return "Ogr Dataset"; }
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
                await _dataset.SetConnectionString(_connectionString);
                if (await _dataset.Open())
                    return _dataset;
            }
            return null;
        }

        #endregion

        private string[] LayerNames
        {
            get
            {
                try
                {
                    OSGeo_v1.OGR.Ogr.RegisterAll();

                    OSGeo_v1.OGR.DataSource dataSource = OSGeo_v1.OGR.Ogr.Open(this.ConnectionString, 0);
                    if (dataSource != null)
                    {
                        List<string> layers = new List<string>();
                        for (int i = 0; i < Math.Min(dataSource.GetLayerCount(), 20); i++)
                        {
                            OSGeo_v1.OGR.Layer ogrLayer = dataSource.GetLayerByIndex(i);
                            if (ogrLayer == null) continue;
                            layers.Add(ogrLayer.GetName());
                        }
                        return layers.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return null;
            }
        }

        #region IExplorerParentObject Members

        async public override Task<bool> Refresh()
        {
            await base.Refresh();

            try
            {
                Dataset dataset = new Dataset();
                await dataset.SetConnectionString(_connectionString);
                await dataset.Open();

                foreach (IDatasetElement element in await dataset.Elements())
                {
                    if (element.Class is IFeatureClass)
                        base.AddChildObject(new OgrLayerExplorerObject(this, element));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return true;
        }
        #endregion

        #region ISerializableExplorerObject Member

        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
                return cache[FullName];

            OgrDatasetGroupObject group = new OgrDatasetGroupObject();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2) return null;

            group = (OgrDatasetGroupObject)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

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
            ConfigConnections stream = new ConfigConnections("OGR", "ca7011b3-0812-47b6-a999-98a900c4087d");
            stream.Remove(_name);

            if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
            return Task.FromResult(true);
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed = null;

        public Task<bool> RenameExplorerObject(string newName)
        {
            bool ret = false;
            ConfigConnections stream = new ConfigConnections("OGR", "ca7011b3-0812-47b6-a999-98a900c4087d");
            ret = stream.Rename(_name, newName);

            if (ret == true)
            {
                _name = newName;
                if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
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

    [gView.Framework.system.RegisterPlugIn("137ab461-6e7b-4c43-8be2-515c9ca475d2")]
    public class OgrLayerExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject
    {
        private string _fcname = "", _type = "";
        private IExplorerIcon _icon = null;
        private IFeatureClass _fc = null;
        private OgrDatasetExplorerObject _parent = null;

        public OgrLayerExplorerObject() : base(null, typeof(FeatureClass), 1) { }
        public OgrLayerExplorerObject(OgrDatasetExplorerObject parent, IDatasetElement element)
            : base(parent, typeof(FeatureClass), 1)
        {
            if (element == null) return;

            _parent = parent;
            _fcname = element.Title;

            if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;
                switch (_fc.GeometryType)
                {
                    case geometryType.Envelope:
                    case geometryType.Polygon:
                        _icon = new AccessFDBPolygonIcon();
                        _type = "Polygon Featureclass";
                        break;
                    case geometryType.Multipoint:
                    case geometryType.Point:
                        _icon = new AccessFDBPointIcon();
                        _type = "Point Featureclass";
                        break;
                    case geometryType.Polyline:
                        _icon = new AccessFDBLineIcon();
                        _type = "Polyline Featureclass";
                        break;
                    default:
                        _icon = new AccessFDBLineIcon();
                        _type = "Featureclass";
                        break;
                }
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
                return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string [] parts = FullName.Split('\\');
            if(parts.Length!=3) return null;
            OgrDatasetExplorerObject parent = new OgrDatasetExplorerObject();
            parent = await parent.CreateInstanceByFullName(parts[0] + @"\" + parts[1], cache) as OgrDatasetExplorerObject;
            if (parent == null)
                return null;

            foreach (IExplorerObject exObject in await parent.ChildObjects())
            {
                if (exObject.Name == parts[2])
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion
    }

    public class OgrDatasetGroupIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("7075eae5-3c16-459f-ba59-cfa18f4a3eca"); }
        }

        public System.Drawing.Image Image
        {
            get { return gView.Win.DataSources.GDAL.UI.Properties.Resources.i_connection; }
        }

        #endregion
    }

    public class OgrNewConnectionIcon : IExplorerIcon
    {

        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("421e1e5e-c5a6-4d9e-9596-ee89c589bb6d"); }
        }

        public System.Drawing.Image Image
        {
            get { return gView.Win.DataSources.GDAL.UI.Properties.Resources.pointer_new; }
        }

        #endregion
    }

    public class OgrDatasetIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("056858f3-4d30-487e-b2e9-8184b97af43f"); }
        }

        public System.Drawing.Image Image
        {
            get { return gView.Win.DataSources.GDAL.UI.Properties.Resources.layers; }
        }

        #endregion
    }

}

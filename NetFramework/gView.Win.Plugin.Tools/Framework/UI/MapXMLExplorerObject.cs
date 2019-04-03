using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using gView.Framework.IO;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.system.UI;

namespace gView.Framework.UI
{
    [gView.Framework.system.RegisterPlugIn("ABE65EF4-6CFA-4d46-8392-5B4A214C1225")]
    public class MapDocumentExplorerObject : ExplorerParentObject, IExplorerFileObject, ISerializableExplorerObject
    {
        private IExplorerIcon _icon = new MapDocumentExploererIcon(0);
        private string _filename = "";
        private MapDocument _mapDocument = null;

        public MapDocumentExplorerObject() : base(null, typeof(MapDocument), 2) { }
        public MapDocumentExplorerObject(IExplorerObject parent, string filename)
            : base(parent, typeof(MapDocument), 2)
        {
            _filename = filename;
        }
        #region IExplorerFileObject Members

        public string Filter
        {
            get { return "*.mxl"; }
        }

        public IExplorerFileObject CreateInstance(IExplorerObject parent, string filename)
        {
            try
            {
                if (!(new FileInfo(filename).Exists)) return null;
            }
            catch { return null; }
            return new MapDocumentExplorerObject(parent, filename);
        }

        #endregion

        #region IExplorerObject Members

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
                    return "";
                }
            }
        }

        public string FullName
        {
            get { return _filename; }
        }

        public string Type
        {
            get { return "Map Xml Document"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public new object Object
        {
            get
            {
                return (_mapDocument != null) ? _mapDocument : new MapDocument();
            }
        }

        #endregion

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            try
            {
                base.Refresh();
                _mapDocument = new MapDocument();

                _mapDocument.LoadMapDocument(_filename);

                foreach (IMap map in _mapDocument.Maps)
                {
                    base.AddChildObject(new MapExplorerObject(this, _filename, map));
                }
            }
            catch { }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            try
            {
                if (cache.Contains(FullName)) return cache[FullName];
                FileInfo fi = new FileInfo(FullName);
                if (!fi.Exists || fi.Extension.ToLower() != ".mxl") return null;

                MapDocumentExplorerObject ex = new MapDocumentExplorerObject(null, FullName);
                cache.Append(ex);
                return ex;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("DD6ABADB-1011-4af1-975F-AB081ABCC400")]
    public class MapExplorerObject : ExplorerParentObject, IExplorerSimpleObject, ISerializableExplorerObject
    {
        private IMap _map = null;
        private string _filename = "";
        private IExplorerIcon _icon = new MapDocumentExploererIcon(1);

        public MapExplorerObject() : base(null, typeof(Map), 2) { }
        internal MapExplorerObject(IExplorerObject parent, string filename, IMap map)
            : base(parent, typeof(Map), 2)
        {
            _map = map;
            _filename = filename;
        }

        #region IExplorerObject Members

        public string Name
        {
            get
            {
                if (_map == null) return "???";
                return _map.Name;
            }
        }

        public string FullName
        {
            get { return _filename + @"\" + this.Name; }
        }

        public string Type
        {
            get { return "Map"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public new object Object
        {
            get { return _map; }
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region IExplorerParentObject Member

        public override void Refresh()
        {
            base.Refresh();
            if (_map == null) return;

            //foreach (IDatasetElement element in _map.MapElements)
            //{
            //    if (!(element is ILayer)) continue;
            //    _childs.Add(new FeatureLayerExplorerObject(this, (ILayer)element));
            //}
            foreach (ITOCElement element in _map.TOC.Elements)
            {
                if (element.ParentGroup != null) continue;
                if (element.ElementType == TOCElementType.ClosedGroup ||
                    element.ElementType == TOCElementType.OpenedGroup)
                {
                    base.AddChildObject(new TOCGroupElementExplorerObject(this, element));
                }
                else
                {
                    base.AddChildObject(new TOCElementExplorerObject(this, element));
                }
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string mxlName = FullName.Substring(0, lastIndex);
            string mapName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            MapDocumentExplorerObject mapDocument = new MapDocumentExplorerObject();
            mapDocument = mapDocument.CreateInstanceByFullName(mxlName, cache) as MapDocumentExplorerObject;
            if (mapDocument == null || mapDocument.ChildObjects == null) return null;

            foreach (MapExplorerObject mapObject in mapDocument.ChildObjects)
            {
                if (mapObject.Name == mapName)
                {
                    cache.Append(mapObject);
                    return mapObject;
                }
            }
            return null;
        }

        #endregion
    }

    //[gView.Framework.system.RegisterPlugIn("CA185303-D338-4101-B7B5-FD55035F58C7")]
    public class FeatureLayerExplorerObject : ExplorerObjectCls, IExplorerSimpleObject
    {
        private ILayer _layer;
        private IExplorerObject _parent;
        private IExplorerIcon _icon = new MapDocumentExploererIcon(1);

        internal FeatureLayerExplorerObject(IExplorerObject parent, ILayer layer)
            : base(parent, typeof(ILayer), 1)
        {
            _parent = parent;
            _layer = layer;
        }
        #region IExplorerObject Member

        public string Name
        {
            get { return (_layer != null) ? _layer.Title : ""; }
        }

        public string FullName
        {
            get
            {
                if (_parent == null || _layer == null) return "";
                return _parent.FullName + "/" + Name;
            }
        }

        public string Type
        {
            get { return "Feature Layer"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public void Dispose()
        {

        }

        public new object Object
        {
            get { return _layer; }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string mapName = FullName.Substring(0, lastIndex);
            string mflName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            MapExplorerObject mapObject = new MapExplorerObject();
            mapObject = mapObject.CreateInstanceByFullName(mapName, cache) as MapExplorerObject;
            if (mapObject == null || mapObject.ChildObjects == null) return null;

            foreach (FeatureLayerExplorerObject mflObject in mapObject.ChildObjects)
            {
                if (mflObject.Name == mflName)
                {
                    cache.Append(mflObject);
                    return mflObject;
                }
            }
            return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("29C1C584-C4AA-4448-B7A5-2AFFDD076A06")]
    public class TOCElementExplorerObject : ExplorerObjectCls, IExplorerObject
    {
        protected IExplorerObject _parent;
        protected ITOCElement _element;
        protected IExplorerIcon _icon;

        public TOCElementExplorerObject() : base(null, typeof(ITOCElement), 2) { }
        internal TOCElementExplorerObject(IExplorerObject parent, ITOCElement element)
            : base(parent, typeof(ITOCElement), 2)
        {
            _parent = parent;
            _element = element;
            _icon = new MapDocumentExploererIcon(2);
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return _element != null ? _element.Name : "???"; }
        }

        public string FullName
        {
            get
            {
                if (_parent == null) return "";
                return _parent.FullName + @"\" + this.Name;
            }
        }

        public string Type
        {
            get { return "TOC Element"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public new object Object
        {
            get { return _element; }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            try
            {
                if (cache.Contains(FullName)) return cache[FullName];

                FullName = FullName.Replace("/", @"\");
                int lastIndex = FullName.LastIndexOf(@"\");
                if (lastIndex == -1) return null;

                string parentName = FullName.Substring(0, lastIndex);
                string tocName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

                lastIndex = parentName.LastIndexOf(@"\");
                if (lastIndex == -1) return null;
                string mapDocname = parentName.Substring(0, lastIndex);
                FileInfo mapDoc = new FileInfo(mapDocname);
                if (mapDoc.Exists)
                {
                    MapExplorerObject mapObject = new MapExplorerObject();
                    mapObject = mapObject.CreateInstanceByFullName(parentName, cache) as MapExplorerObject;
                    if (mapObject == null || mapObject.ChildObjects == null) return null;

                    foreach (TOCElementExplorerObject tocObject in mapObject.ChildObjects)
                    {
                        if (tocObject.Name == tocName)
                        {
                            cache.Append(tocObject);
                            return tocObject;
                        }
                    }
                }
                else
                {
                    TOCGroupElementExplorerObject parentTocObject = new TOCGroupElementExplorerObject();
                    parentTocObject = parentTocObject.CreateInstanceByFullName(parentName, cache) as TOCGroupElementExplorerObject;
                    if (parentTocObject == null || parentTocObject.ChildObjects == null) return null;

                    foreach (TOCElementExplorerObject tocObject in parentTocObject.ChildObjects)
                    {
                        if (tocObject.Name == tocName)
                        {
                            cache.Append(tocObject);
                            return tocObject;
                        }
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("54AD2D1E-7DC0-4fa4-8CBE-4E01226D4FC2")]
    public class TOCGroupElementExplorerObject : TOCElementExplorerObject, IExplorerParentObject
    {
        public TOCGroupElementExplorerObject() { }
        internal TOCGroupElementExplorerObject(IExplorerObject parent, ITOCElement element)
        {
            _parent = parent;
            _element = element;
            _icon = new MapDocumentExploererIcon(3);
        }

        #region IExplorerParentObject Member

        public void Refresh()
        {
            this.DiposeChildObjects();
            _childs = new List<IExplorerObject>();

            if (_element == null || _element.TOC == null) return;
            if (_element.ElementType == TOCElementType.ClosedGroup)
                _element.OpenCloseGroup(true);

            List<ITOCElement> elements = _element.TOC.GroupedElements(_element);
            foreach (ITOCElement element in _element.TOC.GroupedElements(_element))
            {
                if (element.ParentGroup != _element) continue;
                if (element.ElementType == TOCElementType.ClosedGroup ||
                    element.ElementType == TOCElementType.OpenedGroup)
                {
                    _childs.Add(new TOCGroupElementExplorerObject(this, element));
                }
                else
                {
                    _childs.Add(new TOCElementExplorerObject(this, element));
                }
            }
        }

        List<IExplorerObject> _childs = null;
        public List<IExplorerObject> ChildObjects
        {
            get
            {
                if (_childs == null) this.Refresh();
                return _childs;
            }
        }

        public void DiposeChildObjects()
        {
            if (_childs == null) return;
            foreach (IExplorerObject exObject in _childs)
            {
                if (exObject == null) continue;
                exObject.Dispose();
            }
            _childs = null;
        }

        #endregion
    }

    internal class MapDocumentExploererIcon : IExplorerIcon
    {
        private int _imageIndex = 0;

        public MapDocumentExploererIcon(int imageIndex)
        {
            _imageIndex = imageIndex;
        }

        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                switch (_imageIndex)
                {
                    case 0:
                        return new Guid("8D32C819-3977-4a4e-8AF7-D4CEC5D3482A");
                    case 1:
                        return new Guid("83CCD388-07A7-4a30-90D3-145A500346E8");
                    case 2:
                        return new Guid("CFC73ED0-B10D-4a12-AD3F-68282C7BB7DF");
                    case 3:
                        return new Guid("ABE65EF4-6CFA-4d46-8392-5B4A214C1225");
                }

                return new Guid();
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                switch (_imageIndex)
                {
                    case 0:
                        return global::gView.Plugins.Tools.Properties.Resources.map16;
                    case 1:
                        return global::gView.Plugins.Tools.Properties.Resources.layers;
                    case 2:
                        return global::gView.Plugins.Tools.Properties.Resources.layer;
                    case 3:
                        return global::gView.Plugins.Tools.Properties.Resources.layergroup;
                }
                return null;
            }
        }

        #endregion
    }
}

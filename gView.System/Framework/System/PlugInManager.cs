using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Win32;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.Carto;
using gView.Framework.Symbology;
using gView.MapServer;
using gView.Framework.FDB;
using gView.Framework.IO;
using gView.Framework.GeoProcessing;
using gView.Framework.Network;

namespace gView.Framework.system
{
    public class KnownObjects
    {
        static public Guid Tools_DynamicZoomIn { get { return new Guid("09007afa-b255-4864-ac4f-965df330bfc4"); } }
        static public Guid Tools_DynamicZoomOut { get { return new Guid("51d04e6f-a13e-40b6-bf28-1b8e7c24493d"); } }
        static public Guid Tools_Pan { get { return new Guid("2680f0fd-31ee-48c1-b0f7-6674bad0a688"); } }
        static public Guid Tools_SmartNavigation { get { return new Guid("3e2e9f8c-24fb-48f6-b80e-1b0a54e8c309"); } }
        static public Guid Tools_Identify { get { return new Guid("f13d5923-70c8-4c6b-9372-0760d3a8c08c"); } }
        static public Guid Tools_QueryThemeCombo { get { return new Guid("51A2CF81-E343-4c58-9A42-9207C8DFBC01"); } }
        static public Guid Tools_Zoom2Extent { get { return new Guid("58AE3C1D-40CD-4f61-8C5C-0A955C010CF4"); } }
        static public Guid Tools_TOC { get { return new Guid("0728E12C-AC12-4264-9B47-ECE6BB0CFFA9"); } }
        static public Guid Tools_StaticZoomIn { get { return new Guid("6351BCBE-809A-43cb-81AA-6414ED3FA459"); } }
        static public Guid Tools_StaticZoomOut { get { return new Guid("E1C01E9D-8ADC-477b-BCD1-6B7BBA756D44"); } }


        static public Guid Carto_SimpleRenderer { get { return new Guid("646386e4-d010-4c7d-98aa-8c1903a3d5e4"); } }
        static public Guid Carto_SimpleLabelRenderer { get { return new Guid("92650f7d-cec9-4418-9ea0-a8b09436aa7a"); } }
        static public Guid Carto_ValueMapRenderer { get { return new Guid("c7a92674-0120-4f3d-bc03-f1210136b5c6"); } }
        static public Guid Carto_UniversalGeometryRenderer { get { return new Guid("48EDC5DB-18B6-44cc-8646-461B388F2D94"); } }

        static public Guid Symbology_SimplePointSymbol { get { return new Guid("F73F40DD-BA55-40b1-B372-99F08B66D2D4"); } }
        static public Guid Symbology_SimpleLineSymbol { get { return new Guid("91CC3F6F-0EC5-42b7-AA34-9C89803118E7"); } }
        static public Guid Symbology_SimpleFillSymbol { get { return new Guid("1496A1A8-8087-4eba-86A0-23FB91197B22"); } }
        static public Guid Symbology_HatchSymbol { get { return new Guid("E37D7D86-DF11-410f-ADD1-EA89C1E89605"); } }
        static public Guid Symbology_PolygonMask { get { return new Guid("48177A8B-1B3F-480a-87DF-9F7E1DE57D7B"); } }
        static public Guid Symbology_SimpleTextSymbol { get { return new Guid("A5DA4D8D-879F-41a5-9795-F22BE5B85877"); } }

        static public Guid Persistable_Dictionary { get { return new Guid("5CF9FD50-16B2-4650-A3E3-229E60848CA6"); } }
        static public Guid Persistable_Metadata { get { return new Guid("24373762-127A-4125-B87F-AC0D091D947E"); } }
    }
    /// <summary>
    /// Zusammenfassung f�r AssemblyExplorer.
    /// </summary>

    public class PlugInManager
    {
        //private static string _configPath = System.Environment.CurrentDirectory;
        //private static XmlDocument _doc = null;
        private static Dictionary<Guid, Type> _pluginTypes;

        public PlugInManager()
        {
            //if (PlugInManager.CompFileName == "")
            //{
            //    PlugInManager.CompFileName = (SystemVariables.MyCommonApplicationData + @"\gViewGisOS_plugins.xml").ToPlattformPath();
            //}
            //else
            //{
            //    Init();
            //}
            Init();
        }

        public static void Init()
        {
            //if (_doc != null)
            //    return;
            if (_pluginTypes != null)
                return;

            try
            {
                _pluginTypes = new Dictionary<Guid, Type>();
                FileInfo entryAssembly = new FileInfo(Assembly.GetEntryAssembly().Location);
                foreach(FileInfo dll in entryAssembly.Directory.GetFiles("*.dll"))
                {
                    Assembly assembly = Assembly.LoadFrom(dll.FullName);
                    foreach(var pluginType in assembly.GetTypes())
                    {
                        var registerPluginAttribute = pluginType.GetCustomAttribute<RegisterPlugIn>();
                        if (registerPluginAttribute == null)
                            continue;

                        _pluginTypes.Add(registerPluginAttribute.Value, pluginType);
                    }
                }

                //_doc = new XmlDocument();
                //_doc.Load(PlugInManager.CompFileName);
            }
            catch
            {
                //_doc = null;
                _pluginTypes = null;
            }
        }

        public IEnumerable<Type> GetPluginNodes(Type interfaceType)
        {
            Init();

            List<Type> pluginTypes = new List<Type>();

            foreach(var pluginType in _pluginTypes.Values)
            {
                if (interfaceType.IsAssignableFrom(pluginType))
                    pluginTypes.Add(pluginType);
            }

            return pluginTypes;
        }

		public object CreateInstance(Guid guid) 
		{
            return PlugInManager.Create(guid);
		}
		public object CreateInstance(XmlNode node) 
		{
            return PlugInManager.Create(node);
		}
        public object CreateInstance(object plugin)
        {
            return PlugInManager.Create(plugin);
        }

        public static object Create(XmlNode node)
        {
            try
            {
                if (node == null) return null;
                if (node.Attributes["fullname"] == null ||
                    node.Attributes["assembly"] == null) return null;

                string path = node.Attributes["assembly"].Value;
                if (path.Contains("{APP_PATH}"))
                    path = path.Replace("{APP_PATH}", SystemVariables.ApplicationDirectory);
                if (path.Contains("[APP_PATH]"))
                    path = path.Replace("[APP_PATH]", SystemVariables.ApplicationDirectory);

                Assembly assembly = Assembly.LoadFrom(path);
                return assembly.CreateInstance(node.Attributes["fullname"].Value, false);
            }
            catch
            {
                return null;
            }
        }
        public static object Create(Guid guid)
        {

            try
            {
                Init();

                if(_pluginTypes.ContainsKey(guid))
                {
                    var pluginType = _pluginTypes[guid];
                    return Activator.CreateInstance(pluginType);
                }

                return null;
            }
            catch /*(Exception ex)*/
            {
                return null;
            }

        }
        public static object Create(object plugin)
        {
            if (IsPlugin(plugin))
                return Create(PlugInID(plugin));

            return null;
        }
        //public static object Create(string fullName)
        //{
        //    try
        //    {
        //        Init();

        //        foreach (XmlNode node in _doc.ChildNodes[0].ChildNodes)
        //        {
        //            if (node.Attributes["fullname"] == null) continue;

        //            if (node.Attributes["fullname"].Value == fullName)
        //                return (Create(node));
        //        }

        //        return null;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //public static object Create(XmlNode node, object parameter)
        //{
        //    IPlugInParameter plugin = PlugInManager.Create(node) as IPlugInParameter;
        //    if (plugin != null)
        //        plugin.Parameter = parameter;

        //    return plugin;
        //}
        public static object Create(Guid guid, object parameter)
        {
            IPlugInParameter plugin = PlugInManager.Create(guid) as IPlugInParameter;
            if (plugin != null)
                plugin.Parameter = parameter;

            return plugin;
        }
        public static object Create(object PlugIn, object parameter) 
        {
            IPlugInParameter plugin = PlugInManager.Create(PlugIn) as IPlugInParameter;
            if (plugin != null)
                plugin.Parameter = parameter;

            return plugin;
        }
        //public static object Create(string fullName, object parameter)
        //{
        //    IPlugInParameter plugin = PlugInManager.Create(fullName) as IPlugInParameter;
        //    if (plugin != null)
        //        plugin.Parameter = parameter;

        //    return plugin;
        //}



        static public Guid Guid(XmlNode node)
        {
            try
            {
                if (node == null || node.Attributes["guid"] == null) return new Guid();
                return new Guid(node.Attributes["guid"].Value);
            }
            catch
            {
                return new Guid();
            }
        }
        static public bool IsPlugin(object obj)
        {
            if (obj == null) return false;

            RegisterPlugIn plugin = (RegisterPlugIn)Attribute.GetCustomAttribute(obj.GetType(), typeof(RegisterPlugIn));
            return (plugin != null);
        }

        static public Guid PlugInID(object obj)
        {
            if (obj == null) return new Guid();

            if (obj is IPlugInWrapper)
            {
                return PlugInID(((IPlugInWrapper)obj).WrappedPlugIn);
            }
            else
            {
                RegisterPlugIn plugin = (RegisterPlugIn)Attribute.GetCustomAttribute(obj.GetType(), typeof(RegisterPlugIn));
                if (plugin == null) return new Guid();

                return plugin.Value;
            }
        }

        #region Classes

        public class PlugInType
        {
            public Type Type { get; internal set; }
            public Guid PluginGuid { get; internal set; }

            public PlugInType(object plugin)
            {
                if (plugin == null)
                    throw new ArgumentException();

                Type = plugin.GetType();
                PluginGuid = PlugInManager.PlugInID(plugin);
            }

            public object CreateInstance()
            {
                return PlugInManager.Create(PluginGuid);
            }
        }

        public class PlugInTypeList : List<PlugInType>
        {
            public bool ContainsType(Type type)
            {
                if (type == null)
                    throw new ArgumentException();

                foreach (PlugInType t in this)
                {
                    if (type.Equals(t.Type))
                        return true;
                }

                return false;
            }
        }

        #endregion
    }

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

            return ((ISerializableExplorerObject)obj).CreateInstanceByFullName(FullName,this);
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

    [Serializable]
    public class ExplorerObjectSerialization : IExplorerObjectSerialization
    {
        private Guid _guid;
        private string _fullname;
        private List<Type> _exObjectTypes = new List<Type>();
        private List<Type> _ObjectTypes = new List<Type>();

        public ExplorerObjectSerialization()
        {
        }
        public ExplorerObjectSerialization(IExplorerObject exObject)
        {
            if (!PlugInManager.IsPlugin(exObject)) return;
            _guid = PlugInManager.PlugInID(exObject);
            _fullname = exObject.FullName;

            //foreach (Type basetype in exObject.GetType().GetInterfaces())
            //{
            //    _exObjectTypes.Add(basetype);
            //}
            //if (exObject.Object == null) return;
            //foreach (Type basetype in exObject.Object.GetType().GetInterfaces())
            //{
            //    _ObjectTypes.Add(basetype);
            //}
        }

        #region IExplorerObjectSerialization Member

        public Guid Guid
        {
            get { return _guid; }
        }

        public string FullName
        {
            get { return _fullname; }
        }

        public List<Type> ExplorerObjectTypes
        {
            get { return _exObjectTypes; }
        }

        public List<Type> ObjectTypes
        {
            get { return _ObjectTypes; }
        }

        #endregion
    }

    public class SortByIOrder : IComparer<IOrder>
    {
        #region IComparer<IOrder> Members

        public int Compare(IOrder x, IOrder y)
        {
            if (x.SortOrder < y.SortOrder) return -1;
            if (x.SortOrder > y.SortOrder) return 1;
            return 0;
        }

        #endregion
    }

    public class OrderedPluginList<T> 
       // where T : IOrder
    {
        public static List<T> Sort(IEnumerable<XmlNode> pluginNodes)
        {
            List<T> ret = new List<T>();
            foreach (XmlNode node in pluginNodes)
            {
                try
                {
                    T t = (T)PlugInManager.Create(node);
                    if (t == null) continue;
                    ret.Add(t);
                }
                catch { }
            }
            ret.Sort(new Sorter());
            return ret;
        }

        public static List<T> Sort(IEnumerable<T> plugins)
        {
            List<T> ret = new List<T>(plugins);
            ret.Sort(new Sorter());
            return ret;
        }

        private class Sorter : IComparer<T>
        {
            #region IComparer<IOrder> Members

            public int Compare(T x, T y)
            {
                if (!(x is IOrder) || !(y is IOrder))
                    return 0;

                if (((IOrder)x).SortOrder < ((IOrder)y).SortOrder) return -1;
                if (((IOrder)x).SortOrder > ((IOrder)y).SortOrder) return 1;
                return 0;
            }

            #endregion
        }
    }

    public class Plugins
    {

        public enum Type
        {
            IDataset,
            ITool,
            IExTool,
            IDatasetElementContextMenuItem,
            IMapContextMenuItem,
            IFeatureRenderer,
            ILabelRenderer,
            ISymbol,
            IDockableWindowContainer,
            IExplorerObject,
            IExplorerCommand,
            IExplorerTabPage,
            IServiceRequestInterpreter,
            IMapOptionPage,
            IExplorerOptionPage,
            ILayerPropertyPage,
            IGraphicElement2,
            IAutoField,
            IFieldDomain,
            IServiceableDataset,
            IFileFeatureDatabase,
            IMetadataProvider,
            IPersistable,
            IMapApplicationModule,
            IActivity,
            ISimpleNumberCalculation,
            INetworkTracer,
            IFeatureLayerJoin,
            IFeatureDatabase,
            ICartoRibbonTab,
            IExplorerRibbonTab
        };

        public static string TypeName(Plugins.Type type) 
        {
            switch (type)
            {
                case Type.IDataset:
                    return typeof(IDataset).ToString(); 
                case Type.ITool:
                    return typeof(ITool).ToString(); 
                case Type.IExTool:
                    return typeof(IExTool).ToString();
                case Type.IDatasetElementContextMenuItem:
                    return typeof(IDatasetElementContextMenuItem).ToString();
                case Type.IMapContextMenuItem:
                    return typeof(IMapContextMenuItem).ToString();
                case Type.IFeatureRenderer:
                    return typeof(IFeatureRenderer).ToString();
                case Type.ILabelRenderer:
                    return typeof(ILabelRenderer).ToString();
                case Type.ISymbol:
                    return typeof(ISymbol).ToString();
                case Type.IDockableWindowContainer:
                    return typeof(IDockableWindowContainer).ToString();
                case Type.IExplorerObject:
                    return typeof(IExplorerObject).ToString();
                case Type.IExplorerCommand:
                    return typeof(IExplorerCommand).ToString();
                case Type.IExplorerTabPage:
                    return "gView.Framework.UI.IExplorerTabPage";
                case Type.IServiceRequestInterpreter:
                    return typeof(IServiceRequestInterpreter).ToString();
                case Type.IMapOptionPage:
                    return "gView.Framework.UI.IMapOptionPage";
                case Type.IExplorerOptionPage:
                    return "gView.Framework.UI.IExplorerOptionPage";
                case Type.ILayerPropertyPage:
                    return "gView.Framework.UI.ILayerPropertyPage";
                case Type.IGraphicElement2:
                    return typeof(IGraphicElement2).ToString();
                case Type.IAutoField:
                    return typeof(IAutoField).ToString();
                case Type.IFieldDomain:
                    return typeof(IFieldDomain).ToString();
                case Type.IServiceableDataset:
                    return typeof(IServiceableDataset).ToString();
                case Type.IFileFeatureDatabase:
                    return typeof(IFileFeatureDatabase).ToString();
                case Type.IMetadataProvider:
                    return typeof(IMetadataProvider).ToString();
                case Type.IPersistable:
                    return typeof(IPersistable).ToString();
                case Type.IMapApplicationModule:
                    return typeof(IMapApplicationModule).ToString();
                case Type.IActivity:
                    return typeof(IActivity).ToString();
                case Type.ISimpleNumberCalculation:
                    return typeof(ISimpleNumberCalculation).ToString();
                case Type.INetworkTracer:
                    return typeof(INetworkTracer).ToString();
                case Type.IFeatureLayerJoin:
                    return typeof(IFeatureLayerJoin).ToString();
                case Type.IFeatureDatabase:
                    return typeof(IFeatureDatabase).ToString();
                case Type.ICartoRibbonTab:
                    return "gView.Framework.UI.ICartoRibbonTab";
                case Type.IExplorerRibbonTab:
                    return "gView.Framework.UI.IExplorerRibbonTab";
            }
            return String.Empty;
        }
    }
}

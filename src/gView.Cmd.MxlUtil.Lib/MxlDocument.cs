using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using gView.Framework.Core.UI;
using gView.Framework.Data.Relations;
using gView.Framework.IO;
using gView.GraphicsEngine;

namespace gView.Cmd.MxlUtil.Lib
{
    public class MxlDocument : IPersistableLoadAsync
    {
        List<IMap> _maps;
        private int _focusMapIndex = -1;
        private ITableRelations _tableRelations;

        public event MapAddedEvent? MapAdded;
        public event MapDeletedEvent? MapDeleted;
        public event MapScaleChangedEvent? MapScaleChanged;
        public event AfterSetFocusMapEvent? AfterSetFocusMap;

        public MxlDocument()
        {
            _maps = new List<IMap>();
            _tableRelations = new TableRelations(null);

            if (Current.Engine == null)
            {
                SystemInfo.RegisterDefaultGraphicEnginges();
            }
        }

        public bool LoadMapDocument(string path)
        {
            XmlStream stream = new XmlStream("");
            if (stream.ReadStream(path))
            {
                while (_maps.Count > 0)
                {
                    this.RemoveMap(_maps[0]);
                }

                stream.Load("MapDocument", null, this);

                if (_maps.Count > 0)
                {
                    FocusMap = _maps[0];
                }
                else
                {
                    FocusMap = null;
                }

                return true;
            }
            return false;
        }

        public IMap? GetMap(string name)
        {
            if (_maps == null)
            {
                return null;
            }

            foreach (IMap map in _maps)
            {
                if (map.Name == name)
                {
                    return map;
                }
            }
            return null;
        }

        public bool AddMap(IMap map)
        {
            if (_maps.Contains(map))
            {
                return true;
            }

            int c = 1;
            string alias = map.Name, alias2 = alias;
            while (GetMap(alias2) != null)
            {
                alias2 = alias + "_" + c.ToString();
                c++;
            }
            map.Name = alias2;

            _maps.Add(map);

            if (map.Display != null)
            {
                map.Display.MapScaleChanged += new MapScaleChangedEvent(Display_MapScaleChanged);
            }

            if (MapAdded != null)
            {
                MapAdded(map);
            }

            return true;
        }

        public bool RemoveMap(IMap map)
        {
            if (_maps.Contains(map))
            {
                _maps.Remove(map);
                if (MapDeleted != null)
                {
                    MapDeleted(map);
                }
            }
            return true;
        }

        private void Display_MapScaleChanged(IDisplay sender)
        {
            if (MapScaleChanged != null)
            {
                MapScaleChanged(sender);
            }
        }

        #region IMapDocument Member

        public IEnumerable<IMap> Maps
        {
            get
            {
                List<IMap> e = new List<IMap>();
                foreach (IMap map in _maps)
                {
                    e.Add(map);
                }
                return e;
            }
        }

        public IMap? FocusMap
        {
            get
            {
                if (_focusMapIndex < 0 || _focusMapIndex >= _maps.Count)
                {
                    return null;
                }

                return _maps[_focusMapIndex];
            }
            set
            {
                if (value == null)
                {
                    _focusMapIndex = -1;
                    return;
                }
                if (!_maps.Contains(value))
                {
                    this.AddMap(value);
                }

                _focusMapIndex = _maps.IndexOf(value);

                if (AfterSetFocusMap != null)
                {
                    AfterSetFocusMap(this.FocusMap);
                }
            }
        }

        public IMap? this[string name]
        {
            get { return GetMap(name); }
        }

        public IMap? this[IDatasetElement layer]
        {
            get
            {
                foreach (IMap map in _maps)
                {
                    if (layer is IGroupLayer)
                    {
                        foreach (IDatasetElement element in map.MapElements)
                        {
                            if (layer == element)
                            {
                                return map;
                            }
                        }
                    }
                    else
                    {
                        if (map[layer] != null)
                        {
                            return map;
                        }
                    }
                }
                return null;
            }
        }

        public ITableRelations TableRelations
        {
            get { return _tableRelations; }
        }

        #endregion

        #region IPersistable Member

        public string PersistID
        {
            get
            {
                return "";
            }
        }

        async public Task<bool> LoadAsync(IPersistStream stream)
        {
            while (_maps.Count > 0)
            {
                this.RemoveMap(_maps[0]);
            }

            _focusMapIndex = (int)stream.Load("focusMapIndex", 0);

            IMap map;
            while ((map = (await stream.LoadAsync<IMap>("IMap", new MapPersist()))) != null)
            {
                this.AddMap(map);
            }

            if (_maps.Count > 0)
            {
                FocusMap = _maps[0];
            }
            else
            {
                FocusMap = null;
            }

            //if (this.Application is IGUIApplication)
            //{
            //    stream.Load("Moduls", null, new ModulsPersists(this.Application as IMapApplication));
            //    stream.Load("Tools", null, new ToolsPersist(this.Application as IGUIApplication));
            //    stream.Load("Toolbars", null, new ToolbarsPersist(this.Application as IGUIApplication));
            //}
            _tableRelations = (TableRelations)stream.Load("TableRelations", new TableRelations(null), new TableRelations(null));

            return true;
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("focusMapIndex", _focusMapIndex);

            foreach (IMap map in _maps)
            {
                stream.Save("IMap", map);
            }

            //if (this.Application is IGUIApplication)
            //{
            //    stream.Save("Moduls", new ModulsPersists(this.Application as IMapApplication));
            //    stream.Save("Tools", new ToolsPersist(this.Application as IGUIApplication));
            //    stream.Save("Toolbars", new ToolbarsPersist(this.Application as IGUIApplication));
            //}
            if (_tableRelations != null)
            {
                stream.Save("TableRelations", _tableRelations);
            }
        }

        #endregion
    }

    //internal class ToolsPersist : IPersistable
    //{
    //    private IGUIApplication _application;

    //    public ToolsPersist(IGUIApplication application)
    //    {
    //        _application = application;
    //    }

    //    #region IPersistable Member

    //    public void Load(IPersistStream stream)
    //    {
    //        if (_application == null)
    //        {
    //            return;
    //        }

    //        while (true)
    //        {
    //            ToolPersist toolper = stream.Load("Tool", null, new ToolPersist(_application)) as ToolPersist;
    //            if (toolper == null)
    //            {
    //                break;
    //            }
    //        }
    //    }

    //    public void Save(IPersistStream stream)
    //    {
    //        if (_application == null)
    //        {
    //            return;
    //        }

    //        foreach (ITool tool in _application.Tools)
    //        {
    //            if (!(tool is IPersistable))
    //            {
    //                continue;
    //            }

    //            stream.Save("Tool", new ToolPersist(tool));
    //        }
    //    }

    //    #endregion
    //}

    //internal class ToolPersist : IPersistable
    //{
    //    private ITool _tool = null;
    //    private IGUIApplication _app = null;

    //    public ToolPersist(ITool tool)
    //    {
    //        _tool = tool;
    //    }
    //    public ToolPersist(IGUIApplication app)
    //    {
    //        _app = app;
    //    }
    //    #region IPersistable Member

    //    public void Load(IPersistStream stream)
    //    {
    //        if (_app == null)
    //        {
    //            return;
    //        }

    //        try
    //        {
    //            Guid guid = new Guid(stream.Load("GUID") as string);
    //            ITool tool = _app.Tool(guid);

    //            if (!(tool is IPersistable))
    //            {
    //                return;
    //            } ((IPersistable)tool).Load(stream);
    //        }
    //        catch { }
    //    }

    //    public void Save(IPersistStream stream)
    //    {
    //        if (_tool == null || !(_tool is IPersistable))
    //        {
    //            return;
    //        }

    //        stream.Save("GUID", PlugInManager.PlugInID(_tool).ToString());
    //        ((IPersistable)_tool).Save(stream);
    //    }

    //    #endregion
    //}

    //internal class ToolbarsPersist : IPersistable
    //{
    //    private IGUIApplication _application;

    //    public ToolbarsPersist(IGUIApplication application)
    //    {
    //        _application = application;
    //    }

    //    #region IPersistable Member

    //    public void Load(IPersistStream stream)
    //    {
    //        if (_application == null)
    //        {
    //            return;
    //        }

    //        while (true)
    //        {
    //            ToolbarPersist toolbarper = stream.Load("Toolbar", null, new ToolbarPersist(_application)) as ToolbarPersist;
    //            if (toolbarper == null)
    //            {
    //                break;
    //            }
    //        }

    //        return;
    //    }

    //    public void Save(IPersistStream stream)
    //    {
    //        if (_application == null)
    //        {
    //            return;
    //        }

    //        List<IToolbar> toolbars = _application.Toolbars;
    //        toolbars.Sort(new ToolbarPositionComparer());

    //        foreach (IToolbar toolbar in toolbars)
    //        {
    //            if (!(toolbar is IPersistable))
    //            {
    //                continue;
    //            }

    //            stream.Save("Toolbar", new ToolbarPersist(toolbar));
    //        }

    //        return;
    //    }

    //    #endregion

    //    private class ToolbarPositionComparer : IComparer<IToolbar>
    //    {
    //        #region IComparer<IToolbar> Member

    //        public int Compare(IToolbar x, IToolbar y)
    //        {
    //            ToolbarStrip strip1 = x as ToolbarStrip;
    //            ToolbarStrip strip2 = y as ToolbarStrip;

    //            if (strip1 == null || strip2 == null)
    //            {
    //                return 0;
    //            }

    //            if (strip1.Location.Y < strip2.Location.Y)
    //            {
    //                return -1;
    //            }
    //            else if (strip1.Location.Y == strip2.Location.Y)
    //            {
    //                if (strip1.Location.X < strip2.Location.X)
    //                {
    //                    return -1;
    //                }
    //                else if (strip1.Location.X > strip2.Location.X)
    //                {
    //                    return 1;
    //                }

    //                return 0;
    //            }
    //            else
    //            {
    //                return 1;
    //            }
    //        }

    //        #endregion
    //    }
    //}

    //internal class ToolbarPersist : IPersistable
    //{
    //    private IToolbar _toolbar = null;
    //    private IGUIApplication _app = null;

    //    public ToolbarPersist(IToolbar toolbar)
    //    {
    //        _toolbar = toolbar;
    //    }
    //    public ToolbarPersist(IGUIApplication app)
    //    {
    //        _app = app;
    //    }

    //    #region IPersistable Member

    //    public void Load(IPersistStream stream)
    //    {
    //        if (_app == null)
    //        {
    //            return;
    //        }

    //        try
    //        {
    //            Guid guid = new Guid(stream.Load("GUID") as string);
    //            IToolbar toolbar = _app.Toolbar(guid);

    //            if (!(toolbar is IPersistable))
    //            {
    //                return;
    //            } ((IPersistable)toolbar).Load(stream);
    //        }
    //        catch { }
    //    }

    //    public void Save(IPersistStream stream)
    //    {
    //        if (_toolbar == null || !(_toolbar is IPersistable))
    //        {
    //            return;
    //        }

    //        stream.Save("GUID", PlugInManager.PlugInID(_toolbar).ToString());
    //        ((IPersistable)_toolbar).Save(stream);
    //    }

    //    #endregion
    //}

    //internal class ModulsPersists : IPersistable
    //{
    //    private IMapApplication _application;

    //    public ModulsPersists(IMapApplication application)
    //    {
    //        _application = application;
    //    }

    //    #region IPersistable Member

    //    public void Load(IPersistStream stream)
    //    {
    //        if (_application == null)
    //        {
    //            return;
    //        }

    //        if (_application == null)
    //        {
    //            return;
    //        }

    //        while (true)
    //        {
    //            ModulePersist moduleper = stream.Load("Module", null, new ModulePersist(_application)) as ModulePersist;
    //            if (moduleper == null)
    //            {
    //                break;
    //            }
    //        }
    //    }

    //    public void Save(IPersistStream stream)
    //    {
    //        if (_application == null)
    //        {
    //            return;
    //        }

    //        PlugInManager compMan = new PlugInManager();
    //        foreach (Type compType in compMan.GetPlugins(Plugins.Type.IMapApplicationModule))
    //        {
    //            IMapApplicationModule module = _application.IMapApplicationModule(PlugInManager.PluginIDFromType(compType));
    //            if (module is IPersistable)
    //            {
    //                stream.Save("Module", new ModulePersist(module));
    //            }
    //        }

    //    }

    //    #endregion
    //}

    //internal class ModulePersist : IPersistable
    //{
    //    private IMapApplicationModule _module = null;
    //    private IMapApplication _app = null;

    //    public ModulePersist(IMapApplicationModule module)
    //    {
    //        _module = module;
    //    }
    //    public ModulePersist(IMapApplication app)
    //    {
    //        _app = app;
    //    }

    //    #region IPersistable Member

    //    public void Load(IPersistStream stream)
    //    {
    //        if (_app == null)
    //        {
    //            return;
    //        }

    //        try
    //        {
    //            Guid guid = new Guid(stream.Load("GUID") as string);
    //            IMapApplicationModule module = _app.IMapApplicationModule(guid);

    //            if (!(module is IPersistable))
    //            {
    //                return;
    //            } ((IPersistable)module).Load(stream);
    //        }
    //        catch { }
    //    }

    //    public void Save(IPersistStream stream)
    //    {
    //        if (_module == null || !(_module is IPersistable))
    //        {
    //            return;
    //        }

    //        stream.Save("GUID", PlugInManager.PlugInID(_module).ToString());
    //        ((IPersistable)_module).Save(stream);
    //    }

    //    #endregion
    //}
}

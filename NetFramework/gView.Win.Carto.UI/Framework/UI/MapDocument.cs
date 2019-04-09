using System;
using System.Collections;
using System.Collections.Generic;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.IO;
using gView.Framework.system;
using System.Xml;
using gView.Framework.UI.Controls;
using gView.Framework.Data.Relations;
using System.Threading.Tasks;

namespace gView.Framework.UI
{
	public class MapDocument : IMapDocument,IPersistable
	{
		List<IMap> _maps;
		private int _focusMapIndex=-1;
		private IDocumentWindow _docWindow;
        private IMapApplication _application;
        private ITableRelations _tableRelations;

		public event LayerAddedEvent LayerAdded;
        public event LayerRemovedEvent LayerRemoved;
		public event MapAddedEvent MapAdded;
		public event MapDeletedEvent MapDeleted;
        public event MapScaleChangedEvent MapScaleChanged;
        public event AfterSetFocusMapEvent AfterSetFocusMap;

		public MapDocument() 
		{
			_maps=new List<IMap>();
            _tableRelations = new TableRelations(this);
		}

        public MapDocument(IMapApplication application) : this()
        {
            _application = application;
        }

		public bool LoadMapDocument(string path) 
		{
			XmlStream stream=new XmlStream("");
			if(stream.ReadStream(path)) 
			{
				while(_maps.Count>0)
				{
					this.RemoveMap((IMap)_maps[0]);
				}

				stream.Load("MapDocument",null,this);

                if (_maps.Count > 0)
                    FocusMap = _maps[0];
                else
                    FocusMap = null;

				return true;
			}
			return false;
		}

        public IMap GetMap(string name)
        {
            if (_maps == null) return null;

            foreach (IMap map in _maps)
            {
                if (map.Name == name) return map;
            }
            return null;
        }

        public bool AddMap(IMap map)
        {
            if (_maps.Contains(map)) return true;

            int c = 1;
            string alias = map.Name, alias2 = alias;
            while (GetMap(alias2) != null)
            {
                alias2 = alias + "_" + c.ToString();
                c++;
            }
            map.Name = alias2;

            _maps.Add(map);

            map.LayerAdded += new gView.Framework.Carto.LayerAddedEvent(map_LayerAdded);
            map.LayerRemoved += new LayerRemovedEvent(map_LayerRemoved);
            if (map.Display != null)
            {
                map.Display.MapScaleChanged += new gView.Framework.Carto.MapScaleChangedEvent(Display_MapScaleChanged);
            }

            if (MapAdded != null)
                MapAdded(map);

            return true;
        }

		public bool RemoveMap(IMap map) 
		{
			if(_maps.Contains(map)) 
			{
				_maps.Remove(map);
				if(MapDeleted!=null) MapDeleted(map);
			}
            return true;
		}

		private void map_LayerAdded(IMap sender,ILayer dataset) 
		{
			if(LayerAdded!=null) 
				LayerAdded(sender,dataset);
		}

        void map_LayerRemoved(IMap sender, ILayer layer)
        {
            if (LayerRemoved != null)
                LayerRemoved(sender, layer);
        }

        private void Display_MapScaleChanged(IDisplay sender)
        {
            if (MapScaleChanged != null) MapScaleChanged(sender);
        }

		#region IMapDocument Member

		public IEnumerable<IMap> Maps
		{
			get
			{
				List<IMap> e=new List<IMap>();
				foreach(IMap map in _maps) 
				{
					e.Add(map);
				}
				return e;
			}
		}

		public IMap FocusMap
		{
			get
			{
				if(_focusMapIndex<0 || _focusMapIndex>=_maps.Count) return null;
				return (IMap)_maps[_focusMapIndex];
			}
			set 
			{
                if (value == null)
                {
                    _focusMapIndex = -1;
                    return;
                }
				if(!_maps.Contains(value)) 
				{
					this.AddMap(value);
				}
                
                foreach (IMap map in _maps)
                {
                    if (!(map is Map)) continue;
                    ((Map)map).DisposeGraphicsAndImage();
                }
                
				_focusMapIndex=_maps.IndexOf(value);

                if (AfterSetFocusMap != null) AfterSetFocusMap(this.FocusMap);
			}
		}

        public IDocumentWindow DocumentWindow
        {
            get { return _docWindow; }
            set { _docWindow = value; }
        }

        public IMap this[string name]
        {
            get { return GetMap(name); }
        }

		public IMap this[IDatasetElement layer] 
		{
			get 
			{
				foreach(IMap map in _maps) 
				{
                    if (layer is IGroupLayer)
                    {
                        foreach (IDatasetElement element in map.MapElements)
                        {
                            if (layer == element) return map;
                        }
                    }
                    else
                    {
                        if (map[layer] != null) return map;
                    }
				}
				return null;
			}
		}

        public IApplication Application
        {
            get { return _application; }
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

		public Task<bool> Load(IPersistStream stream)
		{
            while (_maps.Count > 0)
            {
                this.RemoveMap((IMap)_maps[0]);
            }

			_focusMapIndex=(int)stream.Load("focusMapIndex",(int)0);

			IMap map;
			while((map=(IMap)stream.Load("IMap",null,new gView.Framework.Carto.Map()))!=null)
			{
				this.AddMap(map);
			}

            if (_maps.Count > 0)
                FocusMap = _maps[0];
            else
                FocusMap = null;

            if (this.Application is IGUIApplication)
            {
                stream.Load("Moduls", null, new ModulsPersists(this.Application as IMapApplication));
                stream.Load("Tools", null, new ToolsPersist(this.Application as IGUIApplication));
                stream.Load("Toolbars", null, new ToolbarsPersist(this.Application as IGUIApplication));
            }
            _tableRelations = (TableRelations)stream.Load("TableRelations", new TableRelations(this), new TableRelations(this));

            return Task.FromResult(true);
		}

		public Task<bool> Save(IPersistStream stream)
		{
			stream.Save("focusMapIndex",_focusMapIndex);

			foreach(IMap map in _maps) 
			{
				stream.Save("IMap",map);
			}

            if (this.Application is IGUIApplication)
            {
                stream.Save("Moduls", new ModulsPersists(this.Application as IMapApplication));
                stream.Save("Tools", new ToolsPersist(this.Application as IGUIApplication));
                stream.Save("Toolbars", new ToolbarsPersist(this.Application as IGUIApplication));
            }
            if (_tableRelations != null)
                stream.Save("TableRelations", _tableRelations);

            return Task.FromResult(true);
		}

		#endregion
    }

    internal class ToolsPersist : IPersistable
    {
        private IGUIApplication _application;

        public ToolsPersist(IGUIApplication application)
        {
            _application = application;
        }

        #region IPersistable Member

        public Task<bool> Load(IPersistStream stream)
        {
            if (_application == null)
                return Task.FromResult(true);

            while (true)
            {
                ToolPersist toolper = stream.Load("Tool", null, new ToolPersist(_application)) as ToolPersist;
                if (toolper == null) break;
            }

            return Task.FromResult(true);
        }

        public Task<bool> Save(IPersistStream stream)
        {
            if (_application == null)
                return Task.FromResult(true);

            foreach (ITool tool in _application.Tools)
            {
                if (!(tool is IPersistable)) continue;

                stream.Save("Tool", new ToolPersist(tool));
            }

            return Task.FromResult(true);
        }

        #endregion
    }

    internal class ToolPersist : IPersistable
    {
        private ITool _tool = null;
        private IGUIApplication _app = null;

        public ToolPersist(ITool tool)
        {
            _tool = tool;
        }
        public ToolPersist(IGUIApplication app)
        {
            _app = app;
        }
        #region IPersistable Member

        async public Task<bool> Load(IPersistStream stream)
        {
            if (_app == null)
                return true;

            try
            {
                Guid guid = new Guid(stream.Load("GUID") as string);
                ITool tool = _app.Tool(guid);

                if (!(tool is IPersistable))
                    return true;
                await ((IPersistable)tool).Load(stream);
            }
            catch { }

            return true;
        }

        async public Task<bool> Save(IPersistStream stream)
        {
            if (_tool == null || !(_tool is IPersistable))
                return true;

            stream.Save("GUID", PlugInManager.PlugInID(_tool).ToString());
            await ((IPersistable)_tool).Save(stream);

            return true;
        }

        #endregion
    }

    internal class ToolbarsPersist : IPersistable
    {
        private IGUIApplication _application;

        public ToolbarsPersist(IGUIApplication application)
        {
            _application = application;
        }

        #region IPersistable Member

        public Task<bool> Load(IPersistStream stream)
        {
            if (_application == null)
                return Task.FromResult(true);

            while (true)
            {
                ToolbarPersist toolbarper = stream.Load("Toolbar", null, new ToolbarPersist(_application)) as ToolbarPersist;
                if (toolbarper == null) break;
            }

            return Task.FromResult(true);
        }

        public Task<bool> Save(IPersistStream stream)
        {
            if (_application == null)
                return Task.FromResult(true);

            List<IToolbar> toolbars=_application.Toolbars;
            toolbars.Sort(new ToolbarPositionComparer());

            foreach (IToolbar toolbar in toolbars)
            {
                if (!(toolbar is IPersistable)) continue;

                stream.Save("Toolbar", new ToolbarPersist(toolbar));
            }

            return Task.FromResult(true);
        }

        #endregion

        private class ToolbarPositionComparer : IComparer<IToolbar>
        {
            #region IComparer<IToolbar> Member

            public int Compare(IToolbar x, IToolbar y)
            {
                ToolbarStrip strip1 = x as ToolbarStrip;
                ToolbarStrip strip2 = y as ToolbarStrip;

                if (strip1 == null || strip2 == null) return 0;

                if (strip1.Location.Y < strip2.Location.Y)
                {
                    return -1;
                }
                else if (strip1.Location.Y == strip2.Location.Y)
                {
                    if (strip1.Location.X < strip2.Location.X)
                        return -1;
                    else if (strip1.Location.X > strip2.Location.X)
                        return 1;
                    return 0;
                }
                else
                {
                    return 1;
                }
            }

            #endregion
        }
    }

    internal class ToolbarPersist : IPersistable
    {
        private IToolbar _toolbar = null;
        private IGUIApplication _app = null;

        public ToolbarPersist(IToolbar toolbar)
        {
            _toolbar = toolbar;
        }
        public ToolbarPersist(IGUIApplication app)
        {
            _app = app;
        }

        #region IPersistable Member

        async public Task<bool> Load(IPersistStream stream)
        {
            if (_app == null)
                return true;

            try
            {
                Guid guid = new Guid(stream.Load("GUID") as string);
                IToolbar toolbar = _app.Toolbar(guid);

                if (!(toolbar is IPersistable))
                    return true;
                await ((IPersistable)toolbar).Load(stream);
            }
            catch { }

            return true;
        }

        async public Task<bool> Save(IPersistStream stream)
        {
            if (_toolbar == null || !(_toolbar is IPersistable))
                return true;

            stream.Save("GUID", PlugInManager.PlugInID(_toolbar).ToString());
            await ((IPersistable)_toolbar).Save(stream);

            return true;
        }

        #endregion
    }

    internal class ModulsPersists : IPersistable
    {
        private IMapApplication _application;

        public ModulsPersists(IMapApplication application)
        {
            _application = application;
        }

        #region IPersistable Member

        public Task<bool> Load(IPersistStream stream)
        {
            if (_application == null)
                return Task.FromResult(true);

            if (_application == null)
                return Task.FromResult(true);

            while (true)
            {
                ModulePersist moduleper = stream.Load("Module", null, new ModulePersist(_application)) as ModulePersist;
                if (moduleper == null) break;
            }

            return Task.FromResult(true);
        }

        public Task<bool> Save(IPersistStream stream)
        {
            if (_application == null)
                return Task.FromResult(true);

            PlugInManager compMan=new PlugInManager();
            foreach (Type compType in compMan.GetPlugins(Plugins.Type.IMapApplicationModule))
            {
                IMapApplicationModule module = _application.IMapApplicationModule(PlugInManager.PluginIDFromType(compType));
                if (module is IPersistable)
                {
                    stream.Save("Module", new ModulePersist(module));
                }
            }

            return Task.FromResult(true);
        }

        #endregion
    }

    internal class ModulePersist : IPersistable
    {
        private IMapApplicationModule _module = null;
        private IMapApplication _app = null;

        public ModulePersist(IMapApplicationModule module)
        {
            _module = module;
        }
        public ModulePersist(IMapApplication app)
        {
            _app = app;
        }

        #region IPersistable Member

        async public Task<bool> Load(IPersistStream stream)
        {
            if (_app == null)
                return true;

            try
            {
                Guid guid = new Guid(stream.Load("GUID") as string);
                IMapApplicationModule module = _app.IMapApplicationModule(guid);

                if (!(module is IPersistable))
                    return true;
                await ((IPersistable)module).Load(stream);
            }
            catch { }

            return true;
        }

        async public Task<bool> Save(IPersistStream stream)
        {
            if (_module == null || !(_module is IPersistable))
                return true;

            stream.Save("GUID", PlugInManager.PlugInID(_module).ToString());
            await ((IPersistable)_module).Save(stream);

            return true;
        }

        #endregion
    }
}

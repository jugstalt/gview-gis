using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Data.Relations;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Server.Services.MapServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class ServerMapDocument : IMapDocument, IMapDocumentModules, IPersistableLoadAsync
    {
        private ConcurrentBag<IMap> _maps = new ConcurrentBag<IMap>();
        private ConcurrentDictionary<IMap, IEnumerable<IMapApplicationModule>> _mapModules = new ConcurrentDictionary<IMap, IEnumerable<IMapApplicationModule>>();
        private ITableRelations _tableRelations;
        private MapServiceManager _mapServerService;
             
        public ServerMapDocument(MapServiceManager mapServerService)
        {
            _mapServerService = mapServerService;
            _tableRelations = new TableRelations(this);
        }

        #region IMapDocument Member

        public event LayerAddedEvent LayerAdded;
        public event LayerRemovedEvent LayerRemoved;
        public event MapAddedEvent MapAdded;
        public event MapDeletedEvent MapDeleted;
        public event MapScaleChangedEvent MapScaleChanged;
        public event AfterSetFocusMapEvent AfterSetFocusMap;

        public IEnumerable<IMap> Maps
        {
            get { return _maps.ToArray(); }
        }

        public IMap FocusMap
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public bool AddMap(IMap map)
        {
            if (_mapServerService.Instance == null)
            {
                return false;
            }

            if (map == null || _maps.Contains(map))
            {
                return true;
            }

            _maps.Add(map);
            if (MapAdded != null)
            {
                MapAdded(map);
            }

            return true;
        }

        public bool RemoveMap(IMap map)
        {
            if (map == null || !_maps.Contains(map))
            {
                return false;
            }

            _maps = new ConcurrentBag<IMap>(_maps.Except(new[] { map }));

            if (MapDeleted != null)
            {
                MapDeleted(map);
            }

            return true;
        }

        public bool RemoveMap(string mapName)
        {
            var map = this[mapName];
            if (map != null)
            {
                return RemoveMap(map);
            }

            return true;
        }

        public IMap this[string mapName]
        {
            get
            {
                foreach (IMap map in _maps)
                {
                    if (map.Name == mapName)
                    {
                        return map;
                    }
                }
                return null;
            }
        }

        public IMap this[gView.Framework.Data.IDatasetElement layer]
        {
            get
            {
                foreach (IMap map in _maps)
                {
                    foreach (IDatasetElement element in map.MapElements)
                    {
                        if (element == layer)
                        {
                            return map;
                        }
                    }
                }
                return null;
            }
        }

        public IApplication Application
        {
            get { return null; }
        }

        public ITableRelations TableRelations
        {
            get { return _tableRelations; }
        }

        #endregion

        async public Task<bool> LoadMapDocumentAsync(string path)
        {
            XmlStream stream = new XmlStream("");
            if (stream.ReadStream(path))
            {
                while (_maps.Count > 0)
                {
                    this.RemoveMap((IMap)_maps.First());
                }

                return await stream.LoadAsync("MapDocument", this) != null;
            }
            return false;
        }

        #region IPersistableLoadAsync Member

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
                this.RemoveMap((IMap)_maps.First());
            }

            IMap map;
            while ((map = (await stream.LoadAsync<IMap>("IMap", new gView.Framework.Carto.Map()))) != null)
            {
                this.AddMap(map);

                var modules = new ModulesPersists(map);
                stream.Load("Moduls", null, modules);
                _mapModules.TryAdd(map, modules.Modules);
            }

            return true;
        }

        public void Save(IPersistStream stream)
        {
            foreach (IMap map in _maps)
            {
                stream.Save("IMap", map);

                if (_mapModules.ContainsKey(map))
                {
                    var modules = _mapModules.ContainsKey(map);
                    var modulePersits = new ModulesPersists(map);

                    stream.Save("Moduls", new ModulesPersists(map) { Modules = _mapModules[map] });
                }
            }
        }

        #endregion

        #region IMapDocumentModules

        public IEnumerable<IMapApplicationModule> GetMapModules(IMap map)
        {
            if (_mapModules.ContainsKey(map))
            {
                return _mapModules[map];
            }

            return new IMapApplicationModule[0];
        }

        public void SetMapModules(IMap map, IEnumerable<IMapApplicationModule> modules)
        {
            _mapModules.TryRemove(map, out IEnumerable<IMapApplicationModule> removed);
            _mapModules.TryAdd(map, modules);
        }

        #endregion
    }

    internal class ModulesPersists : IPersistable
    {
        private IMap _map;
        private List<IMapApplicationModule> _modules = new List<IMapApplicationModule>();

        public ModulesPersists(IMap map)
        {
            _map = map;
        }

        public IEnumerable<IMapApplicationModule> Modules
        {
            get { return _modules; }
            set { _modules = value != null ? new List<IMapApplicationModule>(value) : new List<IMapApplicationModule>(); }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_map == null)
            {
                return;
            }

            while (true)
            {
                ModulePersist module = stream.Load("Module", null, new ModulePersist()) as ModulePersist;
                if (module == null)
                {
                    break;
                }

                if (module.Module != null)
                {
                    module.Module.OnCreate(_map);
                    _modules.Add(module.Module);
                }
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_map == null)
            {
                return;
            }

            PlugInManager pluginManager = new PlugInManager();
            foreach (IMapApplicationModule module in _modules)
            {
                if (module is IPersistable)
                {
                    stream.Save("Module", new ModulePersist(module));
                }
            }
        }

        #endregion
    }

    internal class ModulePersist : IPersistable
    {
        private IMapApplicationModule _module = null;

        public ModulePersist(IMapApplicationModule module)
        {
            _module = module;
        }
        public ModulePersist()
        {
        }

        public IMapApplicationModule Module => _module;

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            try
            {
                Guid guid = new Guid(stream.Load("GUID") as string);
                _module = (IMapApplicationModule)PlugInManager.Create(guid);

                if (!(_module is IPersistable))
                {
                    return;
                } ((IPersistable)_module).Load(stream);
            }
            catch { }
        }

        public void Save(IPersistStream stream)
        {
            if (_module == null || !(_module is IPersistable))
            {
                return;
            }

            stream.Save("GUID", PlugInManager.PlugInID(_module).ToString());
            ((IPersistable)_module).Save(stream);
        }

        #endregion
    }
}

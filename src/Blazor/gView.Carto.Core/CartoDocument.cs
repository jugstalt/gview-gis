using gView.Carto.Core.Abstraction;
using gView.Framework.Cartography;
using System.Threading.Tasks;
using gView.Framework.Data.Relations;
using gView.Framework.Core.Data;
using gView.Framework.Core.Carto;
using gView.Framework.Core.IO;
using gView.Framework.Common;
using gView.Framework.Core.Common;
using System;
using gView.Carto.Core.Services.Abstraction;
using System.Collections.Generic;
using System.Linq;
using gView.Blazor.Core.Services;
using static System.Formats.Asn1.AsnWriter;

namespace gView.Carto.Core;

public class CartoDocument : ICartoDocument
{
    private readonly ICartoApplicationScopeService _scope;
    private IMap _map = new Map();

    public CartoDocument(ICartoApplicationScopeService scope, string name = "map1")
    {
        _scope = scope;
        
        this.Name = name;
        this.Map = new Map() { Name = name };
    }

    #region ICartoDocument

    public string Name { get; set; }
    public string FilePath { get; set; } = "";
    public bool Readonly { get; set; } = false;

    public IMap Map
    {
        get => _map;
        set
        {
            _map = value;

            this.Modules = _scope.PluginManager
              .GetPlugins<IMapApplicationModule>(gView.Framework.Common.Plugins.Type.IMapApplicationModule)
              .ToArray() ?? [];

            foreach (var module in Modules)
            {
                module.OnCreate(_map);
            }
        }
    }

    public ITableRelations TableRelations { get; private set; } = new TableRelations(null);

    public IEnumerable<IMapApplicationModule> Modules { get; private set; } = [];

    #endregion

    #region IPersistableLoadAsync

    async public Task<bool> LoadAsync(IPersistStream stream)
    {
        this.Readonly = (bool)stream.Load("readonly", false);

        // Kompatibility to older versions:
        // more maps in one document were possible...
        int focusIndex = (int)stream.Load("focusMapIndex", 0);

        IMap map;
        int mapIndex = 0;
        while ((map = (await stream.LoadAsync<IMap>("IMap", new Map()))) != null)
        {
            if (mapIndex == focusIndex)
            {
                this.Map = map;
            }
            mapIndex++;
        }
        this.Name = this.Map.Name;

        this.TableRelations = (TableRelations)stream.Load("TableRelations", new TableRelations(null), new TableRelations(null));

        // Modules, eg MapEditLayers
        stream.Load("Moduls", null, new ModulsPersists(_scope.PluginManager, this));

        return true;
    }

    public void Save(IPersistStream stream)
    {
        stream.Save("readonly", this.Readonly);
        stream.Save("focusMapIndex", 0);  // Compatibility to older versions

        stream.Save("IMap", this.Map);
        stream.Save("TableRelations", this.TableRelations);

        // Modules, eg MapEditLayers
        stream.Save("Moduls", new ModulsPersists(_scope.PluginManager, this));
    }

    #endregion

    #region Classes

    internal class ModulsPersists : IPersistable
    {
        private ICartoDocument? _document = null;
        private PluginManagerService? _pluginManager = null;

        public ModulsPersists(PluginManagerService pluginManager, ICartoDocument? document)
        {
            _pluginManager = pluginManager;
            _document = document;
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_document == null || _pluginManager == null)
            {
                return;
            }

            while (true)
            {
                ModulePersist? moduleper = stream.Load("Module", null, new ModulePersist(_pluginManager, _document)) as ModulePersist;
                if (moduleper == null)
                {
                    break;
                }
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_document == null || _pluginManager == null)
            {
                return;
            }

            foreach(var module in _document.Modules)
            {
                if (module is IPersistable)
                {
                    stream.Save("Module", new ModulePersist(_pluginManager, module));
                }
            }
        }

        #endregion
    }

    private class ModulePersist : IPersistable
    {
        private IMapApplicationModule? _module = null;
        private ICartoDocument? _document = null;
        private PluginManagerService? _pluginManager = null;

        public ModulePersist(PluginManagerService pluginManager, IMapApplicationModule module)
        {
            _pluginManager = pluginManager;
            _module = module;
        }
        public ModulePersist(PluginManagerService pluginManager, ICartoDocument? document)
        {
            _pluginManager = pluginManager;
            _document = document;
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_document == null || _pluginManager == null)
            {
                return;
            }

            try
            {
                Guid guid = new Guid((string)stream.Load("GUID")!);
                IMapApplicationModule? module = _document.Modules
                    .Where(m=> guid.Equals(_pluginManager.PluginGuid(m)))
                    .FirstOrDefault();

                if (module is IPersistable persist)
                {
                    persist.Load(stream);
                } 
            }
            catch { }
        }

        public void Save(IPersistStream stream)
        {
            if (_module == null || _pluginManager == null)
            {
                return;
            }

            if (_module is IPersistable persist)
            {
                stream.Save("GUID", PlugInManager.PlugInID(_module).ToString());
                persist.Save(stream);
            }
        }

        #endregion
    }

    #endregion
}

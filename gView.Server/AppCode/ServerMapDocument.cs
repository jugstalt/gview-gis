using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Data.Relations;
using gView.Framework.IO;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class ServerMapDocument : IMapDocument, IPersistable
    {
        private List<IMap> _maps = new List<IMap>();
        private ITableRelations _tableRelations;

        public ServerMapDocument()
        {
            _tableRelations = new TableRelations(this);
        }

        #region IMapDocument Member

        public event LayerAddedEvent LayerAdded;
        public event LayerRemovedEvent LayerRemoved;
        public event MapAddedEvent MapAdded;
        public event MapDeletedEvent MapDeleted;
        public event MapScaleChangedEvent MapScaleChanged;
        public event AfterSetFocusMapEvent AfterSetFocusMap;

        public List<IMap> Maps
        {
            get { return _maps; }
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
            if (InternetMapServer.Instance == null) return false;
            if (map == null || _maps.Contains(map)) return true;

            _maps.Add(map);
            if (MapAdded != null) MapAdded(map);
            return true;
        }

        public bool RemoveMap(IMap map)
        {
            if (map == null || !_maps.Contains(map)) return false;

            _maps.Remove(map);
            if (MapDeleted != null) MapDeleted(map);
            return true;
        }

        public IMap this[string mapName]
        {
            get
            {
                foreach (IMap map in _maps)
                {
                    if (map.Name == mapName)
                        return map;
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
                            return map;
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

        public bool LoadMapDocument(string path)
        {
            XmlStream stream = new XmlStream("");
            if (stream.ReadStream(path))
            {
                while (_maps.Count > 0)
                {
                    this.RemoveMap((IMap)_maps[0]);
                }

                stream.Load("MapDocument", null, this);
                return true;
            }
            return false;
        }

        #region IPersistable Member

        public string PersistID
        {
            get
            {
                return "";
            }
        }

        public void Load(IPersistStream stream)
        {
            while (_maps.Count > 0)
            {
                this.RemoveMap((IMap)_maps[0]);
            }

            IMap map;
            while ((map = (IMap)stream.Load("IMap", null, new gView.Framework.Carto.Map())) != null)
            {
                this.AddMap(map);
            }
        }

        public void Save(IPersistStream stream)
        {
            foreach (IMap map in _maps)
            {
                stream.Save("IMap", map);
            }
        }

        #endregion
    }
}

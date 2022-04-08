using System.Collections.Generic;
using gView.Framework.Data;
using gView.Framework.Carto;

namespace gView.Framework.UI
{
    public interface IMapDocument  // IMxDocument
    {
        event LayerAddedEvent LayerAdded;
        event LayerRemovedEvent LayerRemoved;
        event MapAddedEvent MapAdded;
        event MapDeletedEvent MapDeleted;
        event MapScaleChangedEvent MapScaleChanged;
        event AfterSetFocusMapEvent AfterSetFocusMap;

        IEnumerable<IMap> Maps { get; }
        IMap FocusMap { get; set; }

        bool AddMap(IMap map);
        bool RemoveMap(IMap map);

        IMap this[string mapName]
        {
            get;
        }

        IMap this[IDatasetElement layer]
        {
            get;
        }

        IApplication Application
        {
            get;
        }

        ITableRelations TableRelations { get; }
    }

    
}
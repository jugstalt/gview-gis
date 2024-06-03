using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using System.Collections.Generic;

namespace gView.Framework.Core.UI
{
    public interface IMapDocument  // IMxDocument
    {
        IEnumerable<IMap> Maps { get; }
        IMap FocusMap { get; set; }
        bool Readonly { get; set; }

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
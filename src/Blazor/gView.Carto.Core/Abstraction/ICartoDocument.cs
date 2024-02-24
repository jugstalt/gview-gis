using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using System.Collections.Generic;

namespace gView.Carto.Core.Abstraction;

public interface ICartoDocument : IPersistableLoadAsync
{
    string FilePath { get; set; }

    IMap Map { get; }

    ITableRelations TableRelations { get; }

    IEnumerable<IMapApplicationModule> Modules { get; }
}

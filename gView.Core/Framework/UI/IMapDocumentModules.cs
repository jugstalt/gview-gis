using System.Collections.Generic;
using gView.Framework.Carto;
using gView.Framework.system;

namespace gView.Framework.UI
{
    public interface IMapDocumentModules
    {
        IEnumerable<IMapApplicationModule> GetMapModules(IMap map);
    }

    
}
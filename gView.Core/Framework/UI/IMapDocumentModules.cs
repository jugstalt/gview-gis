using gView.Framework.Carto;
using gView.Framework.system;
using System.Collections.Generic;

namespace gView.Framework.UI
{
    public interface IMapDocumentModules
    {
        IEnumerable<IMapApplicationModule> GetMapModules(IMap map);
    }


}
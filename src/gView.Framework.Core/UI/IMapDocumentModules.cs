using gView.Framework.Core.Carto;
using gView.Framework.Core.system;
using System.Collections.Generic;

namespace gView.Framework.Core.UI
{
    public interface IMapDocumentModules
    {
        IEnumerable<IMapApplicationModule> GetMapModules(IMap map);
    }


}
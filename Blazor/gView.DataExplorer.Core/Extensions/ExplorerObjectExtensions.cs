using gView.Framework.DataExplorer.Abstraction;
using System.Collections.Generic;

namespace gView.DataExplorer.Core.Extensions;

static public class ExplorerObjectExtensions
{
    static public IEnumerable<IExplorerObject> GetAncestors(this IExplorerObject obj, bool includeSelf)
    {
        List<IExplorerObject> ancestors = new List<IExplorerObject>();

        if (includeSelf)
        {
            ancestors.Add(obj);
        }

        while (obj.ParentExplorerObject != null)
        {
            ancestors.Insert(0, obj.ParentExplorerObject);
            obj = obj.ParentExplorerObject;
        }

        return ancestors;
    }
}

using gView.Framework.Core.Geometry;
using System.Collections.Generic;

namespace gView.Framework.Core.FDB
{
    public interface IIndexTree
    {
        List<int> FindShapeIds(IEnvelope Bounds);
    }
}

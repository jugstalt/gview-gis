using gView.Framework.Geometry;
using System.Collections.Generic;

namespace gView.Framework.FDB
{
    public interface IIndexTree
    {
        List<int> FindShapeIds(IEnvelope Bounds);
    }
}

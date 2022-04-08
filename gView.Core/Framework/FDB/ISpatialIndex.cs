using gView.Framework.Geometry;
using System.Collections.Generic;
using System.Data;

namespace gView.Framework.FDB
{
    public interface ISpatialIndex
    {
        bool InitIndex(string FCName);
        bool InsertNodes(string FCName, List<SpatialIndexNode> nodes);

        DataTable QueryIDs(IEnvelope env);
    }
}

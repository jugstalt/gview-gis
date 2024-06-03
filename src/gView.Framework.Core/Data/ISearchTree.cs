using gView.Framework.Core.Geometry;
using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public interface ISearchTree
    {
        List<long> CollectNIDs(IGeometry geometry);
        List<long> CollectNIDsPlus(IEnvelope envelope);
        //List<long> CollectNIDs(IEnvelope bounds);
    }
}
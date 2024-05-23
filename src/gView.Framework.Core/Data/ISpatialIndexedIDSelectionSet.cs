using gView.Framework.Core.Geometry;
using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public interface ISpatialIndexedIDSelectionSet : IIDSelectionSet
    {
        void AddID(int ID, IGeometry geometry);
        long NID(int id);
        List<int> IDsInEnvelope(IEnvelope envelope);
    }
}
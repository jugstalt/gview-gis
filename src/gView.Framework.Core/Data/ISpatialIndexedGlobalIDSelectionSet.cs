using gView.Framework.Core.Geometry;
using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public interface ISpatialIndexedGlobalIDSelectionSet : IGlobalIDSelectionSet
    {
        void AddID(long ID, IGeometry geometry);
        long NID(long id);
        List<long> IDsInEnvelope(IEnvelope envelope);
    }
}
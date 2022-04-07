using System.Collections.Generic;

namespace gView.Framework.Geometry
{
    public interface IAggregateGeometry : IGeometry, IEnumerable<IGeometry>
    {
        void AddGeometry(IGeometry geometry);
        void InsertGeometry(IGeometry geometry, int pos);
        void RemoveGeometry(int pos);

        int GeometryCount { get; }
        IGeometry this[int geometryIndex] { get; }

        List<IPoint> PointGeometries { get; }
        IMultiPoint MergedPointGeometries { get; }
        List<IPolyline> PolylineGeometries { get; }
        IPolyline MergedPolylineGeometries { get; }
        List<IPolygon> PolygonGeometries { get; }
        IPolygon MergedPolygonGeometries { get; }
    }
}

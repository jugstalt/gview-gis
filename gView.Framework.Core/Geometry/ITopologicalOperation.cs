namespace gView.Framework.Core.Geometry
{
    public interface ITopologicalOperation
    {
        IPolygon Buffer(double distance);
        void Clip(IEnvelope clipper);
        void Intersect(IGeometry geometry);
        void Difference(IGeometry geometry);
        void SymDifference(IGeometry geometry);
        void Union(IGeometry geometry);

        void Clip(IEnvelope clipper, out IGeometry result);
        void Intersect(IGeometry geometry, out IGeometry result);
        //void Difference(IGeometry geometry, out IGeometry result);
        //void SymDifference(IGeometry geometry, out IGeometry result);
        //void Union(IGeometry geometry, out IGeometry result);
    }
}

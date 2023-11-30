namespace gView.Framework.Core.Geometry
{
    public interface IRing : IPath
    {
        double Area { get; }
        IPoint Centroid { get; }
        void Close(double tolerance = GeometryConst.Epsilon);

        IPolygon ToPolygon();
    }
}

namespace gView.Framework.Geometry
{
    public interface IRing : IPath
    {
        double Area { get; }
        IPoint Centroid { get; }
        void Close();

        IPolygon ToPolygon();
    }
}

namespace gView.Framework.Core.Geometry
{
    public interface IPoint : IGeometry
    {
        double X { get; set; }
        double Y { get; set; }
        double Z { get; set; }
        double M { get; set; }

        double Distance(IPoint p);
        double Distance2(IPoint p);

        double Distance2D(IPoint p);
    }
}

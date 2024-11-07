namespace gView.Framework.Core.Data
{
    public interface IRasterWorldFile
    {
        string Filename { get; }
        bool isGeoReferenced
        {
            get;
        }

        double dx_X { get; }
        double dx_Y { get; }
        double dy_X { get; }
        double dy_Y { get; }

        double cellX { get; }
        double cellY { get; }

        double X { get; }
        double Y { get; }
    }
}
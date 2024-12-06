using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Carto
{
    public interface IDisplayRotation
    {
        bool UseDisplayRotation
        {
            get;
        }

        double DisplayRotation
        {
            get;
            set;
        }

        void Rotate(ref double x, ref double y);
        void RotateInverse(ref double x, ref double y);
        IEnvelope RotatedBounds();
    }
}
using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Carto
{
    public interface IDisplayTransformation
    {
        bool UseTransformation
        {
            get;
        }

        double DisplayRotation
        {
            get;
            set;
        }

        void Transform(IDisplay display, ref double x, ref double y);
        void InvTransform(IDisplay display, ref double x, ref double y);
        IEnvelope TransformedBounds(IDisplay display);
    }
}
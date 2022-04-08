using System;

namespace gView.Framework.Geometry
{
    public interface IPath : IPointCollection, ICloneable
    {
        double Length { get; }

        void ClosePath();

        void ChangeDirection();
        IPath Trim(double length);

        IPoint MidPoint2D { get; }

        IPolyline ToPolyline();
    }
}

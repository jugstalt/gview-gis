using System.Collections.Generic;

namespace gView.Framework.Core.Geometry
{
    public interface IPolyline : IGeometry, IEnumerable<IPath>
    {
        void AddPath(IPath path);
        void InsertPath(IPath path, int pos);
        void RemovePath(int pos);

        int PathCount { get; }
        IPath this[int pathIndex] { get; }

        IEnumerable<IPath> Paths { get; }

        double Length { get; }

        double Distance2D(IPolyline p);
    }
}

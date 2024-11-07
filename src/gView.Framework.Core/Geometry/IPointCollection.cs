using System.Collections.Generic;
using System.IO;

namespace gView.Framework.Core.Geometry
{
    public interface IPointCollection : IEnumerable<IPoint>
    {
        void AddPoint(IPoint point);
        void InsertPoint(IPoint point, int pos);
        void RemovePoint(int pos);
        //void AddPoints(IPointCollection pColl);
        void AddPoints(IEnumerable<IPoint> points);

        int PointCount { get; }
        IPoint this[int pointIndex] { get; }

        IEnvelope Envelope { get; }

        void Serialize(BinaryWriter w, IGeometryDef geomDef);
        void Deserialize(BinaryReader r, IGeometryDef geomDef);

        bool Equals(object obj, double epsi);

        void RemoveIdentNeighbors(double tolerance);

        IPoint[] ToArray(int fromIndex = 0, bool reverse = false);
    }
}

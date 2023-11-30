using System;
using System.IO;

namespace gView.Framework.Core.Geometry
{


    /// <summary>
    /// Zusammenfassung für IGeometry.
    /// </summary>

    public interface IGeometry : ICloneable
    {
        GeometryType GeometryType { get; }
        IEnvelope Envelope { get; }
        int? Srs { get; set; }

        int VertexCount { get; }

        void Serialize(BinaryWriter w, IGeometryDef geomDef);
        void Deserialize(BinaryReader r, IGeometryDef geomDef);

        bool Equals(object obj, double epsi);

        void Clean(CleanGemetryMethods methods, double tolerance = 1e-8);
        bool IsEmpty();
    }
}

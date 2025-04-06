using System;

namespace gView.Framework.Core.Geometry;

public interface IGeometricTransformer : IDisposable
{
    void SetSpatialReferences(ISpatialReference from, ISpatialReference to);

    object Transform2D(object geometry);
    object InvTransform2D(object geometry);

    void Release();
}

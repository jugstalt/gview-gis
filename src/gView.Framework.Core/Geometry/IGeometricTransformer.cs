using System;

namespace gView.Framework.Core.Geometry;

public enum TransformMethod
{
    _2D,
    _3D,
    KeepZM
}

public interface IGeometricTransformer : IDisposable
{
    void SetSpatialReferences(ISpatialReference from, ISpatialReference to);

    object Transform2D(object geometry, TransformMethod method = TransformMethod._2D);
    object InvTransform2D(object geometry, TransformMethod method = TransformMethod._2D);

    void Release();
}

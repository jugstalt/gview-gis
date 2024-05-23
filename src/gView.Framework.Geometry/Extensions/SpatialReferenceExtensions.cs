#nullable enable

using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry.Extensions;
static public class SpatialReferenceExtensions
{
    static public bool IsWgs84(this ISpatialReference? sRef)
        => sRef?.EpsgCode == SpatialReference.EpsgWgs84;

    static public bool IsWebMercator(this ISpatialReference? sRef)
        => sRef?.EpsgCode == SpatialReference.EpsgWebMercator;
}

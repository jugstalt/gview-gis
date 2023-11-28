using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry
{
    public enum GeoTranformerType
    {
        ManagedProj4 = 0,
        NativeProj4 = 1
    }

    static public class GeometricTransformerFactory
    {
        static public GeoTranformerType TransformerType = GeoTranformerType.NativeProj4;

        static public IGeometricTransformer Create()
        {
            if (TransformerType == GeoTranformerType.NativeProj4)
            {
                return new GeometricTransformerProj4Nativ();
            }

            return new GeometricTransformerProj4Managed();
        }

        static public IGeometry Transform2D(IGeometry geometry, ISpatialReference from, ISpatialReference to)
        {
            if (TransformerType == GeoTranformerType.NativeProj4)
            {
                return GeometricTransformerProj4Nativ.Transform2D(geometry, from, to);
            }

            return GeometricTransformerProj4Managed.Transform2D(geometry, from, to);
        }

        static public IGeometry InvTransform2D(IGeometry geometry, ISpatialReference from, ISpatialReference to)
        {
            if (TransformerType == GeoTranformerType.NativeProj4)
            {
                return GeometricTransformerProj4Nativ.InvTransform2D(geometry, from, to);
            }

            return GeometricTransformerProj4Managed.InvTransform2D(geometry, from, to);
        }
    }
}

#nullable enable

using gView.Framework.Core.Geometry;
using System;

namespace gView.Framework.Geometry
{
    public enum GeoTranformerType
    {
        ManagedProj4 = 0,
        NativeProj4 = 1,
        NativeProj6 = 2,
    }

    static public class GeometricTransformerFactory
    {
        static public string PROJ_LIB =
            System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!,
            "share", "proj");


        static public GeoTranformerType TransformerType = GeoTranformerType.NativeProj4;

        static public IGeometricTransformer Create(IDatumsTransformations? datumsTransformations)
        {
            return TransformerType switch
            {
                GeoTranformerType.NativeProj4 => new GeometricTransformerProj4Nativ(datumsTransformations),
                GeoTranformerType.NativeProj6 => new GeometricTransformerProj6Nativ(datumsTransformations),
                GeoTranformerType.ManagedProj4 => new GeometricTransformerProj4Managed(datumsTransformations),
                _ => throw new NotImplementedException($"GeometricTransformerFactory: {TransformerType} not implemented!"),
            };
        }

        static public IGeometry Transform2D(IGeometry geometry, ISpatialReference from, ISpatialReference to, IDatumsTransformations? datumsTransformations = null)
        {
            return TransformerType switch
            {
                GeoTranformerType.NativeProj4 => GeometricTransformerProj4Nativ.Transform2D(geometry, from, to, datumsTransformations),
                GeoTranformerType.NativeProj6 => GeometricTransformerProj6Nativ.Transform2D(geometry, from, to, datumsTransformations),
                GeoTranformerType.ManagedProj4 => GeometricTransformerProj4Managed.Transform2D(geometry, from, to, datumsTransformations),
                _ => throw new NotImplementedException($"GeometricTransformerFactory: {TransformerType} not implemented!"),
            };
        }

        static public IGeometry InvTransform2D(IGeometry geometry, ISpatialReference from, ISpatialReference to, IDatumsTransformations? datumsTransformations)
        {
            return TransformerType switch
            {
                GeoTranformerType.NativeProj4 => GeometricTransformerProj4Nativ.InvTransform2D(geometry, from, to, datumsTransformations),
                GeoTranformerType.NativeProj6 => GeometricTransformerProj6Nativ.InvTransform2D(geometry, from, to, datumsTransformations),
                GeoTranformerType.ManagedProj4 => GeometricTransformerProj4Managed.InvTransform2D(geometry, from, to, datumsTransformations),
                _ => throw new NotImplementedException($"GeometricTransformerFactory: {TransformerType} not implemented!")
            };
        }

        static public string[] SupportedGridShifts()
        {
            return TransformerType switch
            {
                GeoTranformerType.NativeProj4 => new GeometricTransformerProj4Nativ(null).GridShiftNames(),
                GeoTranformerType.NativeProj6 => [],
                GeoTranformerType.ManagedProj4 => new GeometricTransformerProj4Managed(null).GridShiftNames(),
                _ => throw new NotImplementedException($"GeometricTransformerFactory: {TransformerType} not implemented!")
            };
        }
    }
}


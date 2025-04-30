#nullable enable

using gView.Framework.Core.Geometry;
using System;

namespace gView.Framework.Geometry
{
    public enum GeoTranformerType
    {
        ManagedProj4Parallel = 0,
        ManagedProj4 = 1,
        NativeProj4 = 2
    }

    static public class GeometricTransformerFactory
    {
        static public GeoTranformerType TransformerType = GeoTranformerType.ManagedProj4Parallel;

        static public string PROJ_LIB =
            System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!,
            "share",
            TransformerType switch {
                _ => "proj"
            }
            );

        static public IGeometricTransformer Create(IDatumTransformations? datumTransformations)
        {
            return TransformerType switch
            {
                GeoTranformerType.NativeProj4 => new GeometricTransformerProj4Nativ(datumTransformations),
                //GeoTranformerType.NativeProj6 => new GeometricTransformerProj6Nativ(datumTransformations),
                //GeoTranformerType.NativeProj9 => new GeometricTransformerProj9Nativ(datumTransformations),
                GeoTranformerType.ManagedProj4 => new GeometricTransformerProj4Managed(datumTransformations),
                GeoTranformerType.ManagedProj4Parallel => new GeometricTransformerProj4ManagedParallel(datumTransformations),
                _ => throw new NotImplementedException($"GeometricTransformerFactory: {TransformerType} not implemented!"),
            };
        }

        static public IGeometry Transform2D(IGeometry geometry, ISpatialReference from, ISpatialReference to, IDatumTransformations? datumTransformations)
        {
            return TransformerType switch
            {
                GeoTranformerType.NativeProj4 => GeometricTransformerProj4Nativ.Transform2D(geometry, from, to, datumTransformations),
                //GeoTranformerType.NativeProj6 => GeometricTransformerProj6Nativ.Transform2D(geometry, from, to, datumTransformations),
                //GeoTranformerType.NativeProj9 => GeometricTransformerProj9Nativ.Transform2D(geometry, from, to, datumTransformations),
                GeoTranformerType.ManagedProj4 => GeometricTransformerProj4Managed.Transform2D(geometry, from, to, datumTransformations),
                GeoTranformerType.ManagedProj4Parallel => GeometricTransformerProj4ManagedParallel.Transform2D(geometry, from, to, datumTransformations),
                _ => throw new NotImplementedException($"GeometricTransformerFactory: {TransformerType} not implemented!"),
            };
        }

        static public IGeometry InvTransform2D(IGeometry geometry, ISpatialReference from, ISpatialReference to, IDatumTransformations? datumTransformations)
        {
            return TransformerType switch
            {
                GeoTranformerType.NativeProj4 => GeometricTransformerProj4Nativ.InvTransform2D(geometry, from, to, datumTransformations),
                //GeoTranformerType.NativeProj6 => GeometricTransformerProj6Nativ.InvTransform2D(geometry, from, to, datumTransformations),
                //GeoTranformerType.NativeProj9 => GeometricTransformerProj9Nativ.InvTransform2D(geometry, from, to, datumTransformations),
                GeoTranformerType.ManagedProj4 => GeometricTransformerProj4Managed.InvTransform2D(geometry, from, to, datumTransformations),
                GeoTranformerType.ManagedProj4Parallel => GeometricTransformerProj4ManagedParallel.InvTransform2D(geometry, from, to, datumTransformations),
                _ => throw new NotImplementedException($"GeometricTransformerFactory: {TransformerType} not implemented!")
            };
        }

        static public (string shortName, string name)[] SupportedGridShifts()
        {
            return TransformerType switch
            {
                GeoTranformerType.NativeProj4 => new GeometricTransformerProj4Nativ(null).GridShiftNames(),
                GeoTranformerType.ManagedProj4 => new GeometricTransformerProj4Managed(null).GridShiftNames(),
                GeoTranformerType.ManagedProj4Parallel => new GeometricTransformerProj4ManagedParallel(null).GridShiftNames(),
                _ => throw new NotImplementedException($"GeometricTransformerFactory: {TransformerType} not implemented!")
            };
        }

        static public (string shortName, string name)[] SupportedEllipsoids()
        {
            return TransformerType switch
            {
                GeoTranformerType.NativeProj4 => new GeometricTransformerProj4Nativ(null).EllipsoidNames(),
                GeoTranformerType.ManagedProj4 => new GeometricTransformerProj4Managed(null).EllipsoidNames(),
                GeoTranformerType.ManagedProj4Parallel => new GeometricTransformerProj4ManagedParallel(null).EllipsoidNames(),
                _ => throw new NotImplementedException($"GeometricTransformerFactory: {TransformerType} not implemented!")
            };
        }
    }
}


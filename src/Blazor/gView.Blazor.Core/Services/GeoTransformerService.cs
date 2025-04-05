using gView.Blazor.Core.Extensions;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;

namespace gView.Blazor.Core.Services;

public class GeoTransformerService
{
    public IGeometry ToWGS84(
                     IGeometry geometry, 
                     ISpatialReference fromSRef,
                     IDatumsTransformations? datumsTransformations)
    {
        var toSRef = new SpatialReference($"epsg:4326");

        return Transform(geometry, fromSRef, toSRef, datumsTransformations);
    }

    public IGeometry FromWGS84(
                    IGeometry geometry, 
                    ISpatialReference toSRef,
                    IDatumsTransformations? datumsTransformations)
    {
        var fromSRef = new SpatialReference($"epsg:4326");

        return Transform(geometry, fromSRef, toSRef, datumsTransformations);
    }

    public IGeometry Transform(
                    IGeometry geometry, 
                    int fromEpsg, int toEpsg,
                    IDatumsTransformations? datumsTransformations)
    {
        var fromSRef = new SpatialReference($"epsg:{fromEpsg}");
        var toSRef = new SpatialReference($"epsg:{toEpsg}");

        return Transform(geometry, fromSRef, toSRef, datumsTransformations);
    }

    public IGeometry Transform(
                    IGeometry geometry, 
                    ISpatialReference? fromSRef, 
                    ISpatialReference? toSRef,
                    IDatumsTransformations? datumsTransformations)
    {
        if (fromSRef is null || toSRef is null)
        {
            return geometry;
        }

        using (var transformer = GeometricTransformerFactory.Create(datumsTransformations))
        {
            transformer.SetSpatialReferences(fromSRef, toSRef);
            var result = (transformer.Transform2D(geometry) as IGeometry)
                            .ThrowIfNull(() => $"Geometry {geometry} can not transformed to a valid geometry from {fromSRef.Name} to {toSRef.Name}");

            return result;
        }
    }
}

using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Blazor.Core.Services;

public class GeoTransformerService
{
    public IGeometry ToWGS84(IGeometry geometry, ISpatialReference fromSRef)
    {
        var toSRef = new SpatialReference($"epsg:4326");

        return Transform(geometry, fromSRef, toSRef); 
    }

    public IGeometry Transform(IGeometry geometry, int fromEpsg, int toEpsg)
    {
        var fromSRef = new SpatialReference($"epsg:{fromEpsg}");
        var toSRef = new SpatialReference($"epsg:{toEpsg}");

        return Transform(geometry, fromSRef, toSRef); 
    }

    public IGeometry Transform(IGeometry geometry, ISpatialReference fromSRef, ISpatialReference toSRef)
    {
        using (var transformer = GeometricTransformerFactory.Create())
        {
            transformer.SetSpatialReferences(fromSRef, toSRef);
            return transformer.Transform2D(geometry) as IGeometry;
        }
    }
}

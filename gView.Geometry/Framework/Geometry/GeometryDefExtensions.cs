using System;

namespace gView.Framework.Geometry
{
    static public class GeometryDefExtensions
    {
        static public IGeometry ConvertTo(this IGeometryDef geomDef, IGeometry geometry)
        {
            if (geomDef.GeometryType == GeometryType.Point)
            {
                if (geometry is IPoint)
                {
                    return geometry;
                }
                if (geometry is IMultiPoint)
                {
                    if (((IMultiPoint)geometry).PointCount == 1)
                    {
                        return geomDef.ConvertTo(((IMultiPoint)geometry)[0]);
                    }
                }
            }

            if (geomDef.GeometryType == GeometryType.Multipoint)
            {
                if (geometry is IMultiPoint)
                {
                    return geometry;
                }
                if (geometry is IPoint)
                {
                    var multiPoint = new MultiPoint();
                    multiPoint.AddPoint((IPoint)geometry);

                    return geomDef.ConvertTo(multiPoint);
                }
            }

            if (geomDef.GeometryType == GeometryType.Polyline)
            {
                if (geometry is IPolyline)
                {
                    return geometry;
                }
            }

            if (geomDef.GeometryType == GeometryType.Polygon)
            {
                if (geometry is IPolygon)
                {
                    return geometry;
                }
            }

            if (geomDef.GeometryType == GeometryType.Envelope)
            {
                if (geometry is IEnvelope)
                {
                    return geometry;
                }
                else if (geometry != null)
                {
                    return geomDef.ConvertTo(geometry.Envelope);
                }
            }

            if (geomDef.GeometryType == GeometryType.Aggregate)
            {
                if (geometry is IAggregateGeometry)
                {
                    return geometry;
                }
                else if (geometry != null)
                {
                    var aggregateGeometry = new AggregateGeometry();
                    aggregateGeometry.AddGeometry(geometry);

                    return geomDef.ConvertTo(aggregateGeometry);
                }
            }

            throw new ArgumentException("Unconvertable for geometrys type "
                + geomDef.GeometryType.ToString() + ": "
                + (geometry != null ? geometry.GetType().ToString() : "NULL"));
        }
    }
}

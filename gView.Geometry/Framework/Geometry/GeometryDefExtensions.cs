using System;

//[assembly: InternalsVisibleTo("gView.OGC, PublicKey=0024000004800000940000000602000000240000525341310004000001000100916d0be3f662c2d3589fbe93479f3215e23fd195db9a20e77f42dc1d2942bd48cad3ea36b797f57880e6c31af0c238d2e445898c8ecce990aacbb70ae05a10aff73ab65c7db86366697f934b780238ed8fd1b2e28ba679a97e060b53fce66118e129b91d24f392d4dd3d482fa4173e61f18c74cda9f35721a97e77afbbc96dd2")]


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

using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.SpatialAlgorithms
{
    static public class Extensions
    {
        static public IGeometry MakeValid(this IGeometry geometry, geometryType geometryType= geometryType.Unknown, bool closeRings=false)
        {
            if(geometry==null)
            {
                throw new InvalidGeometryException("geometry is null");
            }

            #region Check geometry type

            switch(geometryType)
            {
                case geometryType.Point:
                    if(!(geometry is IPoint))
                        throw new InvalidGeometryException("Invalid point type: " + geometry.GetType().ToString());
                    break;
                case geometryType.Multipoint:
                    if (!(geometry is IMultiPoint))
                        throw new InvalidGeometryException("Invalid multipoint type: " + geometry.GetType().ToString());
                    break;
                case geometryType.Polyline:
                    if (!(geometry is IPolyline))
                        throw new InvalidGeometryException("Invalid polyline type: " + geometry.GetType().ToString());
                    break;
                case geometryType.Polygon:
                    if (!(geometry is IPolygon))
                        throw new InvalidGeometryException("Invalid polygon type: " + geometry.GetType().ToString());
                    break;
                case geometryType.Aggregate:
                    if (!(geometry is IAggregateGeometry))
                        throw new InvalidGeometryException("Invalid aggregate geometry type: " + geometry.GetType().ToString());
                    break;
            }

            #endregion

            if (geometry is IPolygon)
            {
                var polygon = (IPolygon)geometry;

                #region Check rings

                for (int p = 0, to = polygon.RingCount; p < to; p++)
                {
                    var ring = polygon[p];

                    if (Algorithm.IsSelfIntersecting(ring))
                    {
                        throw new InvalidGeometryException("Selfintersecting polygon rings are not allowed");
                    }

                    if(closeRings)
                    {
                        ring.Close();
                    }
                }

                #endregion
            }

            return geometry;
        }
    }
}

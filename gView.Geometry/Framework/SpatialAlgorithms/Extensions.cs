using gView.Framework.Core.Geometry;

namespace gView.Framework.SpatialAlgorithms
{
    static public class Extensions
    {
        static public IGeometry MakeValid(this IGeometry geometry,
                                          double tolerance,
                                          GeometryType geometryType = GeometryType.Unknown, 
                                          bool closeRings = false)
        {
            if (geometry == null)
            {
                throw new InvalidGeometryException("geometry is null");
            }

            #region Check geometry type

            switch (geometryType)
            {
                case GeometryType.Point:
                    if (!(geometry is IPoint))
                    {
                        throw new InvalidGeometryException("Invalid point type: " + geometry.GetType().ToString());
                    }

                    break;
                case GeometryType.Multipoint:
                    if (!(geometry is IMultiPoint))
                    {
                        throw new InvalidGeometryException("Invalid multipoint type: " + geometry.GetType().ToString());
                    }

                    break;
                case GeometryType.Polyline:
                    if (!(geometry is IPolyline))
                    {
                        throw new InvalidGeometryException("Invalid polyline type: " + geometry.GetType().ToString());
                    }

                    break;
                case GeometryType.Polygon:
                    if (!(geometry is IPolygon))
                    {
                        throw new InvalidGeometryException("Invalid polygon type: " + geometry.GetType().ToString());
                    }

                    break;
                case GeometryType.Aggregate:
                    if (!(geometry is IAggregateGeometry))
                    {
                        throw new InvalidGeometryException("Invalid aggregate geometry type: " + geometry.GetType().ToString());
                    }

                    break;
            }

            #endregion

            if (geometry is IPolyline)
            {
                var polyline = (IPolyline)geometry;

                #region Remove Zero Length Paths 

                int removePath;
                do
                {
                    removePath = -1;

                    for (int p = 0, to = polyline.PathCount; p < to; p++)
                    {
                        if (polyline[p] == null || polyline[p].Length == 0.0)
                        {
                            removePath = p;
                            break;
                        }
                    }

                    if (removePath >= 0)
                    {
                        polyline.RemovePath(removePath);
                    }
                }
                while (removePath >= 0);

                #endregion
            }

            if (geometry is IPolygon)
            {
                var polygon = (IPolygon)geometry;

                #region Remove Zero Area Rings

                int removeRing;
                do
                {
                    removeRing = -1;

                    for (int p = 0, to = polygon.RingCount; p < to; p++)
                    {
                        if (polygon[p] == null || polygon[p].Area == 0.0)
                        {
                            removeRing = p;
                            break;
                        }
                    }

                    if (removeRing >= 0)
                    {
                        polygon.RemoveRing(removeRing);
                    }
                }
                while (removeRing >= 0);

                #endregion

                #region Check rings

                //for (int p = 0, to = polygon.RingCount; p < to; p++)
                //{
                //    var ring = polygon[p];

                //    if (Algorithm.IsSelfIntersecting(ring, tolerance))
                //    {
                //        throw new InvalidGeometryException("Selfintersecting polygon rings are not allowed");
                //    }

                //    if (closeRings)
                //    {
                //        ring.Close();
                //    }
                //}

                #endregion

                //polygon.RemoveLineArtifacts(tolerance);
            }

            return geometry;
        }
    }
}

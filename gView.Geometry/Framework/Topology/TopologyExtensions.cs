using gView.Framework.Geometry;
using gView.Framework.SpatialAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Geometry.Framework.Topology
{
    public static class TopologyExtensions
    {
        #region Polygon

        public static IEnumerable<IPath> ToPaths(this IPolygon polygon)
        {
            List<IPath> paths = new List<IPath>();

            for (int r = 0, to_r = polygon.RingCount; r < to_r; r++)
            {
                var ring = polygon[r];
                ring.Close();
                paths.Add(ring.ToPath());
            }

            return paths;
        }

        public static Path ToPath(this IRing ring)
        {
            return new Path(ring);
        }

        public static IEnumerable<IPath> Clip(this IPolygon polygon, IPath clipee)
        {
            return polygon?.ToPaths().Split(clipee, LineSplitResultType.Even) ?? new Path[0];
        }

        public static IEnumerable<IPolygon> Cut(this Polygon polygon, Path cutter)
        {
            polygon.CloseAllRings();
            polygon.VerifyHoles();

            var polygonPaths = polygon.ToPaths().ToArray();
            IEnumerable<PointM3> intersectionPoints;

            #region Cutter parts

            //
            //  Schnittlinie mit Polygon verschneiden. Das Ergebnis ist die geschnitte Schnittlinie
            //  Das Ergebnis sind die Schnittpunte (PointM3)
            //
            //  Zusätzlich kommen noch die einzeln Schnittpunkte (IntersectionPoints) zurück:
            //  M ... RingIndex
            //  M2 .. Stationierung des Schnittpunktes auf den Polygon Ring 
            //  M3 .. Stationierung des Schnittpunktes auf der Schnittlinie
            //

            var cutterParts = polygonPaths.Split(cutter, out intersectionPoints, LineSplitResultType.All).
                Where(p =>
                {
                    var midPoint = p.MidPoint2D;
                    if (midPoint != null)  // Nur die Teile übernehmen, die auch wirklich über dem Polygon liegen
                    {
                        return Algorithm.Jordan(polygon, midPoint.X, midPoint.Y);
                    }
                    return false;
                });

            if (intersectionPoints.Count() == 0)
            {
                throw new Exception("Keine Schnittpunkte gefunden");
            }

            #endregion

            #region Polygon Rings aufgrund der Schnittpunte schneiden

            //
            // PolygonParts sind die einzelnen geschnitten Pfade des Poylgons.
            // Daraus sollt dann unten die neuen Polygone erzeugt werden (Polygonize)
            //
            var polygonParts = new List<IPath>();

            for (int p = 0, to = polygonPaths.Length; p < to; p++)
            {
                var pathIntersectPoints = intersectionPoints.Where(i => p.Equals(i.M)).ToList();

                #region Remove identic neighbours and order by M2 ( = stat on polygon part)

                pathIntersectPoints = pathIntersectPoints.RemoveIdenticNeighbours<PointM3>().ToList();
                pathIntersectPoints.Sort(new PointM2Comparerer<double>());

                #endregion

                var partPolyline = new Polyline(polygonPaths[p]);
                double stat = 0;
                foreach (var pathIntersectPoint in pathIntersectPoints)
                {
                    if (Math.Abs(stat - (double)pathIntersectPoint.M2) > 1e-7)
                    {
                        var clippedLine = Algorithm.PolylineSplit(partPolyline, stat, (double)pathIntersectPoint.M2);
                        if (clippedLine != null && clippedLine.PathCount == 1)
                        {
                            polygonParts.Add(clippedLine[0]);
                        }
                    }

                    stat = (double)pathIntersectPoint.M2;
                }
                if (stat < partPolyline.Length)
                {
                    var clippedLine = Algorithm.PolylineSplit(partPolyline, stat, partPolyline.Length);
                    polygonParts.Add(clippedLine[0]);
                }
            }

            #endregion

            // 
            // inklusive der geschnitten Schnittlinie
            //
            polygonParts.AddRange(cutterParts);

            List<IPolygon> resultPolygons = new List<IPolygon>(), untouchedPolygons = new List<IPolygon>();

            #region Polygonize

            foreach (var newPolygon in polygonParts.Polygonize())  // aus Pfaden Polygone erzeugen
            {
                if (polygon.Rings.Where(h => h.ToPolygon().Equals(newPolygon)).Count() == 0)  // Polygone, die sich nicht geändert haben hier noch nicht übernehmen -> könnten Löcher sein
                {
                    resultPolygons.Add(newPolygon);
                }
                else
                {
                    untouchedPolygons.Add(newPolygon);
                }
            }

            if (resultPolygons.Count == 0)
            {
                throw new Exception("Der Verschnitt liefert kein Ergebnis");
            }

            #region Unberührute Polygone dem nächstgelegen Schnittpolygon hinzufügen

            foreach (var untouchedPolygon in untouchedPolygons)
            {
                untouchedPolygon.VerifyHoles();

                for (int r = 0; r < untouchedPolygon.RingCount; r++)
                {
                    //
                    // Löcher (Holes) des ürsprünglichen Polygons hier nicht übernehmen
                    // Holes werden im nächsten Schritt übernommen
                    //

                    if (polygon.Holes.Where(h => h.ToPolygon().Equals(untouchedPolygon[r].ToPolygon())).FirstOrDefault() != null)
                    {
                        continue;
                    }

                    int resultIndex = 0;
                    double dist = double.MaxValue;
                    var outouchedRingPolygon = untouchedPolygon[r].ToPolygon();
                    for (int i = 0; i < resultPolygons.Count(); i++)
                    {
                        var ringDistance = resultPolygons[i].Distance2D(outouchedRingPolygon);
                        if (ringDistance < dist)
                        {
                            resultIndex = i;
                            dist = ringDistance;
                        }
                    }

                    resultPolygons[resultIndex].AddRing(untouchedPolygon[r]);
                    untouchedPolygon.RemoveRing(r);
                    r--;
                }
            }

            #endregion

            #region Testen ob einens der Polygone Donat-Hole eines anderen ist...

            //
            //  Die restlichen unberührten Polygone sind dann wahrscheinlich Löcher der geschnitten Polygone  
            //  Wenn nicht sind sie wahrscheinlich Löcher aus dem ursprünglichen Polygon die nach dem schneiden nicht mehr existieren
            //

            foreach (var resultPolygon in resultPolygons.OrderBy(p => p.Area).ToArray())
            {
                foreach (var untoucedPolygon in untouchedPolygons.ToArray())
                {
                    var untouchedRings = untoucedPolygon.Rings;

                    foreach (var resultRing in resultPolygon.Rings)
                    {
                        for (int r = 0; r < untoucedPolygon.RingCount; r++)
                        {
                            var untouchedRing = untoucedPolygon[r];
                            if (Algorithm.Jordan(resultRing, untouchedRing))
                            {
                                resultPolygon.AddRing(untouchedRing);
                                untoucedPolygon.RemoveRing(r);
                                r--;
                            }
                        }
                    }
                }
            }

            #endregion

            //#region alle übrigen Untouced Polygone wieder zu einem Polygon zusammenfassen (das sind Multipart Inseln, die nicht angegriffen wurden)

            //Polygon restPolygon = null;
            //foreach (var untoucedPolygon in untouchedPolygons.Where(p => p.RingCount > 0).OrderByDescending(p => p.Area).ToList())
            //{
            //    bool isHole = untoucedPolygon.RingCount == 1 &&
            //                  polygon.Holes.Where(h => new Polygon(new Ring(h)).Equals(untoucedPolygon)).FirstOrDefault() != null;

            //    if (restPolygon == null)
            //    {
            //        if (!isHole)
            //            restPolygon = untoucedPolygon;
            //    }
            //    else
            //    {
            //        if (!isHole)
            //        {
            //            restPolygon.AddRings(untoucedPolygon.Rings);
            //        }
            //        else if (restPolygon.Rings.Where(r => SpatialAlgorithms.Jordan(r, untoucedPolygon[0])).FirstOrDefault() != null)
            //        {
            //            restPolygon.AddRing(untoucedPolygon[0]);
            //        }
            //    }
            //}
            //if (restPolygon != null)
            //    resultPolygons.Add(restPolygon);

            //#endregion

            #endregion

            return resultPolygons;
        }

        public static IEnumerable<Polygon> Polygonize(this IEnumerable<IPath> paths)
        {
            var pathArray = paths.ToArray();

            List<Polygon> results = new List<Polygon>();

            List<PointM> nodes = new List<PointM>();
            List<Edge> edges = new List<Edge>();

            #region Determine unique nodes & edges

            //
            // Im ersten Schritt wird ein Graph mit knoten und Kanten erzeuge.
            // Über die Verfolgung innerhalb dieses Graphs werden alle möglichen Polygone wieder zusammen gebaut.
            //

            for (int i = 0, to = pathArray.Length; i < to; i++)
            {
                var path = pathArray[i];
                PointM p1 = nodes.Where(p => p.Equals(path[0])).FirstOrDefault();
                if (p1 == null)
                {
                    p1 = new PointM(path[0], nodes.Count);
                    nodes.Add(p1);
                }

                PointM p2 = p2 = nodes.Where(p => p.Equals(path[path.PointCount - 1])).FirstOrDefault();
                if (p2 == null)
                {
                    p2 = new PointM(path[path.PointCount - 1], nodes.Count);
                    nodes.Add(p2);
                }

                edges.Add(new Edge()
                {
                    Index = i,
                    Node1 = (int)p1.M,
                    Node2 = (int)p2.M
                });
            }

            #endregion

            #region Calc Edge Combinations

            //
            //  Alle möglichen Kombination inerhalb des Graphs suchen, mit denen ein Polygon erzeugt werden kann
            //
            List<EdgeIndices> combinations = new List<EdgeIndices>();

            //
            //  Kombinationen werden pro Kante (Path) ermittelt
            // 
            foreach (var edge in edges)
            {
                var polygonEdges = new List<Edge>();
                polygonEdges.Add(edge);

                List<EdgeIndices> edgeCombinations = new List<EdgeIndices>();
                edge.PolygonizeGraph(polygonEdges, edges, edgeCombinations, edge.Node2);
                foreach (var edgeIndices in edgeCombinations)
                {
                    // nur die Kombinatinen übernehmen, die noch nicht schon über eine andere Kante ermittelt wurden.
                    if (!combinations.Contains(edgeIndices))
                    {
                        combinations.Add(edgeIndices);
                    }
                }
            }

            #endregion

            #region Stitch Paths and determinte ring with minium area -> new Polygon

            //
            // Polygone wieder zusammen bauen. 
            // 
            // Dazu wird die möglichen Kombinationen nach der ersten Kange groupiert. 
            // Danach werden alle Polygone für diese Kante aus den Pfanden zusammengestellt (edgeRings)
            // 
            // Übernommen wird pro Kante nur das Polygon mit der kleinsten Fläche
            //
            List<int> startEdgeIndices = combinations.Select(c => c.First()).Distinct().ToList();

            foreach (var startEdgeIndex in startEdgeIndices)
            {
                var edgeRings = combinations.Where(c => c.First() == startEdgeIndex)
                                            .Select(edgeIndices => pathArray.StitchPaths(edgeIndices))
                                            .OrderBy(ring => ring.Area)
                                            .ToArray();

                if (edgeRings.Length > 0 && edgeRings.Select(r => r.Area).Sum() > 0)
                {
                    results.Add(new Polygon(edgeRings[0]));
                }
            }

            #endregion

            return results;
        }

        #region Polygonize Helper

        private static void PolygonizeGraph(this Edge currentEdge, List<Edge> polygonEdges, List<Edge> edges, List<EdgeIndices> combinations, int currentEndNode)
        {
            int targetNodeIndex = polygonEdges[0].Node1;

            if (currentEdge == polygonEdges[0] &&
                currentEdge.Node2 == targetNodeIndex)
            {
                // Simple Ring
                combinations.Add(new EdgeIndices(polygonEdges.Select(e => e.Index).ToList()));
            }
            else if (currentEdge != polygonEdges[0] &&
                    (currentEdge.ContainsNode(targetNodeIndex)))
            {
                // Finished
                combinations.Add(new EdgeIndices(polygonEdges.Select(e => e.Index).ToList()));
            }
            else
            {
                var connectedEdges = currentEdge.ConnectedEdges(edges).Where(e => e.ContainsNode(currentEndNode));
                foreach (var edge in connectedEdges)
                {
                    var currentPolygonEdges = new List<Edge>(polygonEdges);

                    if (currentPolygonEdges.Contains(edge))
                    {
                        continue;
                    }

                    var nextNode = edge.ContainsNode(targetNodeIndex) ?
                        targetNodeIndex : edge.UnusedNode(currentPolygonEdges);

                    if (nextNode < 0)
                    {
                        continue;
                    }

                    currentPolygonEdges.Add(edge);

                    edge.PolygonizeGraph(currentPolygonEdges, edges, combinations, nextNode);
                }
            }
        }

        private static bool Contains(this List<Edge> edges, Edge edge)
        {
            return edges.Where(e => e.Index == edge.Index).FirstOrDefault() != null;
        }

        private static int UnusedNode(this Edge edge, IEnumerable<Edge> edges)
        {
            if (edges.Where(e => e.Node1 == edge.Node1 || e.Node2 == edge.Node1).FirstOrDefault() == null)
            {
                return edge.Node1;
            }

            if (edges.Where(e => e.Node1 == edge.Node2 || e.Node2 == edge.Node2).FirstOrDefault() == null)
            {
                return edge.Node2;
            }

            return -1;
        }

        private static bool ContainsNode(this Edge edge, int nodeIndex)
        {
            return edge.Node1 == nodeIndex || edge.Node2 == nodeIndex;
        }

        private static IEnumerable<Edge> ConnectedEdges(this Edge edge, IEnumerable<Edge> edges)
        {
            return edges.Where(e => e.Index != edge.Index &&
                                    (e.Node1 == edge.Node1 || e.Node1 == edge.Node2 ||
                                     e.Node2 == edge.Node1 || e.Node2 == edge.Node2));
        }

        private static bool Contains(this IEnumerable<EdgeIndices> bag, EdgeIndices indices)
        {
            return bag.Where(i => i.Equals(indices)).FirstOrDefault() != null;
        }

        private static IRing StitchPaths(this IPath[] paths, EdgeIndices edgeIndices)
        {
            IRing result = new Ring();

            foreach (var edgeIndex in edgeIndices)
            {
                var path = paths[edgeIndex];
                if (path.PointCount == 0)
                {
                    continue;
                }

                if (result.PointCount == 0)
                {
                    result.AddPoints(path.ToArray());
                }
                else
                {
                    var lastPoint = result[result.PointCount - 1];
                    if (lastPoint.Equals(path[0]))
                    {
                        result.AddPoints(path.ToArray(1));
                    }
                    else if (lastPoint.Equals(path[path.PointCount - 1]))
                    {
                        result.AddPoints(path.ToArray(1, true));
                    }
                    else
                    {
                        throw new Exception("Can't stitch path to ring");
                    }
                }
            }

            return result;
        }

        #endregion

        #endregion

        #region Polyline

        public static IEnumerable<IPath> ToPaths(this IPolyline polyline)
        {
            List<IPath> paths = new List<IPath>();

            for (int r = 0, to_r = polyline.PathCount; r < to_r; r++)
            {
                var path = polyline[r];
                paths.Add(path);
            }

            return paths;
        }

        public static IEnumerable<Polyline> Cut(this Polyline polyline, Path cutter)
        {
            var polylinePaths = polyline.ToPaths().ToArray();
            IEnumerable<PointM3> intersectionPoints;

            var cutterPargs = polylinePaths.Split(cutter, out intersectionPoints, LineSplitResultType.All);

            #region Pfade aufgrund der Schnittpunkte schneiden

            //
            // PolygonParts sind die einzelnen geschnitten Pfade des Polygons.
            // Daraus sollt dann unten die neuen Polygone erzeugt werden (Polygonize)
            //

            var newPolylineParts = new List<IPath>();
            var untouchedParts = new List<IPath>();

            for (int p = 0, to = polylinePaths.Length; p < to; p++)
            {
                var pathIntersectPoints = intersectionPoints.Where(i => p.Equals(i.M)).ToList();

                #region Remove identic neighbours and order by M2 ( = stat on polygon part)

                pathIntersectPoints = pathIntersectPoints.RemoveIdenticNeighbours<PointM3>().ToList();
                pathIntersectPoints.Sort(new PointM2Comparerer<double>());

                #endregion

                if (pathIntersectPoints.Count() == 0)
                {
                    untouchedParts.Add(polylinePaths[p]);
                    continue;
                }

                var partPolyline = new Polyline(polylinePaths[p]);
                double stat = 0;
                foreach (var pathIntersectPoint in pathIntersectPoints)
                {
                    if (Math.Abs(stat - (double)pathIntersectPoint.M2) > 1e-7)
                    {
                        var clippedLine = Algorithm.PolylineSplit(partPolyline, stat, (double)pathIntersectPoint.M2);
                        if (clippedLine != null && clippedLine.PathCount == 1)
                        {
                            newPolylineParts.Add(clippedLine[0]);
                        }
                    }

                    stat = (double)pathIntersectPoint.M2;
                }
                if (stat < partPolyline.Length)
                {
                    var clippedLine = Algorithm.PolylineSplit(partPolyline, stat, partPolyline.Length);
                    newPolylineParts.Add(clippedLine[0]);
                }
            }

            #endregion

            if (newPolylineParts.Count() == 0)
            {
                throw new Exception("Keine Änderungen festgestellt");
            }

            var newPolylines = newPolylineParts.Select(p => new Polyline(p)).ToArray();

            foreach (var untoucedPart in untouchedParts)
            {
                int resultIndex = 0;
                double dist = double.MaxValue;
                for (int i = 0; i < newPolylines.Count(); i++)
                {
                    double d = newPolylines[i].Distance2D(new Polyline(untoucedPart));
                    if (d < dist)
                    {
                        resultIndex = i;
                        dist = d;
                    }
                }

                newPolylines[resultIndex].AddPath(untoucedPart);
            }

            return newPolylines;
        }

        #endregion

        public static IEnumerable<IPath> Split(this IEnumerable<IPath> paths, IPath clipee, LineSplitResultType resultType = LineSplitResultType.All)
        {
            IEnumerable<PointM3> dummy;

            return paths.Split(clipee, out dummy, resultType);
        }

        public static IEnumerable<IPath> Split(this IEnumerable<IPath> paths, IPath clipee, out IEnumerable<PointM3> iPoints, LineSplitResultType resultType = LineSplitResultType.All)
        {
            List<IPath> result = new List<IPath>();

            #region Determine Points

            List<PointM3> intersectionPoints = new List<PointM3>();

            var pathArray = paths.ToArray();
            for (int i = 0, to = pathArray.Length; i < to; i++)
            {
                intersectionPoints.AddRange(pathArray[i].Intersect(clipee, i));
            }

            #endregion

            #region Remove identic neighbours and order by M3 ( = stat on clipee)

            intersectionPoints = intersectionPoints.RemoveIdenticNeighbours<PointM3>().ToList();
            intersectionPoints.Sort(new PointM3Comparerer<double>());

            #endregion

            #region Split clipee

            double stat = 0D;
            int counter = 0;
            Polyline clipeeLine = new Polyline(clipee);

            foreach (var intersectPoint in intersectionPoints)
            {
                if (Math.Abs(stat - (double)intersectPoint.M3) > 1e-7)
                {
                    var clippedLine = Algorithm.PolylineSplit(clipeeLine, stat, (double)intersectPoint.M3);
                    if (clippedLine != null && clippedLine.PathCount == 1)
                    {

                        if (resultType == LineSplitResultType.All ||
                            (resultType == LineSplitResultType.Even && counter % 2 == 1) ||
                            (resultType == LineSplitResultType.Odd && counter % 2 == 0))
                        {
                            result.Add(clippedLine[0]);
                        }
                    }
                }

                counter++;
                stat = (double)intersectPoint.M3;
            }
            if (stat < clipeeLine.Length)
            {
                var clippedLine = Algorithm.PolylineSplit(clipeeLine, stat, clipeeLine.Length);
                if (clippedLine != null && clippedLine.PathCount == 1)
                {
                    if (resultType == LineSplitResultType.All ||
                        (resultType == LineSplitResultType.Even && counter % 2 == 1) ||
                        (resultType == LineSplitResultType.Odd && counter % 2 == 0))
                    {
                        result.Add(clippedLine[0]);
                    }
                }
            }

            #endregion

            iPoints = intersectionPoints;
            return result;
        }

        public static IEnumerable<PointM3> Intersect(this IPath path1, IPath path2, object M = null)
        {
            List<PointM3> points = new List<PointM3>();

            if (path1 == null || path2 == null)
            {
                return points;
            }

            int pointCount1 = path1.PointCount,
                pointCount2 = path2.PointCount;

            if (pointCount1 == 0 || pointCount2 == 0)
            {
                return points;
            }

            double stat1 = 0;
            for (int t1 = 0; t1 < pointCount1 - 1; t1++)
            {
                IPoint p11 = path1[t1], p12 = path1[t1 + 1];

                double stat2 = 0;
                for (int t2 = 0; t2 < pointCount2 - 1; t2++)
                {
                    IPoint p21 = path2[t2], p22 = path2[t2 + 1];

                    var point = Algorithm.IntersectLine(p11, p12, p21, p22, true);
                    if (point != null)
                    {
                        points.Add(new PointM3(point, M, stat1 + p11.Distance2D(point), stat2 + p21.Distance2D(point)));
                    }

                    stat2 += p21.Distance2D(p22);
                }
                stat1 += p11.Distance2D(p12);
            }

            return points;
        }

        public static IEnumerable<T> RemoveIdenticNeighbours<T>(this IEnumerable<T> points) where T : Point
        {
            var pointList = points.ToList();

            for (int i = 0; i < pointList.Count - 1; i++)
            {
                T p1 = pointList[i], p2 = pointList[i + 1];
                if (p1.Distance2D(p2) < 1e-7)
                {
                    pointList.Remove(p2);
                    i--; // continue with same point
                }
            }

            return pointList;
        }

        #region HelperClasses

        private class Edge
        {
            public int Index { get; set; }
            public int Node1 { get; set; }
            public int Node2 { get; set; }
        }

        private class EdgeIndices : List<int>
        {
            public EdgeIndices()
                : base()
            {

            }

            public EdgeIndices(IEnumerable<int> collection)
                : base(collection)
            {

            }

            public override bool Equals(object obj)
            {
                if (!(obj is EdgeIndices))
                {
                    return false;
                }

                var indices = (EdgeIndices)obj;

                if (indices.Count != this.Count)
                {
                    return false;
                }

                foreach (var index in indices)
                {
                    if (!this.Contains(index))
                    {
                        return false;
                    }
                }

                return true;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        #endregion
    }
}

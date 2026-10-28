using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Geometry.Engine
{
    /// <summary>
    /// Switches that move an area of gView's geometry code to SpatialNET (ADR 0002, phase 2).
    /// All are off by default; gView's own implementation runs then.
    /// </summary>
    /// <remarks>
    /// The initial values come from the environment variable <see cref="EnvironmentVariable"/>, a comma-separated
    /// list of areas (<c>relation</c>, <c>buffer</c>, <c>merge</c>, <c>clip</c>) or <c>all</c>, e.g. <c>GVIEW_SPATIALNET=relation,buffer</c>. It works the same in
    /// every host (Server, WebApps, Cmd tools). Hosts and tests may also set the properties directly.
    /// </remarks>
    public static class GeometryEngineOptions
    {
        public const string EnvironmentVariable = "GVIEW_SPATIALNET";

        static GeometryEngineOptions()
        {
            var areas = ParseAreas(Environment.GetEnvironmentVariable(EnvironmentVariable));

            UseSpatialNetForSpatialRelation = areas.Contains("all") || areas.Contains("relation");
            UseSpatialNetForBuffer = areas.Contains("all") || areas.Contains("buffer");
            UseSpatialNetForMerge = areas.Contains("all") || areas.Contains("merge");
            UseSpatialNetForClip = areas.Contains("all") || areas.Contains("clip");
        }

        /// <summary>
        /// <c>SpatialRelation.Check</c>, the spatial filter fallback of the data sources, runs on SpatialNET predicates.
        /// </summary>
        public static bool UseSpatialNetForSpatialRelation { get; set; }

        /// <summary>
        /// <c>Buffer</c> of points, multipoints, polylines and polygons runs on SpatialNET.
        /// </summary>
        public static bool UseSpatialNetForBuffer { get; set; }

        /// <summary>
        /// Merging polygons (<c>Algorithm.MergePolygons</c>, <c>Algorithm.FastMergePolygon</c>, <c>Polygon.Union</c>
        /// with a polygon) runs on SpatialNET.
        /// </summary>
        public static bool UseSpatialNetForMerge { get; set; }

        /// <summary>
        /// Clipping polygons to an envelope (Clipper's <c>Clip</c>, used by <c>Clip.PerformClip</c>, <c>Polygon.Clip</c>,
        /// highlighting, raster footprints and polygon labels) runs on SpatialNET. Polylines and multipoints keep
        /// gView's own envelope clip.
        /// </summary>
        public static bool UseSpatialNetForClip { get; set; }

        internal static HashSet<string> ParseAreas(string value)
        {
            return new HashSet<string>(
                (value ?? string.Empty)
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(area => area.Trim().ToLowerInvariant())
                    .Where(area => area.Length > 0));
        }
    }
}

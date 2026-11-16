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

        /// <summary>Environment variable for <see cref="LabelSpatialNetMinVertices"/>, e.g. <c>GVIEW_LABEL_SPATIALNET_MIN_VERTICES=12000</c>.</summary>
        public const string LabelMinVerticesEnvironmentVariable = "GVIEW_LABEL_SPATIALNET_MIN_VERTICES";

        /// <summary>Environment variable for <see cref="LabelAlternativeSeparationPixels"/>.</summary>
        public const string LabelSeparationEnvironmentVariable = "GVIEW_LABEL_ALTERNATIVE_SEPARATION_PIXELS";

        /// <summary>Environment variable for <see cref="LabelMaxAlternatives"/>.</summary>
        public const string LabelMaxAlternativesEnvironmentVariable = "GVIEW_LABEL_MAX_ALTERNATIVES";

        static GeometryEngineOptions()
        {
            var areas = ParseAreas(Environment.GetEnvironmentVariable(EnvironmentVariable));

            UseSpatialNetForSpatialRelation = areas.Contains("all") || areas.Contains("relation");
            UseSpatialNetForBuffer = areas.Contains("all") || areas.Contains("buffer");
            UseSpatialNetForMerge = areas.Contains("all") || areas.Contains("merge");
            UseSpatialNetForClip = areas.Contains("all") || areas.Contains("clip");

            LabelSpatialNetMinVertices =
                int.TryParse(Environment.GetEnvironmentVariable(LabelMinVerticesEnvironmentVariable), out int minVertices) && minVertices >= 0
                    ? minVertices
                    : 0;

            LabelAlternativeSeparationPixels =
                double.TryParse(Environment.GetEnvironmentVariable(LabelSeparationEnvironmentVariable),
                    System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double separation) && separation > 0
                    ? separation
                    : 20.0;

            LabelMaxAlternatives =
                int.TryParse(Environment.GetEnvironmentVariable(LabelMaxAlternativesEnvironmentVariable), out int maxAlternatives) && maxAlternatives > 0
                    ? maxAlternatives
                    : 32;
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

        /// <summary>
        /// Polygon labels (<c>SimpleLabelRenderer</c>): polygons with <b>more</b> vertices than this are clipped and
        /// labelled with SpatialNET (label point first, clip only if it is not visible; alternative points lazily).
        /// Smaller polygons keep gView's classic path (Clipper, ring raster). Default 0: all polygons use SpatialNET
        /// (it is faster for polygons of every size, see the label path benchmarks). Set to <see cref="int.MaxValue"/>
        /// for the classic path only; the old behaviour was 12000.
        /// Also settable by <c>GVIEW_LABEL_SPATIALNET_MIN_VERTICES</c>. Independent of <see cref="UseSpatialNetForClip"/>.
        /// </summary>
        public static int LabelSpatialNetMinVertices { get; set; }

        /// <summary>
        /// Polygon labels on the SpatialNET path: after a collision the alternative label points are at least this many
        /// pixels apart (default 20). A smaller distance tries more points (more labels placed in dense maps, but more
        /// label engine checks, which cost most of the time); a larger one is faster and places fewer labels.
        /// Also settable by <c>GVIEW_LABEL_ALTERNATIVE_SEPARATION_PIXELS</c> (a decimal number with a point).
        /// </summary>
        public static double LabelAlternativeSeparationPixels { get; set; }

        /// <summary>
        /// Polygon labels on the SpatialNET path: at most this many alternative label points per polygon after a
        /// collision (default 32), the upper bound of the work for one label.
        /// Also settable by <c>GVIEW_LABEL_MAX_ALTERNATIVES</c>.
        /// </summary>
        public static int LabelMaxAlternatives { get; set; }

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

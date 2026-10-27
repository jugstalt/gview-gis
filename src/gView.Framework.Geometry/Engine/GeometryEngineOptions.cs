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
    /// list of areas (<c>relation</c>) or <c>all</c>, e.g. <c>GVIEW_SPATIALNET=relation</c>. It works the same in
    /// every host (Server, WebApps, Cmd tools). Hosts and tests may also set the properties directly.
    /// </remarks>
    public static class GeometryEngineOptions
    {
        public const string EnvironmentVariable = "GVIEW_SPATIALNET";

        static GeometryEngineOptions()
        {
            var areas = ParseAreas(Environment.GetEnvironmentVariable(EnvironmentVariable));

            UseSpatialNetForSpatialRelation = areas.Contains("all") || areas.Contains("relation");
        }

        /// <summary>
        /// <c>SpatialRelation.Check</c>, the spatial filter fallback of the data sources, runs on SpatialNET predicates.
        /// </summary>
        public static bool UseSpatialNetForSpatialRelation { get; set; }

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

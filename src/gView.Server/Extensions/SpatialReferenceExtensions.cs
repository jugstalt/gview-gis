#nullable enable

using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Server.Extensions;

internal static class SpatialReferenceExtensions
{
    /// <summary>Max. number of codes the test map may ask for at once</summary>
    public const int MaxTestMapProjections = 20;

    /// <summary>
    /// "EPSG:31256,3857, epsg:4326" → distinct EPSG codes (at most <see cref="MaxTestMapProjections"/>),
    /// invalid entries are ignored
    /// </summary>
    public static int[] ParseEpsgCodes(this string? codes)
        => (codes ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(c => c.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase) ? c.Substring(5) : c)
            .Select(c => int.TryParse(c, out int code) && code > 0 ? code : 0)
            .Where(code => code > 0)
            .Distinct()
            .Take(MaxTestMapProjections)
            .ToArray();

    /// <summary>
    /// The projection for OpenLayers: the proj4 definition and the axis orientation as WMS 1.3.0
    /// expects it ("neu": north first, like the GML3 axis order of gView)
    /// </summary>
    public static TestMapProjection? ToTestMapProjection(this ISpatialReference? sRef)
    {
        if (sRef is null)
        {
            return null;
        }

        var proj4 = SpatialReference.ToProj4(sRef);
        if (String.IsNullOrWhiteSpace(proj4))
        {
            return null;
        }

        return new TestMapProjection(
            proj4,
            sRef.Gml3AxisX == AxisDirection.North ? "neu" : "enu");
    }

    /// <summary>Projections of EPSG codes, unknown codes are left out</summary>
    public static IDictionary<string, TestMapProjection> TestMapProjections(this IEnumerable<int> epsgCodes)
    {
        var result = new Dictionary<string, TestMapProjection>();

        foreach (var code in epsgCodes)
        {
            ISpatialReference? sRef = null;
            try
            {
                sRef = SpatialReference.FromID($"epsg:{code}");
            }
            catch { }

            var projection = sRef.ToTestMapProjection();
            if (projection is not null)
            {
                result[$"EPSG:{code}"] = projection;
            }
        }

        return result;
    }
}

public record TestMapProjection(string Proj4, string Axis);

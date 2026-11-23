using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Pipe.Abstraction;
using System.Globalization;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis;

static internal class GisSpatialReferences
{
    static public ISpatialReference FromEpsg(int epsg)
        => SpatialReference.FromID($"epsg:{epsg}")
           ?? throw new PipeException($"gView doesn't know the coordinate system EPSG:{epsg}");

    /// <summary>An EPSG code from a parameter or item field: 31256, "31256" or "EPSG:31256"; null or 0 is none.</summary>
    static public int? ToEpsg(JsonNode? value)
    {
        var text = value?.ToString().Trim() ?? "";
        if (text.StartsWith("epsg:", StringComparison.OrdinalIgnoreCase))
        {
            text = text[5..];
        }
        if (text.Length == 0)
        {
            return null;
        }

        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var code)
            ? code > 0 ? code : null
            : throw new PipeException($"'{value}' is not an EPSG code");
    }

    /// <summary>The EPSG code of a feature class, or null when gView doesn't know it.</summary>
    static public int? Epsg(IFeatureClass featureClass) => Epsg(featureClass.SpatialReference);

    static public int? Epsg(ISpatialReference? spatialReference)
    {
        if (spatialReference is null)
        {
            return null;
        }
        if (spatialReference.EpsgCode > 0)
        {
            return spatialReference.EpsgCode;
        }

        var name = spatialReference.Name ?? "";
        return name.StartsWith("epsg:", StringComparison.OrdinalIgnoreCase)
               && int.TryParse(name[5..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var code)
            ? code
            : null;
    }

    /// <summary>Projects a geometry from one EPSG code to another; unchanged when they are equal or one is unknown.</summary>
    static public IGeometry Project(IGeometry geometry, int? from, int? to)
    {
        if (from is not > 0 || to is not > 0 || from == to)
        {
            return geometry;
        }

        return GeometricTransformerFactory.Transform2D(geometry, FromEpsg(from.Value), FromEpsg(to.Value), null)
            ?? throw new PipeException($"Can't project a geometry from EPSG:{from} to EPSG:{to}");
    }
}

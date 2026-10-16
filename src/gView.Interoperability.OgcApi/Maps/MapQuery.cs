using gView.GraphicsEngine;
using gView.Interoperability.OgcApi.Features;
using System.Globalization;

namespace gView.Interoperability.OgcApi.Maps;

/// <summary>
/// The parsed and validated query parameters of /map and /collections/{collectionId}/map
/// </summary>
public sealed class MapQuery
{
    public const int DefaultWidth = 1024;
    public const int DefaultHeight = 768;
    public const double DefaultMmPerPixel = 0.28;  // OGC standard rendering pixel size
    private const int AbsoluteMaxImageSize = 8192;  // if the service has no limits

    private static readonly string[] IgnoredParameters = ["token", "f"];

    private static readonly string[] KnownParameters =
        ["bbox", "bbox-crs", "crs", "width", "height", "scale-denominator", "mm-per-pixel", "transparent", "bgcolor"];

    /// <summary>
    /// minx, miny, maxx, maxy in the axis order of <see cref="BBoxCrs"/>
    /// </summary>
    public double[]? BBox { get; private set; }
    public CrsReference BBoxCrs { get; private set; } = CrsReference.Crs84;

    /// <summary>
    /// The CRS of the map, null => default (CRS of the map service)
    /// </summary>
    public CrsReference? Crs { get; private set; }

    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public double? ScaleDenominator { get; private set; }
    public double MmPerPixel { get; private set; } = DefaultMmPerPixel;
    public double Dpi => 25.4 / MmPerPixel;

    public bool Transparent { get; private set; }
    public ArgbColor BackgroundColor { get; private set; } = ArgbColor.White;

    /// <summary>
    /// collections=... (dataset map only), null => default visibility of the map
    /// </summary>
    public IReadOnlyList<string>? Collections { get; private set; }

    public ImageFormat Format { get; private set; } = ImageFormat.Png;
    public string ContentType => Format == ImageFormat.Jpeg ? OgcApiMediaTypes.Jpeg : OgcApiMediaTypes.Png;

    public static MapQuery Parse(
                IReadOnlyDictionary<string, string> parameters,
                string? accept,
                IReadOnlyList<CrsReference> supportedCrs,
                bool isDatasetMap)
    {
        var query = new MapQuery();

        foreach (var key in parameters.Keys)
        {
            if (!IgnoredParameters.Contains(key)
                && !KnownParameters.Contains(key)
                && !(isDatasetMap && key == "collections"))
            {
                throw OgcApiException.BadRequest($"Unknown query parameter '{key}'");
            }
        }

        query.Format = ParseFormat(parameters.TryGetValue("f", out var f) ? f : null, accept);

        if (parameters.TryGetValue("bbox", out var bbox))
        {
            query.BBox = ItemsQuery.ParseBBox(bbox);
        }
        if (parameters.TryGetValue("bbox-crs", out var bboxCrs))
        {
            query.BBoxCrs = ParseSupportedCrs(bboxCrs, supportedCrs, "bbox-crs");
        }
        if (parameters.TryGetValue("crs", out var crs))
        {
            query.Crs = ParseSupportedCrs(crs, supportedCrs, "crs");
        }

        query.Width = ParseSize(parameters, "width");
        query.Height = ParseSize(parameters, "height");

        if (parameters.TryGetValue("scale-denominator", out var scale))
        {
            query.ScaleDenominator = ParsePositive(scale, "scale-denominator");
        }
        if (parameters.TryGetValue("mm-per-pixel", out var mmPerPixel))
        {
            query.MmPerPixel = ParsePositive(mmPerPixel, "mm-per-pixel");
        }

        if (parameters.TryGetValue("transparent", out var transparent))
        {
            query.Transparent = transparent.ToLowerInvariant() switch
            {
                "true" => true,
                "false" => false,
                _ => throw OgcApiException.BadRequest("transparent must be true or false")
            };
        }
        if (parameters.TryGetValue("bgcolor", out var bgColor))
        {
            query.BackgroundColor = ParseColor(bgColor);
        }

        if (isDatasetMap && parameters.TryGetValue("collections", out var collections))
        {
            query.Collections = collections
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToArray();
        }

        return query;
    }

    /// <summary>
    /// The image size in pixels for the given extent. Missing width/height are calculated from the
    /// aspect ratio (or the scale denominator), the result is limited by the map service limits.
    /// </summary>
    /// <param name="extentWidth">width of the extent in CRS units</param>
    /// <param name="extentHeight">height of the extent in CRS units</param>
    /// <param name="metersPerUnit">null if the CRS units are not linear (scale-denominator not possible)</param>
    public (int width, int height) ImageSize(double extentWidth, double extentHeight, double? metersPerUnit, int maxWidth, int maxHeight)
    {
        double aspect = extentWidth > 0 && extentHeight > 0 ? extentWidth / extentHeight : 4.0 / 3.0;
        double width, height;

        if (ScaleDenominator.HasValue && !Width.HasValue && !Height.HasValue)
        {
            if (metersPerUnit is null)
            {
                throw OgcApiException.BadRequest("scale-denominator is not supported for geographic CRS");
            }
            double metersPerPixel = ScaleDenominator.Value * MmPerPixel / 1000.0;
            width = extentWidth * metersPerUnit.Value / metersPerPixel;
            height = extentHeight * metersPerUnit.Value / metersPerPixel;
        }
        else if (Width.HasValue && Height.HasValue)
        {
            (width, height) = (Width.Value, Height.Value);
        }
        else if (Width.HasValue)
        {
            (width, height) = (Width.Value, Width.Value / aspect);
        }
        else if (Height.HasValue)
        {
            (width, height) = (Height.Value * aspect, Height.Value);
        }
        else
        {
            // default: fit into 1024 x 768, keeping the aspect ratio
            (width, height) = aspect >= (double)DefaultWidth / DefaultHeight
                ? ((double)DefaultWidth, DefaultWidth / aspect)
                : (DefaultHeight * aspect, (double)DefaultHeight);
        }

        maxWidth = maxWidth > 0 ? Math.Min(maxWidth, AbsoluteMaxImageSize) : AbsoluteMaxImageSize;
        maxHeight = maxHeight > 0 ? Math.Min(maxHeight, AbsoluteMaxImageSize) : AbsoluteMaxImageSize;

        double factor = Math.Min(1.0, Math.Min(maxWidth / width, maxHeight / height));

        return (Math.Max(1, (int)Math.Round(width * factor)), Math.Max(1, (int)Math.Round(height * factor)));
    }

    #region Helper

    internal static ImageFormat ParseFormat(string? f, string? accept)
    {
        if (!String.IsNullOrEmpty(f))
        {
            return f.ToLowerInvariant() switch
            {
                "png" or "image/png" => ImageFormat.Png,
                "jpg" or "jpeg" or "image/jpeg" => ImageFormat.Jpeg,
                _ => throw OgcApiException.BadRequest($"Unsupported format f={f}")
            };
        }

        if (accept is not null
            && accept.Contains(OgcApiMediaTypes.Jpeg, StringComparison.OrdinalIgnoreCase)
            && !accept.Contains(OgcApiMediaTypes.Png, StringComparison.OrdinalIgnoreCase))
        {
            return ImageFormat.Jpeg;
        }

        return ImageFormat.Png;
    }

    internal static ArgbColor ParseColor(string value)
    {
        string hex = value.Trim();
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            hex = hex.Substring(2);
        }
        else if (hex.StartsWith('#'))
        {
            hex = hex.Substring(1);
        }

        if ((hex.Length == 6 || hex.Length == 8)
            && uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint argb))
        {
            return hex.Length == 6
                ? ArgbColor.FromArgb(255, (int)(argb >> 16) & 0xff, (int)(argb >> 8) & 0xff, (int)argb & 0xff)
                : ArgbColor.FromArgb((int)(argb >> 24) & 0xff, (int)(argb >> 16) & 0xff, (int)(argb >> 8) & 0xff, (int)argb & 0xff);
        }

        throw OgcApiException.BadRequest($"Invalid bgcolor '{value}' (0xRRGGBB expected)");
    }

    private static int? ParseSize(IReadOnlyDictionary<string, string> parameters, string name)
    {
        if (!parameters.TryGetValue(name, out var value))
        {
            return null;
        }
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int size) || size < 1)
        {
            throw OgcApiException.BadRequest($"{name} must be an integer >= 1");
        }
        return size;
    }

    private static double ParsePositive(string value, string name)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result)
            || !double.IsFinite(result) || result <= 0)
        {
            throw OgcApiException.BadRequest($"{name} must be a positive number");
        }
        return result;
    }

    private static CrsReference ParseSupportedCrs(string value, IReadOnlyList<CrsReference> supportedCrs, string parameterName)
    {
        if (!CrsReference.TryParse(value, out var crs) || !supportedCrs.Contains(crs))
        {
            throw OgcApiException.BadRequest($"Unsupported {parameterName}: {value}");
        }
        return crs;
    }

    #endregion
}

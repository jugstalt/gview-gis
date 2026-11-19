using gView.Framework.Core.Exceptions;
using System;

namespace gView.Interoperability.Extensions.Rendering;

/// <summary>
/// Limits of the image size and dpi of map requests (WMS, GeoServices, ArcXML, GeoJson, OGC API Maps)
/// </summary>
public static class ImageSizeLimits
{
    /// <summary>
    /// Hard limit for the width/height of a map image, even if a service or the mapserver-defaults allow more
    /// </summary>
    public const int AbsoluteMaxImageSize = 16384;

    /// <summary>
    /// Default for the max dpi, if neither the service nor mapserver-defaults:maxDpi sets one
    /// </summary>
    public const int DefaultMaxDpi = 600;

    public const double DefaultDpi = 96.0;

    /// <summary>
    /// The effective max image size: the configured value (0 = not set), but never more than <see cref="AbsoluteMaxImageSize"/>
    /// </summary>
    public static int EffectiveMaxImageSize(int configured)
        => configured > 0 ? Math.Min(configured, AbsoluteMaxImageSize) : AbsoluteMaxImageSize;

    /// <summary>
    /// The effective max dpi: the configured value (0 = not set => <see cref="DefaultMaxDpi"/>)
    /// </summary>
    public static double EffectiveMaxDpi(int configured)
        => configured > 0 ? configured : DefaultMaxDpi;

    /// <summary>
    /// Throws a <see cref="MapServerException"/> (400) if width or height are not positive
    /// </summary>
    public static void ThrowIfInvalidImageSize(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new MapServerException($"Invalid image size ({width},{height}): width and height must be greater than 0", 400);
        }
    }

    /// <summary>
    /// Fits an image request into the limits. The dpi is limited to <paramref name="maxDpi"/> first,
    /// then the image is scaled down (keeping the aspect ratio) if it is larger than the max size.
    /// The dpi is scaled down by the same factor, so the map shows the same content.
    /// </summary>
    /// <param name="maxWidth">configured max width, 0 = not set</param>
    /// <param name="maxHeight">configured max height, 0 = not set</param>
    /// <param name="maxDpi">configured max dpi, 0 = not set</param>
    /// <exception cref="MapServerException">width or height are not positive</exception>
    public static (int width, int height, double dpi, bool resized) Fit(
        int width, int height, double dpi,
        int maxWidth, int maxHeight, int maxDpi)
    {
        ThrowIfInvalidImageSize(width, height);

        if (double.IsNaN(dpi) || double.IsInfinity(dpi) || dpi <= 0.0)
        {
            dpi = DefaultDpi;
        }
        dpi = Math.Clamp(dpi, 1.0, EffectiveMaxDpi(maxDpi));

        maxWidth = EffectiveMaxImageSize(maxWidth);
        maxHeight = EffectiveMaxImageSize(maxHeight);

        double factor = Math.Min(1.0, Math.Min((double)maxWidth / width, (double)maxHeight / height));
        if (factor >= 1.0)
        {
            return (width, height, dpi, false);
        }

        return (
            Math.Clamp((int)(width * factor), 1, maxWidth),
            Math.Clamp((int)(height * factor), 1, maxHeight),
            dpi * factor,
            true);
    }
}

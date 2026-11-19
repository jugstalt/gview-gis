using gView.Framework.Core.Carto;
using gView.Interoperability.Extensions.Rendering;

namespace gView.Interoperability.Extensions;

static public class ServiceMapExtensions
{
    /// <summary>
    /// Fits Display.ImageWidth/ImageHeight/Dpi into the limits of the map service
    /// (see <see cref="ImageSizeLimits.Fit"/>)
    /// </summary>
    /// <returns>true, if the image was scaled down</returns>
    /// <exception cref="gView.Framework.Core.Exceptions.MapServerException">width or height are not positive</exception>
    static public bool ResizeImageSizeToMapServiceLimits(this IServiceMap map)
    {
        var properties = map.MapServiceProperties;

        var (width, height, dpi, resized) = ImageSizeLimits.Fit(
            map.Display.ImageWidth,
            map.Display.ImageHeight,
            map.Display.Dpi,
            properties?.MaxImageWidth ?? 0,
            properties?.MaxImageHeight ?? 0,
            properties?.MaxDpi ?? 0);

        map.Display.ImageWidth = width;
        map.Display.ImageHeight = height;
        map.Display.Dpi = dpi;

        return resized;
    }
}

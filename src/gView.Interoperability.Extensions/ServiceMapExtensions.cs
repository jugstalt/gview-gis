using gView.Framework.Core.Carto;
using System;

namespace gView.Interoperability.Extensions;

static public class ServiceMapExtensions
{
    static public bool ResizeImageSizeToMapServiceLimits(this IServiceMap map)
    {
        if ((map.MapServiceProperties.MaxImageWidth == 0 && map.MapServiceProperties.MaxImageHeight == 0)
            || map.Display.ImageWidth == 0 || map.Display.ImageHeight == 0)
        {
            return false;
        }

        bool resized = false;
        float ratio = (float)map.Display.ImageWidth / (float)map.Display.ImageHeight;

        if (map.Display.ImageWidth > map.MapServiceProperties.MaxImageWidth)
        {
            float diffRatio = (float)map.Display.ImageWidth / (float)map.MapServiceProperties.MaxImageWidth;

            map.Display.ImageWidth = map.MapServiceProperties.MaxImageWidth;
            map.Display.ImageHeight = (int)((float)map.Display.ImageWidth / ratio);
            map.Display.Dpi /= diffRatio;
            resized = true;
        }
        if (map.Display.ImageHeight > map.MapServiceProperties.MaxImageHeight)
        {
            float diffRatio = (float)map.Display.ImageHeight / (float)map.MapServiceProperties.MaxImageHeight;

            map.Display.ImageHeight = map.MapServiceProperties.MaxImageHeight;
            map.Display.ImageWidth = (int)((float)map.Display.ImageHeight * ratio);
            map.Display.Dpi /= diffRatio;
            resized = true;
        }

        return resized;
    }
}

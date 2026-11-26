using System;

namespace gView.Framework.Cartography.Extensions;

internal static class RasterOpacityExtensions
{
    extension(float layerOpacity)
    {
        /// <summary>
        /// <see cref="gView.Framework.Core.Data.IRasterLayer.Opacity"/> in percent (100 = opaque, 0 = not set, as in the
        /// Carto layer settings) as the 0..1 opacity the canvas draws with.
        /// </summary>
        internal float ToDrawingOpacity()
            => layerOpacity <= 0f
                ? 1f
                : Math.Min(layerOpacity / 100f, 1f);
    }
}

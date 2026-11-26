using gView.DataSources.VectorTileCache.Json.GLStyles;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Data;
using gView.Framework.Symbology.Vtc;
using gView.Framework.Symbology.Vtc.Extensions;

namespace gView.Framework.Vtc;

internal class VtcRasterLayer : RasterLayer, ILayerCloneBeforeRender
{
    private readonly PaintSymbol? _paintSymbol;

    /// <summary>
    /// For the persisted clone of the Carto layer settings dialog; the GL paint (raster-opacity) is not persisted,
    /// so such a layer renders with its own raster settings.
    /// </summary>
    public VtcRasterLayer()
    {
    }

    public VtcRasterLayer(IRasterClass rasterClass, PaintSymbol paintSymbol)
        : base(rasterClass)
    {
        _paintSymbol = paintSymbol;
    }

    private VtcRasterLayer(VtcRasterLayer layer)
        : base(layer)  // interpolation, opacity, transparent color, filter
    {
        this.Class = layer.Class;  // CopyFrom leaves the class out
        _paintSymbol = layer._paintSymbol;
    }

    public ILayer CloneBeforeRender(IDisplay display)
    {
        var rasterLayer = new VtcRasterLayer(this);

        // an opacity set in the layer settings wins over the style's raster-opacity
        if (rasterLayer.Opacity <= 0f && _paintSymbol is not null)
        {
            rasterLayer.Opacity = _paintSymbol.GetValueOrDeafult<float>(
                GLStyleProperties.RasterOpacity,
                1f,
                display,
                feature: null) * 100f;
        }

        return rasterLayer;
    }
}

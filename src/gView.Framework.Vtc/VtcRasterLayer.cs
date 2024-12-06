using gView.DataSources.VectorTileCache.Json.GLStyles;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Data;
using gView.Framework.Symbology.Vtc;
using gView.Framework.Symbology.Vtc.Extensions;

namespace gView.Framework.Vtc;

internal class VtcRasterLayer : RasterLayer, ILayerCloneBeforeRender
{
    private readonly PaintSymbol _paintSymbol;

    public VtcRasterLayer(IRasterClass rasterClass, PaintSymbol paintSymbol)
        : base(rasterClass)
    {
        _paintSymbol = paintSymbol;
    }

    public ILayer CloneBeforeRender(IDisplay display)
    {
        var rasterLayer = new VtcRasterLayer(this.RasterClass, _paintSymbol);

        rasterLayer.Opacity = _paintSymbol.GetValueOrDeafult<float>(
            GLStyleProperties.RasterOpacity,
            1f,
            display,
            feature: null) * 100f;

        return rasterLayer;
    }
}

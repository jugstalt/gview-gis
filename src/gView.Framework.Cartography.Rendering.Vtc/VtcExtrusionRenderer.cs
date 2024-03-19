using gView.DataSources.VectorTileCache.Json;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology.Vtc;

namespace gView.Framework.Cartography.Rendering.Vtc;

public class VtcExtrusionRenderer : ExtrusionRenderer
{
    private VtcStyleFilter? _filter = null;

    protected override ExtrusionRenderer CreateCloneInstance()
        => new VtcExtrusionRenderer();

    public override void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter)
        => _filter = layer.FilterQuery as VtcStyleFilter;

    public override void Draw(IDisplay disp, IFeature feature)
    {
        if (_filter == null || _filter.Filter(feature))
        {
            base.Draw(disp, feature);
        }
    }

    protected override void DrawGround(IDisplay disp, IFeature feature, ISymbol groundSymbol)
    {
        if(groundSymbol is PaintSymbol paintSymbol)
        {
            paintSymbol.Draw(disp, feature, false);
        } 
        else
        {
            base.DrawGround(disp, feature, groundSymbol);
        }
    }

    protected override void DrawElevated(IDisplay disp, IFeature feature, ISymbol symbol, IGeometry elevatedGeometry)
    {
        if (symbol is PaintSymbol paintSymbol)
        {
            var originalShape = feature.Shape;
            feature.Shape = elevatedGeometry;

            paintSymbol.Draw(disp, feature, true);

            feature.Shape = originalShape;
        }
        else
        {
            base.DrawElevated(disp, feature, symbol, elevatedGeometry);
        }
    }
}

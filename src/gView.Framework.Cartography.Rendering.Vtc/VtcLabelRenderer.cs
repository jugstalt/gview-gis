using gView.DataSources.VectorTileCache.Json;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Symbology;

namespace gView.Framework.Cartography.Rendering.Vtc;

public class VtcLabelRenderer : SimpleLabelRenderer
{
    private VtcStyleFilter? _filter = null;

    public VtcLabelRenderer() : base() { }

    protected VtcLabelRenderer(ITextSymbol? symbol, string fieldname)
        : base(symbol, fieldname) { }

    public override void PrepareQueryFilter(IDisplay display, IFeatureLayer layer, IQueryFilter filter)
    {
        base.PrepareQueryFilter(display, layer, filter);

        _filter = layer.FilterQuery as VtcStyleFilter;
    }

    protected override bool RenderFeature(IFeature feature)
        => _filter == null || _filter.Filter(feature);

    protected override SimpleLabelRenderer CreateCloneInstance(CloneOptions options)
    => new VtcLabelRenderer(
                (ITextSymbol?)(base.TextSymbol is IClone2 ? base.TextSymbol.Clone(options) : null),
                base.FieldName);
}

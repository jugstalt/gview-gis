using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;

namespace gView.Razor.EventArgs;

public class HighlightFeaturesEventArgs(
    ILayer layer,
    IQueryFilter? filter)
{
    public ILayer Layer { get; } = layer;
    public IQueryFilter? Filter { get; } = filter;
}

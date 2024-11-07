#nullable enable

using gView.Framework.Core.Data.Filters;

namespace gView.Framework.Core.Data
{
    public interface IFeatureHighlighting
    {
        IQueryFilter? FeatureHighlightFilter { get; set; }
    }
}
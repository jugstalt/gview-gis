#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;

namespace gView.Framework.Data
{
    public class FeatureHighlighting : FeatureLayer, IFeatureHighlighting
    {
        public FeatureHighlighting() { }
        public FeatureHighlighting(IFeatureClass featureClass)
            : base(featureClass)
        {
        }

        public FeatureHighlighting(IFeatureLayer layer)
            : base(layer)
        {
        }

        public IQueryFilter? FeatureHighlightFilter { get; set; }
    }
}

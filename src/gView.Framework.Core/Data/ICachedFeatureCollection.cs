using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using System;

namespace gView.Framework.Core.Data
{
    public interface ICachedFeatureCollection
    {
        void AddFeature(IFeature feature);

        Guid CollectionGUID { get; }
        IQueryFilter QueryFilter { get; }
        bool UsableWith(IQueryFilter filter);

        IFeatureCursor FeatureCursor();
        IFeatureCursor FeatureCursor(IQueryFilter filter);
    }
}
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataSources.GeoJson
{
    class GeoJsonFeatureCursor : FeatureCursor
    {
        private readonly IFeature[] _features;
        private readonly ISpatialFilter _spatialFilter;
        private readonly IQueryFilter _queryFilter;
        private int pos = 0;

        public GeoJsonFeatureCursor(
                        GeoJsonServiceFeatureClass fc, 
                        IEnumerable<IFeature> features, 
                        IQueryFilter filter
                    )
            : base(fc?.SpatialReference, filter?.FeatureSpatialReference)
        {
            _features = features?.ToArray();
            _queryFilter = filter;
            _spatialFilter = filter as ISpatialFilter;
        }

        #region IFeatureCursor

        override public void Dispose()
        {

        }

        override public Task<IFeature> NextFeature()
        {
            if (_features == null || _features.Length <= pos)
            {
                return Task.FromResult<IFeature>(null);
            }

            var feature = _features[pos++];

            // TODO: Check Filter WHERE
            if (_spatialFilter != null)
            {
                if (!gView.Framework.Geometry.SpatialRelation.Check(_spatialFilter, feature?.Shape))
                {
                    return NextFeature();
                }
            }

            //
            // always clone feature. It can be changed by the "client"
            // never give back the original feature from the Source =>
            // because it my be a cached feature
            //
            feature = base.CloneAndTransform(feature);
            
            return Task.FromResult<IFeature>(feature);
        }

        #endregion
    }
}

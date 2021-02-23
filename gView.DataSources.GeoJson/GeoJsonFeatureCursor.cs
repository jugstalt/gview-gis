using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.GeoJson
{
    class GeoJsonFeatureCursor : FeatureCursor
    {
        private readonly IFeature[] _features;
        private readonly ISpatialFilter _spatialFilter;
        private readonly IQueryFilter _queryFilter;
        private int pos = 0;

        public GeoJsonFeatureCursor(GeoJsonServiceFeatureClass fc, IEnumerable<IFeature> features, IQueryFilter filter)
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
                return Task.FromResult<IFeature>(null);

            var feature = _features[pos++];

            // ToDO: Check Filter WHERE
            if(_spatialFilter!=null)
            {
                if (!gView.Framework.Geometry.SpatialRelation.Check(_spatialFilter, feature?.Shape))
                {
                    return NextFeature();
                }
            }

            feature = base.CloneIfTransform(feature);
            return Task.FromResult<IFeature>(feature);
        }

        #endregion
    }
}

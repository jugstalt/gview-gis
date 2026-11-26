using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataSources.VectorTileCache
{
    class FeatureCursor : gView.Framework.Data.FeatureCursor
    {
        private readonly FeatureClass _fc;
        private readonly FeatureCache _cache;
        private readonly VectorTileFeature[] _features;
        private int _pos = 0;

        public FeatureCursor(FeatureClass fc, IQueryFilter filter)
            : base(fc, fc?.SpatialReference, filter?.FeatureSpatialReference, filter?.DatumTransformations)
        {
            _fc = fc;
            _cache = filter?.DatasetCachingContext?.GetCache<FeatureCache>();
            
            _features = _cache?[_fc.Name]?.ToArray();
        }

        #region IFeatureCursor

        public override void Dispose()
        {

        }

        public override Task<IFeature> NextFeature()
        {
            if (_features == null || _features.Length <= _pos)
            {
                return Task.FromResult<IFeature>(null);
            }

            var vectorTileFeature = _features[_pos++];

            var feature = new Feature();

            foreach (var attribute in vectorTileFeature.Attributes)
            {
                feature.Fields.Add(new FieldValue(attribute.Key, attribute.Value.Value));
            }

            feature.Shape = vectorTileFeature.Shape;

            // Filtering?
            // is managed in VtcFeatureRenderer...

            Transform(feature);
            return Task.FromResult<IFeature>(feature);
        }

        #endregion
    }
}

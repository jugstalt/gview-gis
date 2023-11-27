using System;
using System.Collections.Generic;
using gView.Framework.Core.Data;

namespace gView.Framework.Data
{
    public class FeatureBag : IDisposable
    {
        private Dictionary<int, IFeature> _features = new Dictionary<int, IFeature>();

        public void AddFeature(IFeature feature)
        {
            _features[feature.OID] = feature;
        }

        public IFeature GetFeature(int oid)
        {
            if (_features.TryGetValue(oid, out IFeature feature))
            {
                return feature;
            }

            return null;
        }

        public IEnumerable<IFeature> GetFeatures(IEnumerable<int> oids)
        {
            List<IFeature> features = new List<IFeature>();

            foreach (var oid in oids)
            {
                var feature = GetFeature(oid);
                if (feature != null)
                {
                    features.Add(feature);
                }
            }

            return features;
        }

        public void Dispose()
        {
            _features.Clear();
        }
    }
}

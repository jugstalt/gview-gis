using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataSources.GeoJson
{
    class GeoJsonDistinctFeatureCursor : IFeatureCursor
    {
        private readonly List<string> _values = new List<string>();
        private readonly DistinctFilter _filter;
        private readonly string _distinctField;
        private int pos = 0;

        public GeoJsonDistinctFeatureCursor(IEnumerable<IFeature> features, DistinctFilter filter)
        {
            _distinctField = filter.SubFields?.Trim().Replace(",", " ").Split(' ')[0];

            if (features != null && !String.IsNullOrEmpty(_distinctField))
            {
                foreach (var feature in features?.ToArray())
                {
                    var val = feature[_distinctField]?.ToString();
                    if (val != null && !_values.Contains(val))
                    {
                        _values.Add(val);
                    }
                }
            }

            _filter = filter;
        }

        #region IFeatureCursor

        public void Dispose()
        {

        }

        public Task<IFeature> NextFeature()
        {
            if (_values.Count <= pos)
            {
                return Task.FromResult<IFeature>(null);
            }

            var feature = new Feature();
            feature.Fields.Add(new FieldValue(_distinctField, _values[pos++]));

            return Task.FromResult<IFeature>(feature);
        }

        #endregion
    }
}
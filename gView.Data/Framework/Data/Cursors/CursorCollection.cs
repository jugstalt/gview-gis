using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Data.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Data.Cursors
{

    public class CursorCollection<T> : IFeatureCursor
    {
        private Dictionary<T, IFeatureClass> _fcs;
        private Dictionary<T, QueryFilter> _filters;
        private Dictionary<T, Dictionary<int, List<FieldValue>>> _additionalFields;
        private IFeatureCursor _cursor = null;
        private int index = 0;
        private List<T> _keys = new List<T>();

        public CursorCollection(Dictionary<T, IFeatureClass> fcs, Dictionary<T, QueryFilter> filters, Dictionary<T, Dictionary<int, List<FieldValue>>> additionalFields = null)
        {
            _fcs = fcs;
            _filters = filters;
            _additionalFields = additionalFields;

            if (_fcs != null && _filters != null)
            {
                foreach (T key in _fcs.Keys)
                {
                    if (_filters.ContainsKey(key))
                    {
                        _keys.Add(key);
                    }
                }
            }
        }

        #region IFeatureCursor Member

        async public Task<IFeature> NextFeature()
        {
            try
            {
                if (_cursor == null)
                {
                    if (index >= _keys.Count)
                    {
                        return null;
                    }

                    T key = _keys[index++];
                    _cursor = await _fcs[key].GetFeatures(_filters[key]);
                    if (_cursor == null)
                    {
                        return await NextFeature();
                    }
                }

                IFeature feature = await _cursor.NextFeature();
                if (feature == null)
                {
                    _cursor.Dispose();
                    _cursor = null;
                    return await NextFeature();
                }

                if (_fcs.ContainsKey(_keys[index - 1]))
                {
                    var fc = _fcs[_keys[index - 1]];
                    if (fc != null)
                    {
                        feature.Fields.Add(new FieldValue("_classname", fc.Name));
                    }
                }

                if (_additionalFields != null)
                {
                    var fcDictionary = _additionalFields[_keys[index - 1]];
                    if (fcDictionary != null && fcDictionary.ContainsKey(feature.OID) && fcDictionary[feature.OID] != null)
                    {
                        var fields = fcDictionary[feature.OID];
                        foreach (var fieldValue in fields)
                        {
                            feature.Fields.Add(fieldValue);
                        }
                    }
                }

                return feature;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            if (_cursor != null)
            {
                _cursor.Dispose();
                _cursor = null;
            }
        }

        #endregion
    }
}

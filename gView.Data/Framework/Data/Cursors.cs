using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Data
{
    public class SimpleRowCursor : IRowCursor
    {
        private List<IRow> _rows;
        private int _pos = 0;

        public SimpleRowCursor(List<IRow> rows)
        {
            _rows = rows;
        }
        #region IRowCursor Member

        public IRow NextRow
        {
            get
            {
                if (_rows == null || _pos >= _rows.Count)
                    return null;

                return _rows[_pos++];
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }

    public class SimpleFeatureCursor : IFeatureCursor
    {
        private List<IFeature> _features;
        private int _pos = 0;

        public SimpleFeatureCursor(List<IFeature> features)
        {
            _features = features;
        }

        #region IFeatureCursor Member

        public IFeature NextFeature
        {
            get
            {
                if (_features == null || _pos >= _features.Count)
                    return null;

                return _features[_pos++];
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }

    public class SimpleRasterlayerCursor : IRasterLayerCursor
    {
        private List<IRasterLayer> _layers;
        private int _pos = 0;

        public SimpleRasterlayerCursor(List<IRasterLayer> layers)
        {
            _layers = layers;
        }

        #region IRasterLayerCursor Member

        public IRasterLayer NextRasterLayer
        {
            get
            {
                if (_layers == null || _pos >= _layers.Count)
                    return null;

                return _layers[_pos++];
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }

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
                        _keys.Add(key);
                }
            }
        }

        #region IFeatureCursor Member

        public IFeature NextFeature
        {
            get
            {
                try
                {
                    if (_cursor == null)
                    {
                        if (index >= _keys.Count)
                            return null;

                        T key = _keys[index++];
                        _cursor = _fcs[key].GetFeatures(_filters[key]);
                        if (_cursor == null)
                            return NextFeature;
                    }

                    IFeature feature = _cursor.NextFeature;
                    if (feature == null)
                    {
                        _cursor.Dispose();
                        _cursor = null;
                        return NextFeature;
                    }

                    if (_fcs.ContainsKey(_keys[index-1]))
                    {
                        var fc = _fcs[_keys[index-1]];
                        if (fc != null)
                            feature.Fields.Add(new FieldValue("_classname", fc.Name));
                    }

                    if (_additionalFields != null)
                    {
                        var fcDictionary = _additionalFields[_keys[index-1]];
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

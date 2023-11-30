using System;
using System.Collections.Generic;

namespace gView.Framework.Common
{
    public class DisposableDictionary<TKey, TValue> : IDisposable
        where TValue : IDisposable
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        public DisposableDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public TValue this[TKey key]
        {
            get
            {
                if (_dictionary.ContainsKey(key))
                {
                    return _dictionary[key];
                }

                return default;
            }
        }

        public void AddIfNotExists(TKey key, Func<TKey, TValue> func)
        {
            if (!_dictionary.ContainsKey(key))
            {
                _dictionary.Add(key, func(key));
            }
        }

        public void Dispose()
        {
            foreach (var value in _dictionary.Values)
            {
                if (value != null)
                {
                    value.Dispose();
                }
            }

            _dictionary.Clear();
        }
    }
}

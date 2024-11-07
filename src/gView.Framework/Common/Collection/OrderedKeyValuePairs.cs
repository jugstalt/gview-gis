#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Common.Collection;
public class OrderedKeyValuePairs<TKey, TValue>
{
    private readonly List<MutableKeyValuePair<TKey, TValue>> _list;
    private readonly HashSet<TKey> _keyHashSet;

    public OrderedKeyValuePairs()
    {
        _list = new();
        _keyHashSet = new();
    }

    public IEnumerable<TKey> Keys =>
        _list.Select(kv => kv.Key);

    public bool ContainsKey(TKey key)
        => _keyHashSet.Contains(key);

    public IEnumerable<TValue> Values =>
        _list.Select(kv => kv.Value);

    public void Clear()
    {
        _list.Clear();
        _keyHashSet.Clear();
    }

    public int Count => _list.Count;

    public TValue? this[TKey key]
    {
        get
        {
            var item = GetItemOrNull(key);
            if (item == null)
            {
                return default;
            }

            return item.Value;
        }
        set
        {
            AddOrSet(key, value);
        }
    }

    public void AddOrSet(TKey key, TValue? value)
    {
        var kv = GetItemOrNull(key);

        if (kv is null)
        {
            _list.Add(new MutableKeyValuePair<TKey, TValue>(key, value ?? default!));
            _keyHashSet.Add(key);
        }
        else
        {
            kv.Value = value ?? default!;
        }
    }

    public void Add(TKey key, TValue value)
    {
        var kv = GetItemOrNull(key);

        if (kv is not null)
        {
            throw new ArgumentException($"An item with the same key has already been added. Key: {key}");
        }

        _list.Add(new MutableKeyValuePair<TKey, TValue>(key, value));
        _keyHashSet.Add(key);
    }

    public bool TryGetValue(TKey key, out TValue? value)
    {
        if (!_keyHashSet.Contains(key))
        {
            value = default;
            return false;
        }

        var kv = _list.First(kv => kv.Key?.Equals(key) == true);

        value = kv.Value;
        return true;
    }

    public bool Remove(TKey key)
    {
        var kv = GetItemOrNull(key);

        if (kv is null)
        {
            return false;
        }

        _keyHashSet.Remove(key);
        return _list.Remove(kv!);
    }

    public TValue ValueAt(int index) => _list[index].Value;

    public TValue? ValueAtOrDefault(int index, TValue? defaultValue = default(TValue))
    {
        if (index < 0 || index >= _list.Count)
        {
            return defaultValue;
        }

        return _list[index].Value;
    }

    #region Helper

    private MutableKeyValuePair<TKey, TValue>? GetItemOrNull(TKey key)
    {
        if(!_keyHashSet.Contains(key))
        {
            return null;

        }

        var item = _list.FirstOrDefault(k => k.Key?.Equals(key) == true);

        return item;
    }

    #endregion
}

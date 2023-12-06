#nullable enable

using System;
using System.Collections.Generic;

namespace gView.Framework.Common.Extensions;
static public class DictionaryExtensions
{
    static public TResult? IfKeyExists<TKey, TValue, TResult>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        Func<TValue, TResult> action,
        TResult? defaultResult = default)
        => dict?.ContainsKey(key) == true ? action(dict[key]) : defaultResult;

    static public void RemoveIfExists<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        Action<TValue?>? removeItem = null
        )
    {
        if (dict is not null && dict.ContainsKey(key))
        {
            dict.Remove(key, out TValue? value);

            removeItem?.Invoke(value);
        }
    }

    static public void ForEach<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        Action<TKey, TValue> action)
    {
        if (dict is null)
        {
            return;
        }

        foreach (var key in dict.Keys)
        {
            action(key, dict[key]);
        }
    }
}

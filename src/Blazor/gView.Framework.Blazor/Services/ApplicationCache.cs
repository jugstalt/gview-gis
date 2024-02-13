using gView.Framework.Blazor.Services.Abstraction;
using System;
using System.Collections.Concurrent;

namespace gView.Framework.Blazor.Services;
public class ApplicationCache : IApplicationCache
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public T? GetCacheItem<T>(string key) where T : class
        => _cache.ContainsKey(key)
            ? _cache[key] as T
            : null;
            

    public void SetCacheItem(string key, object value)
    {
        _cache[key] = value;
    }
}

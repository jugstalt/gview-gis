using gView.Framework.Blazor.Models;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Blazor.Services.Abstraction;

public interface IApplicationScope : IApplicationBusyHandling, IApplicationCache, IDisposable
{
    Task<T?> ShowModalDialog<T>(Type razorComponent,
                                string title,
                                T? model = default(T),
                                ModalDialogOptions? modalDialogOptions = null);

    Task<T?> ShowKnownDialog<T>(KnownDialogs dialog,
                                             string? title = null,
                                             T? model = default);
}

public interface IApplicationCache
{
    void SetCacheItem(string key, object value);
    T? GetCacheItem<T>(string key) where T : class; 
}

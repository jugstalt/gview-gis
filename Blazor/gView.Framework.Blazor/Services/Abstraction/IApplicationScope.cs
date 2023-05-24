using System;
using System.Threading.Tasks;

namespace gView.Framework.Blazor.Services.Abstraction;

public interface IApplicationScope : IDisposable
{
    Task<T?> ShowModalDialog<T>(Type razorComponent,
                                string title,
                                T? model = default(T));

    Task<T?> ShowKnownDialog<T>(KnownDialogs dialog,
                                             string? title = null,
                                             T? model = default);

    Task<IAsyncDisposable> RegisterBusyTaskAsync(string task);
}

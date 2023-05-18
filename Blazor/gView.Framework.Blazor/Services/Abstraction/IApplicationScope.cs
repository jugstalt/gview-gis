using System;
using System.Threading.Tasks;

namespace gView.Framework.Blazor.Services.Abstraction;

public interface IApplicationScope : IDisposable
{
    Task<T?> ShowModalDialog<T>(Type razorComponent,
                                string title,
                                T? model = default(T));
}

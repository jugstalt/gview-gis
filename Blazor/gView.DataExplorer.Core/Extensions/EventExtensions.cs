using gView.Framework.DataExplorer.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataExplorer.Core.Extensions;

internal static class EventExtensions
{
    async static public Task FireAsync<T>(this Func<T, Task> eventFunction, T eventArg)
    {
        if (eventFunction != null)
        {
            foreach(var handler in eventFunction.GetInvocationList()
                                                .OfType<Func<T, Task>>())
            {
                await handler.Invoke(eventArg);
            }
        }
    }
}

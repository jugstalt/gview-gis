using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace gView.Blazor.Core.Exceptions;
internal class MapRendererNotIntializedException : Exception
{
    static public void ThrowIfNull(object? obj)
    {
        if(obj == null) throw new MapRendererNotIntializedException();
    }
}

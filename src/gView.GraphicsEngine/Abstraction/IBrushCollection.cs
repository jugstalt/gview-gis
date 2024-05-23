using System;
using System.Collections.Generic;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IBrushCollection : IDisposable
    {
        IEnumerable<IBrush> Brushes { get; }
    }
}

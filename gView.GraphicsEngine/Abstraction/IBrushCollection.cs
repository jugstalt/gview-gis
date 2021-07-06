using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IBrushCollection : IDisposable
    {
        IEnumerable<IBrush> Brushes { get; }
    }
}

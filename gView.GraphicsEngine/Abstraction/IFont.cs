using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IFont : IDisposable 
    {
        string Name { get; }

        float Size { get; }

        FontStyle Style { get; }

        GraphicsUnit Unit { get; }

        object EngineElement { get; }
    }
}

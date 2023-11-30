using System;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IBrush : IDisposable
    {
        ArgbColor Color { get; set; }

        object EngineElement { get; }
    }
}

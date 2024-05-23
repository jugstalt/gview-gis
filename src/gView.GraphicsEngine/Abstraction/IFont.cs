using gView.GraphicsEngine.Threading;
using System;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IFont : IDisposable
    {
        string Name { get; }

        float Size { get; }

        FontStyle Style { get; }

        GraphicsUnit Unit { get; }

        object EngineElement { get; }

        IThreadLocker LockObject { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IPen : IDisposable
    {
        ArgbColor Color { get; set; }

        float Width { get; set; }

        LineDashStyle DashStyle { get; set; }
        LineCap StartCap { get; set; }
        LineCap EndCap { get; set; }
        LineJoin LineJoin { get; set; }

        object EngineElement { get; }
    }
}

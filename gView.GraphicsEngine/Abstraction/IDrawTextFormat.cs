using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IDrawTextFormat
    {
        StringAlignment Alignment { get; set; }
        StringAlignment LineAlignment { get; set; }

        object EngineElement { get; }
    }
}

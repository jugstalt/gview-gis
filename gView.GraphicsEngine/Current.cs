using gView.GraphicsEngine.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine
{
    static public class Current
    {
        static public IGraphicsEngine Engine { get; set; }
    }
}

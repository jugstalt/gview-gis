using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IBitmap : IDisposable
    {
        ICanvas CreateCanvas();

        void Save(string filename, ImageFormat format, int quality = 0);
        void Save(Stream stream, ImageFormat format, int quality = 0);
    }
}

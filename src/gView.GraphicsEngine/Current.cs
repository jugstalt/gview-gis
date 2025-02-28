using gView.GraphicsEngine.Abstraction;
using System;

namespace gView.GraphicsEngine
{
    static public class Current
    {
        static public IGraphicsEngine Engine { get; set; }
        static public IBitmapEncoding Encoder { get; set; }

        static public bool UseSecureDisposingOnUserInteractiveUIs = false;

        static public int DefaultExportQuality { get; private set; } = 75;
        static public void SetDefaultExportQuality(int quality)
        {
            DefaultExportQuality = Math.Max(0, quality <= 0 ? 75 : Math.Min(quality, 100));
        }
    }
}

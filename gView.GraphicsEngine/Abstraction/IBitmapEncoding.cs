using System.IO;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IBitmapEncoding
    {
        string EngineName { get; }

        bool CanEncode(IBitmap bitmap);

        void Encode(IBitmap bitmap, string filename, ImageFormat format, int quality = 0);

        void Encode(IBitmap bitmap, Stream stream, ImageFormat format, int quality = 0);
    }
}

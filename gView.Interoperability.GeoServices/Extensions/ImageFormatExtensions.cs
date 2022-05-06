using gView.Interoperability.GeoServices.Rest.Json;

namespace gView.Interoperability.GeoServices.Extensions
{
    static internal class ImageFormatExtensions
    {
        static public GraphicsEngine.ImageFormat ToGraphicsEngineImageFormat(this ImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case ImageFormat.png:
                case ImageFormat.png24:
                case ImageFormat.png32:
                    return GraphicsEngine.ImageFormat.Png;
                case ImageFormat.jpg:
                    return GraphicsEngine.ImageFormat.Jpeg;
                case ImageFormat.webp:
                    return GraphicsEngine.ImageFormat.Webp;
                case ImageFormat.bmp:
                    return GraphicsEngine.ImageFormat.Bmp;
                case ImageFormat.gif:
                    return GraphicsEngine.ImageFormat.Gif;
                case ImageFormat.astc:
                    return GraphicsEngine.ImageFormat.Astc;
                case ImageFormat.dng:
                    return GraphicsEngine.ImageFormat.Dng;
                case ImageFormat.heif:
                    return GraphicsEngine.ImageFormat.Heif;
                case ImageFormat.ico:
                    return GraphicsEngine.ImageFormat.Ico;
                case ImageFormat.ktx:
                    return GraphicsEngine.ImageFormat.Ktx;
                case ImageFormat.pkm:
                    return GraphicsEngine.ImageFormat.Pkm;
                case ImageFormat.wbmp:
                    return GraphicsEngine.ImageFormat.Wbmp;
                 default:
                    throw new System.Exception($"Unknown GraphicsEngine ImageFormat {imageFormat}");
            }
        }
    }
}

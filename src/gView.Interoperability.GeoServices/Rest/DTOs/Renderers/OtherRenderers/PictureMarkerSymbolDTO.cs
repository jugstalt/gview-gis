using gView.Framework.Core.Symbology;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.Renderers.OtherRenderers
{
    class PictureMarkerSymbolDTO
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("imageData")]
        public string ImageData { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("angle")]
        public float Angle { get; set; }

        [JsonPropertyName("xoffset")]
        public float Xoffset { get; set; }

        [JsonPropertyName("yoffset")]
        public float Yoffset { get; set; }

        private const int MaxImageBytes = 512 * 1024;

        /// <summary>
        /// Creates a raster marker symbol from the embedded imageData (base64 png/jpeg/gif).
        /// Remote urls are intentionally not supported (the server would fetch arbitrary urls).
        /// </summary>
        static public ISymbol ToSymbol(PictureMarkerSymbolDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ImageData))
            {
                return null;
            }

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(dto.ImageData);
            }
            catch (FormatException)
            {
                return null;
            }

            if (bytes.Length == 0 || bytes.Length > MaxImageBytes)
            {
                return null;
            }

            var extension = ImageExtension(bytes);
            if (extension == null)
            {
                return null;
            }

            var directory = Path.Combine(Path.GetTempPath(), "gview-ags-symbols");
            Directory.CreateDirectory(directory);

            var filename = Path.Combine(directory, Convert.ToHexString(SHA256.HashData(bytes)) + extension);
            if (!File.Exists(filename))
            {
                File.WriteAllBytes(filename, bytes);
            }

            return new Framework.Symbology.RasterMarkerSymbol()
            {
                Filename = filename,
                // ArcGIS: width/height in points
                SizeX = (float)(dto.Width > 0 ? dto.Width : 10),
                SizeY = (float)(dto.Height > 0 ? dto.Height : 10),
                Angle = dto.Angle,
                HorizontalOffset = dto.Xoffset,
                VerticalOffset = dto.Yoffset
            };
        }

        static private string ImageExtension(byte[] b)
        {
            if (b.Length > 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47)
            {
                return ".png";
            }

            if (b.Length > 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            {
                return ".jpg";
            }

            if (b.Length > 6 && b[0] == 'G' && b[1] == 'I' && b[2] == 'F' && b[3] == '8')
            {
                return ".gif";
            }

            return null;
        }
    }
}

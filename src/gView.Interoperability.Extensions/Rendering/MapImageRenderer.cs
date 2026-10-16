using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.GraphicsEngine;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.Interoperability.Extensions.Rendering;

/// <summary>
/// A map image request in the coordinates of <see cref="SpatialReference"/> (gView x/y order)
/// </summary>
public class MapImageRequest
{
    public ISpatialReference SpatialReference { get; set; }
    public IEnvelope BBox { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double Dpi { get; set; } = 96.0;
    public bool Transparent { get; set; }
    public ArgbColor BackgroundColor { get; set; } = ArgbColor.White;
    public ImageFormat Format { get; set; } = ImageFormat.Png;

    /// <summary>
    /// Optional: set layer visibility, renderers, ... before the layers are drawn
    /// </summary>
    public BeforeRenderLayersEvent BeforeRenderLayers { get; set; }
}

/// <summary>
/// Renders the image of a service map. Shared by WMS GetMap and OGC API - Maps.
/// </summary>
public static class MapImageRenderer
{
    /// <returns>the encoded image or null, if nothing was rendered</returns>
    async public static Task<byte[]> RenderImageAsync(this IServiceMap map, MapImageRequest request)
    {
        map.Display.SpatialReference = request.SpatialReference;

        map.Display.Dpi = request.Dpi;
        map.Display.ImageWidth = request.Width;
        map.Display.ImageHeight = request.Height;
        map.ResizeImageSizeToMapServiceLimits();

        map.Display.Limit = request.BBox;
        map.Display.ZoomTo(request.BBox);

        if (request.Transparent)
        {
            map.Display.MakeTransparent = true;
            map.Display.TransparentColor = ArgbColor.White;
        }
        else
        {
            map.Display.MakeTransparent = false;
            map.Display.BackgroundColor = request.BackgroundColor;
        }

        if (request.BeforeRenderLayers != null)
        {
            map.BeforeRenderLayers += request.BeforeRenderLayers;
        }

        await map.Render();

        if (map.MapImage == null)
        {
            return null;
        }

        //
        // Zurück in das richtige Koordinatenfenster verzerren
        //
        double minx = request.BBox.MinX, miny = request.BBox.MinY,
               maxx = request.BBox.MaxX, maxy = request.BBox.MaxY;

        map.Display.World2Image(ref minx, ref miny);
        map.Display.World2Image(ref maxx, ref maxy);

        if ((int)Math.Round(minx) != 0 || (int)Math.Round(maxy) != 0 ||
            (int)Math.Round(maxx) != request.Width || (int)Math.Round(miny) != request.Height)
        {
            using (var bitmap = Current.Engine.CreateBitmap(request.Width, request.Height))
            {
                using (var canvas = bitmap.CreateCanvas())
                {
                    var sourceRect = new CanvasRectangleF((float)minx, (float)maxy, (float)Math.Abs(maxx - minx), (float)Math.Abs(miny - maxy));
                    canvas.DrawBitmap(map.MapImage,
                        new CanvasRectangleF(0f, 0f, bitmap.Width, bitmap.Height),
                        sourceRect);
                }

                if (request.Transparent)
                {
                    bitmap.MakeTransparent(ArgbColor.White);
                }

                var ms = new MemoryStream();
                bitmap.Save(ms, request.Format);
                return ms.ToArray();
            }
        }
        else
        {
            var ms = new MemoryStream();
            if (await map.SaveImage(ms, request.Format) >= 0)
            {
                return ms.ToArray();
            }
        }

        return null;
    }
}

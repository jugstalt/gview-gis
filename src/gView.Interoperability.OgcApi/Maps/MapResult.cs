using System.Globalization;

namespace gView.Interoperability.OgcApi.Maps;

/// <summary>
/// A rendered map image with the information for the Content-Crs and Content-Bbox headers
/// </summary>
public sealed class MapResult
{
    public MapResult(byte[] image, string contentType, CrsReference crs, double[] contentBbox)
    {
        Image = image;
        ContentType = contentType;
        Crs = crs;
        ContentBbox = contentBbox;
    }

    public byte[] Image { get; }
    public string ContentType { get; }
    public CrsReference Crs { get; }

    /// <summary>
    /// The extent of the image in the axis order of <see cref="Crs"/>
    /// </summary>
    public double[] ContentBbox { get; }

    public string ContentBboxHeader
        => String.Join(",", ContentBbox.Select(v => v.ToString("R", CultureInfo.InvariantCulture)));
}

namespace gView.Blazor.Core.Models;

public class MapControlBackgroundTilesModel
{
    public string Name { get; set; } = "";
    public int Epsg { get; set; }
    public string UrlTemplate { get; set; } = "";
    public string Attribution { get; set; } = "";
    public float Opacity { get; set; }

    public int[] TileSize { get; set; } = [256, 256];

    public float MinZoom { get; set; }
    public float MaxZoom { get; set; }
    public float MaxNativeZoom { get; set; }
}

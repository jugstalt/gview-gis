using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Models.Json;

internal class GLSprite
{
    public int Width { get; set; }
    public int Height { get; set; }

    public int X { get; set; }
    public int Y { get; set; }

    public float PixelRatio { get; set; }
}

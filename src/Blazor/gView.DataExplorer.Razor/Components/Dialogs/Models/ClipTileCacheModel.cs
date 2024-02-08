using gView.Blazor.Models.Dialogs;
using gView.Cmd.TileCache.Lib;
using gView.Framework.Core.Data;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;

public class ClipTileCacheModel : IDialogResultItem
{
    public TileCacheClipType ClipType { get; set; }

    public string SourceCache { get; set; } = "";
    public int MaxLevel { get; set; } = -1;

    public string TargetCache { get; set;  } = "";
    public int JpegQuality { get; set; } = -1;

    public IFeatureClass? Clipper { get; set; }
    public string ClipperDefinitionQuery { get; set; } = "";
}

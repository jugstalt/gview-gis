using gView.Blazor.Models.Dialogs;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;

public class ImportVectorTileGLStylesModel : IDialogResultItem
{
    public string GLStylesJsonUrl { get; set; } = "";
    public string TargetFolder { get; set; } = "";
    public string MapName { get; set; } = "";
}

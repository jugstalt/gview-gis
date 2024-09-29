using gView.Blazor.Models.Dialogs;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class RestoreMapDocumentDialogModel : IDialogResultItem
{
    public string? MxlFilePath { get; set; }

    public string? RestorePointHash { get; set; }   
}

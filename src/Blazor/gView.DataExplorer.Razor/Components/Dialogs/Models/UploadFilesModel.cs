using gView.Blazor.Models.Dialogs;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;
public class UploadFilesModel : IDialogResultItem
{
    public string TargetFolder { get; set; } = "";
}

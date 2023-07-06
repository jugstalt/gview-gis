using gView.Blazor.Models.Dialogs;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;

public class NewFdbDatasetModel : IDialogResultItem
{
    public string Name { get; set; } = string.Empty;
}

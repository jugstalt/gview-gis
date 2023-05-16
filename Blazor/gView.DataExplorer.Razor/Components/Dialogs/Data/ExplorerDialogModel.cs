using gView.DataExplorer.Razor.Components.Dialogs.Filters;

namespace gView.DataExplorer.Razor.Components.Dialogs.Data;

public class ExplorerDialogModel
{
    public required List<ExplorerDialogFilter> Filters { get; set; }
    public bool Open { get; set; }
}

using gView.Blazor.Models.Dialogs;
using gView.Cmd.Core.Abstraction;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;

public class ExecuteCommandModel : IDialogResultItem
{
    public required ICommand Command { get; init; }

    public required IDictionary<string, object> Parameters { get; init; }

    public bool Result { get; set; }
}

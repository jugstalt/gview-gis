using gView.Blazor.Models.Dialogs;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;
public class SelectMapServerServiceModel : IDialogResultItem
{
    public string Server { get; set; } = "";
    public string Service { get; set; } = "";

    public string? Username { get; set; }
    public string? Password { get; set; }
}

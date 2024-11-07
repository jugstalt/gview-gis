using gView.Blazor.Models.Dialogs;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;

public class ElasticSearchToolModel : IDialogResultItem
{
    public string JsonFile { get; set; } = "";

    public string BasicAuthenticationUser { get; set; } = "";
    public string BasicAuthenticationPassword { get; set; } = "";

    public string ProxyUrl { get; set; } = "";
    public string ProxyUsername { get; set; } = "";
    public string ProxyPassword { get; set; } = "";
}

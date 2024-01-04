using gView.Blazor.Models.Dialogs;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class PromptDialogModel : IDialogResultItem
{
    public string Prompt { get; set; } = "Input Value";
    public string HelperText { get; set; } = "";
    public string Value { get; set; } = "";
    public bool Required { get; set; } = false;
}

using gView.Blazor.Models.Dialogs;

namespace gView.Razor.Dialogs.Models;

public class PromptSelectDialogModel<T>
        : IDialogResultItem
{
    public string Prompt { get; set; } = "Input Value";
    public string HelperText { get; set; } = "";

    public bool Required { get; set; } = false;

    public IEnumerable<KeyValuePair<string, T>>? Options { get; set; }
    public T? SelectedValue { get; set; }
}

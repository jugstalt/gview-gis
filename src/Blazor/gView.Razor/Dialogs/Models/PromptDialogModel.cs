using gView.Blazor.Models.Dialogs;

namespace gView.Razor.Dialogs.Models;

public class PromptDialogModel<T> : IDialogResultItem
{
    public string Prompt { get; set; } = "Input Value";
    public string HelperText { get; set; } = "";
    public T? Value { get; set; } = default(T);
    public bool Required { get; set; } = false;
}

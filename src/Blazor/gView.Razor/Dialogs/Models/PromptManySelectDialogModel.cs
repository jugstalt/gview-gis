using gView.Blazor.Models.Dialogs;

namespace gView.Razor.Dialogs.Models;
public class PromptManySelectDialogModel<T> : IDialogResultItem
{
    public IEnumerable<PromptSelectDialogModel<T>>? Prompts { get; set; }
}

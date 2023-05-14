using gView.Blazor.Models.Dialogs;
using gView.Blazor.Models.Extensions;
using Microsoft.AspNetCore.Components;

namespace gView.Razor.Base;

public class ModalDialogFormBase<T> : ModalDialogForm
    where T : IDialogResultItem
{
    [Parameter] public EventCallback<gView.Blazor.Models.Dialogs.DialogResult> OnDialogClose { get; set; }
    [Parameter] public T Model { get; set; } = Activator.CreateInstance<T>();

    async override protected Task Submit()
    {
        if (_form != null)
        {
            await _form.Validate();

            if (_form.IsValid == true)
            {
                await OnDialogClose.InvokeAsync(Model.ToResult());
            }
        }
    }

    override protected Task Close() => OnDialogClose.InvokeAsync(null);
}

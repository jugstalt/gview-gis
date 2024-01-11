using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Models;

namespace gView.Carto.Plugins.Services.Dialogs;

internal class PromptBoolDialogService : IKnownDialogService
{
    public KnownDialogs Dialog => KnownDialogs.PromptBoolDialog;

    public Type RazorType => typeof(Razor.Components.Dialogs.PromptBoolDialog);

    public string Title => "Prompt";

    public ModalDialogOptions? DialogOptions => new ModalDialogOptions()
    {
        Width = ModalDialogWidth.ExtraSmall,
    };
}

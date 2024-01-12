using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Models;

namespace gView.Carto.Plugins.Services.Dialogs;

internal class PromptDialogService : IKnownDialogService
{
    public KnownDialogs Dialog => KnownDialogs.PromptDialog;

    public Type RazorType => typeof(gView.Razor.Dialogs.PromptDialog);

    public string Title => "Prompt";

    public ModalDialogOptions? DialogOptions => new ModalDialogOptions()
    {
        Width = ModalDialogWidth.ExtraSmall,
    };
}

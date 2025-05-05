using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Models;
using System;

namespace gView.DataExplorer.Plugins.Services.Dialogs;

public class DatumTransformationSelectorDialogService : IKnownDialogService
{
    public KnownDialogs Dialog => KnownDialogs.DatumTransformationDialog;

    public Type RazorType => typeof(gView.Razor.Dialogs.DatumTransformationDialog);

    public string Title => "Datum Transformation";

    public ModalDialogOptions? DialogOptions => null;
}

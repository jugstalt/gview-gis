using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Models;
using System;

namespace gView.DataExplorer.Plugins.Services.Dialogs;

internal class GeographicsDatumAndGridShiftSelectorDialogService : IKnownDialogService
{
    public KnownDialogs Dialog => KnownDialogs.GeographicDatumAndGridShiftSelectorDialog;

    public Type RazorType => typeof(gView.DataExplorer.Razor.Components.Dialogs.GeographicDatumAndGridShiftDialog);

    public string Title => "Geographic Datum";

    public ModalDialogOptions? DialogOptions => null;
}
using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Models;
using System;

namespace gView.DataExplorer.Plugins.Services.Dialogs;

internal class GeographicDatumNonGeoCentricDialogService : IKnownDialogService
{
    public KnownDialogs Dialog => KnownDialogs.GeographicDatumNonGeoCentricDialog;

    public Type RazorType => typeof(gView.DataExplorer.Razor.Components.Dialogs.GeoGraphicDatumNonGeoCentricDialog);

    public string Title => "Geographic Datum (non geocentric)";

    public ModalDialogOptions? DialogOptions => null;
}
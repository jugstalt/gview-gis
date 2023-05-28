﻿using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.Blazor;
using System;

namespace gView.DataExplorer.Plugins.Services.Dialogs
{
    internal class GeographicsDatumSelectorDialogService : IKnownDialogService
    {
        public KnownDialogs Dialog => KnownDialogs.GeographicDatumSelectorDialog;

        public Type RazorType => typeof(gView.DataExplorer.Razor.Components.Dialogs.GeograpphicDatumDialog);

        public string Title => "Geographic Datum";
    }
}
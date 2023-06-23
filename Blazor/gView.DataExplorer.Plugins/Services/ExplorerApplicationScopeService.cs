using gView.Blazor.Core.Exceptions;
using gView.Blazor.Core.Services.Abstraction;
using gView.DataExplorer.Core.Services;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Models;
using gView.Framework.Blazor.Services;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.Services;

public class ExplorerApplicationScopeService : ApplictionBusyHandler, IApplicationScope
{
    private readonly IDialogService _dialogService;
    IEnumerable<IKnownDialogService> _knownDialogs;
    private readonly EventBusService _eventBus;
    private readonly IJSRuntime _jsRuntime;
    private readonly ISnackbar _snackbar;

    public ExplorerApplicationScopeService(IDialogService dialogService,
                                           IEnumerable<IKnownDialogService> knownDialogs,
                                           EventBusService eventBus,
                                           IJSRuntime jsRuntime,
                                           ISnackbar snackbar)
    {
        _dialogService = dialogService;
        _knownDialogs = knownDialogs;
        _eventBus = eventBus;
        _jsRuntime = jsRuntime;

        _eventBus.OnCurrentExplorerObjectChanged += EventBus_OnTreeItemClickAsync;
        _eventBus.OnContextExplorerObjectsChanged += EventBus_OnContextExplorerObjectsChanged;
        _snackbar = snackbar;
    }



    #region IDisposable

    public void Dispose()
    {
        _eventBus.OnCurrentExplorerObjectChanged -= EventBus_OnTreeItemClickAsync;
        _eventBus.OnContextExplorerObjectsChanged -= EventBus_OnContextExplorerObjectsChanged;
    }

    #endregion

    private Task EventBus_OnTreeItemClickAsync(IExplorerObject? arg)
    {
        CurrentExplorerObject = arg;

        return Task.CompletedTask;
    }

    private Task EventBus_OnContextExplorerObjectsChanged(IEnumerable<IExplorerObject>? arg)
    {
        ContextExplorerObjects = arg;

        return Task.CompletedTask;
    }

    public IExplorerObject? CurrentExplorerObject { get; private set; }
    public IEnumerable<IExplorerObject>? ContextExplorerObjects { get; private set; }

    public EventBusService EventBus => _eventBus;

    async public Task<T?> ShowModalDialog<T>(Type razorComponent,
                                             string title,
                                             T? model = default(T),
                                             ModalDialogOptions? modalDialogOptions = null)
    {
        IDialogReference? dialog = null;
        T? result = default(T);

        var dialogParameters = new DialogParameters
        {
            { "OnDialogClose", new EventCallback<Blazor.Models.Dialogs.DialogResult>(null, OnClose) },
            //{ "OnClose", new EventCallback<Blazor.Models.Dialogs.DialogResult>(null, OnClose) }
        };

        if (model != null)
        {
            dialogParameters.Add("Model", model);
        }

        var dialogOptions = new DialogOptions()
        {
            DisableBackdropClick = true,
            CloseButton = modalDialogOptions?.ShowCloseButton ?? true,
            MaxWidth = MaxWidth.ExtraExtraLarge,
            CloseOnEscapeKey = modalDialogOptions?.CloseOnEscapeKey ?? false,
            //FullWidth = true
        };

        dialog = await _dialogService.ShowAsync(razorComponent, title, dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;

        if (dialogResult != null && !dialogResult.Canceled)
        {
            return result;
        }

        return default(T?);

        void OnClose(Blazor.Models.Dialogs.DialogResult? data)
        {
            if (data?.Result is T)
            {
                result = (T)data.Result;
            }

            if (dialog != null)
            {
                dialog.Close();
            }
        }
    }

    async public Task<T?> ShowKnownDialog<T>(KnownDialogs dialog,
                                             string? title = null,
                                             T? model = default)
    {
        var knownDialog = _knownDialogs.Where(d => d.Dialog == dialog).FirstOrDefault();

        if (knownDialog == null)
        {
            throw new GeneralException($"Dialog {dialog} is not registered as know dialog");
        }

        model = model ?? Activator.CreateInstance<T>();

        if (model == null)
        {
            throw new GeneralException($"Can't create dialog model");
        }

        return await ShowModalDialog(knownDialog.RazorType, title ?? knownDialog.Title, model, knownDialog.DialogOptions);
    }

    public void AddToSnackbar(string message)
    {
        _snackbar.Add(message);
    }

    #region Clipboard

    private ClipboardItem? _clipboardItem = null;
    public void SetClipboardItem(ClipboardItem? item)
    {
        _clipboardItem = item;
    }
    public Type? GetClipboardItemType() => _clipboardItem?.ElementType;
    public IEnumerable<T> GetClipboardElements<T>()
    {
        if (_clipboardItem?.Elements == null)
        {
            return Array.Empty<T>();
        }

        return _clipboardItem.Elements.OfType<T>();
    }

    #endregion

    #region Busy Context

    override protected Task HandleBusyStatusChanged(bool isBussy, string message)
        => _eventBus.FireBusyStatusChanged(isBussy, message);

    override async protected ValueTask SetBusyCursor()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsyncIgnoreErrors("window.gview_base.setCursor", "wait");
        }
        catch { }
    }
    override async protected ValueTask SetDefaultCursor()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsyncIgnoreErrors("window.gview_base.setCursor", "default");
        }
        catch { }
    }

    #endregion
}

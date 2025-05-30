﻿using gView.Blazor.Core.Exceptions;
using gView.Blazor.Core.Services;
using gView.Blazor.Core.Services.Abstraction;
using gView.DataExplorer.Core.Services;
using gView.DataExplorer.Plugins.ExplorerObjects;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Models;
using gView.Framework.Blazor.Services;
using gView.Framework.Core.IO;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.Services;

public class ExplorerApplicationScopeService : ApplictionBusyHandlerAndCache, IExplorerApplicationScopeService
{
    private readonly IDialogService _dialogService;
    private readonly IEnumerable<IKnownDialogService> _knownDialogs;
    private readonly ExplorerEventBusService _eventBus;
    private readonly IJSRuntime _jsRuntime;
    private readonly ISnackbar _snackbar;
    private readonly ExplorerApplicationScopeServiceOptions _options;
    private readonly IConfigConnectionStorageService _configConnectionStorageService;
    private readonly IAppIdentityProvider _identityProvider;


    private readonly DrivesService _drives;

    public ExplorerApplicationScopeService(IDialogService dialogService,
                                           IEnumerable<IKnownDialogService> knownDialogs,
                                           ExplorerEventBusService eventBus,
                                           IJSRuntime jsRuntime,
                                           ISnackbar snackbar,
                                           IConfigConnectionStorageService configConnectionStorageService,
                                           DrivesService drives,
                                           IOptions<ExplorerApplicationScopeServiceOptions> options,
                                           IAppIdentityProvider identityProvider)
    {
        _dialogService = dialogService;
        _knownDialogs = knownDialogs;
        _eventBus = eventBus;
        _jsRuntime = jsRuntime;

        _eventBus.OnCurrentExplorerObjectChanged += EventBus_OnTreeItemClickAsync;
        _eventBus.OnContextExplorerObjectsChanged += EventBus_OnContextExplorerObjectsChanged;
        _snackbar = snackbar;

        _configConnectionStorageService = configConnectionStorageService;
        _drives = drives;

        _options = options.Value;
        _identityProvider = identityProvider;
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

    public IExplorerObject RootExplorerObject(string? fileFilter = null)
        => String.IsNullOrEmpty(fileFilter)
            ? new StartObject(this)
            : new StartObject(this, fileFilter);

    public IExplorerObject? CurrentExplorerObject { get; private set; }
    public IEnumerable<IExplorerObject>? ContextExplorerObjects { get; private set; }

    public ExplorerEventBusService EventBus => _eventBus;

    public IEnumerable<VirtualDrive> VirtualDrives => _drives.VirtualDrives();

    public IConfigConnectionStorage ConfigConnectionStorage => _configConnectionStorageService;

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
            BackdropClick = false,
            CloseButton = modalDialogOptions?.ShowCloseButton ?? true,
            MaxWidth = modalDialogOptions?.Width switch
            {
                ModalDialogWidth.ExtraExtraLarge => MaxWidth.ExtraExtraLarge,
                ModalDialogWidth.Large => MaxWidth.Large,
                ModalDialogWidth.Medium => MaxWidth.Medium,
                ModalDialogWidth.Small => MaxWidth.Small,
                ModalDialogWidth.ExtraSmall => MaxWidth.ExtraSmall,
                _ => MaxWidth.ExtraLarge
            },
            CloseOnEscapeKey = modalDialogOptions?.CloseOnEscapeKey ?? false,
            FullWidth = modalDialogOptions?.FullWidth ?? false,
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

    async public Task ForceContentRefresh()
    {
        if (this.CurrentExplorerObject is IExplorerParentObject parentExObject)
        {
            await parentExObject.DiposeChildObjects();
        }

        await _eventBus.FireSoftRefreshContentAsync();
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

    public string GetToolConfigFilename(params string[] paths)
    {
        var fileInfo = new FileInfo(Path.Combine(_options.ConfigRootPath, Path.Combine(paths)));

        if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
        {
            fileInfo.Directory.Create();
        }

        return fileInfo.FullName;
    }

    public IEnumerable<string> GetToolConfigFiles(params string[] paths)
    {
        var directoryInfo = new DirectoryInfo(Path.Combine(_options.ConfigRootPath, Path.Combine(paths)));

        if (!directoryInfo.Exists)
        {
            return Array.Empty<string>();
        }

        return directoryInfo.GetFiles().Select(f => f.FullName);
    }

    public async ValueTask<bool> CopyContentToClientClipboardAsyc(string elementId)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsyncIgnoreErrors("window.gview_base.copyToClipboard", elementId);
            return true;
        }
        catch { return false; }
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

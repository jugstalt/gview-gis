using gView.Blazor.Core;
using gView.Blazor.Core.Exceptions;
using gView.Blazor.Core.Services.Abstraction;
using gView.DataExplorer.Core.Services;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.Services;

public class ExplorerApplicationScopeService : IApplicationScope
{
    private readonly IDialogService _dialogService;
    IEnumerable<IKnownDialogService> _knownDialogs;
    private readonly EventBusService _eventBus;

    public ExplorerApplicationScopeService(IDialogService dialogService,
                                           IEnumerable<IKnownDialogService> knownDialogs,
                                           EventBusService eventBus)
    {
        _dialogService = dialogService;
        _knownDialogs = knownDialogs;
        _eventBus = eventBus;

        _eventBus.OnCurrentExplorerObjectChanged += EventBus_OnTreeItemClickAsync;
        _eventBus.OnContextExplorerObjectsChanged += EventBus_OnContextExplorerObjectsChanged;
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
                                             T? model = default(T))
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
            CloseButton = true,
            MaxWidth = MaxWidth.ExtraExtraLarge,
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

        return await ShowModalDialog(knownDialog.RazorType, title ?? knownDialog.Title, model);
    }
}

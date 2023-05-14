using gView.DataExplorer.Core.Services;
using gView.Framework.DataExplorer.Abstraction;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace gView.DataExplorer.Plugins.Services;

public class ExplorerApplicationScopeService : IExplorerApplicationScope
{
    private readonly IDialogService _dialogService;
    private readonly EventBusService _eventBus;

    public ExplorerApplicationScopeService(IDialogService dialogService,
                                           EventBusService eventBus)
    {
        _dialogService = dialogService;
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
}

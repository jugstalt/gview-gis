using gView.DataExplorer.Core.Services;
using gView.Framework.DataExplorer.Abstraction;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.Services;

public class ExplorerApplicationScopeService : IExplorerApplicationScope, IHandleEvent
{
    private readonly IDialogService _dialogService;
    private readonly EventBusService _eventBus;

    public ExplorerApplicationScopeService(IDialogService dialogService,
                                           EventBusService eventBus)
    {
        _dialogService = dialogService;
        _eventBus = eventBus;

        _eventBus.OnTreeItemClickAsync += EventBus_OnTreeItemClickAsync;
    }

    #region IDisposable

    public void Dispose()
    {
        _eventBus.OnTreeItemClickAsync -= EventBus_OnTreeItemClickAsync;
    }

    #endregion

    private Task EventBus_OnTreeItemClickAsync(IExplorerObject? arg)
    {
        CurrentExplorerObject = arg;

        return Task.CompletedTask;
    }

    public IExplorerObject? CurrentExplorerObject { get; private set; }

    public EventBusService EventBus => _eventBus;

    //public void ShowModalDialog2(Type razorType, Action<IDialogResultItem> callback)
    //{
    //    throw new ShowDialogException(razorType, callback);
    //}

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

    #region IHandleEvent

    public Task HandleEventAsync(EventCallbackWorkItem item, object? arg)
    {
        return item.InvokeAsync(arg);
    }

    #endregion
}

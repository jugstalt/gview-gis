using gView.Blazor.Core.Exceptions;
using gView.Blazor.Core.Extensions;
using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Models;
using gView.Framework.Blazor.Services.Abstraction;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace gView.Rator.Services;

internal class ApplicationScopeFactory : IApplicationScopeFactory, IDisposable
{
    private readonly IDialogService _dialogService;
    private readonly IEnumerable<IKnownDialogService> _knownDialogs;
    private readonly IServiceScope _serviceScope;

    public ApplicationScopeFactory(IDialogService dialogService,
                                   IEnumerable<IKnownDialogService> knownDialogs,
                                   IServiceProvider serviceProvider)
    {
        _dialogService = dialogService;
        _knownDialogs = knownDialogs;
        _serviceScope = serviceProvider.CreateScope();
    }

    #region IApplicationScopeFactory

    public T GetInstance<T>() where T : IApplicationScope
    {
        var instance = (_serviceScope.ServiceProvider.GetRequiredService<T>() as IApplicationScope)
            .ThrowIfNull(() => $"Can't resolve {typeof(T)} IApplicationService as IApplicationScope");

        return (T)instance;

        
    }

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

        return await ShowModalDialog(knownDialog.RazorType,
                                     title ?? knownDialog.Title,
                                     model,
                                     knownDialog.DialogOptions);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        _serviceScope.Dispose();
    }

    #endregion
}

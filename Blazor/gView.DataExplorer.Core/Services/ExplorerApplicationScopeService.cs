using gView.Blazor.Core.Exceptions;
using gView.Blazor.Models.Dialogs;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.Network;
using System;

namespace gView.DataExplorer.Core.Services;

public class ExplorerApplicationScopeService : IExplorerApplicationScope
{
    private readonly EventBusService _eventBus;

    public ExplorerApplicationScopeService(EventBusService eventBus)
    {
        _eventBus = eventBus;
    }

    public EventBusService EventBus => _eventBus;

    public void ShowModalDialog(Type razorType, Action<IDialogResultItem> callback)
    {
        throw new ShowDialogException(razorType, callback);
    }
}

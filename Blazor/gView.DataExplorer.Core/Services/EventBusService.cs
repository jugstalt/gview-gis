using gView.DataExplorer.Core.Extensions;
using gView.Framework.DataExplorer.Abstraction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Core.Services;

public class EventBusService
{
    public event Func<Task>? OnRefreshContentAsync;
    public Task FireFreshContentAsync()
        => OnRefreshContentAsync?.FireAsync() ?? Task.CompletedTask;

    public event Func<IExplorerObject?, Task>? OnCurrentExplorerObjectChanged;
    public Task FireCurrentExplorerObjectChanged(IExplorerObject? item)
        => OnCurrentExplorerObjectChanged?.FireAsync(item) ?? Task.CompletedTask;

    public event Func<IExplorerObject?, Task>? OnSetCurrentExplorerObjectAsync;
    public Task SetCurrentExplorerObjectAsync(IExplorerObject? item)
        => OnSetCurrentExplorerObjectAsync?.FireAsync(item) ?? Task.CompletedTask;

    public event Func<IEnumerable<IExplorerObject>?, Task>? OnContextExplorerObjectsChanged;
    public Task FireContextExplorerObjectsChanged(IEnumerable<IExplorerObject>? item)
        => OnContextExplorerObjectsChanged?.Invoke(item) ?? Task.CompletedTask;
}

using gView.DataExplorer.Core.Extensions;
using gView.Framework.DataExplorer.Abstraction;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Core.Services;

public class EventBusService
{
    public event Func<Task>? OnRefreshContentAsync;
    public Task FireRefreshContentAsync()
        => OnRefreshContentAsync?.FireAsync() ?? Task.CompletedTask;

    public event Func<IExplorerObject?, Task>? OnTreeItemClickAsync;
    public Task FireTreeItemClickAsync(IExplorerObject? item)
        => OnTreeItemClickAsync?.FireAsync(item) ?? Task.CompletedTask;

    //public event Func<IExplorerObject, Task>? OnContentItemClickAsync;
    //public Task ContentItemClickAsync(IExplorerObject item)
    //    => OnContentItemClickAsync?.Invoke(item) ?? Task.CompletedTask;

    public event Func<IExplorerObject?, Task>? OnSetCurrentExplorerObjectAsync;
    public Task SetCurrentExplorerObjectAsync(IExplorerObject? item)
        => OnSetCurrentExplorerObjectAsync?.FireAsync(item) ?? Task.CompletedTask;    
}

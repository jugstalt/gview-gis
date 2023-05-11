using gView.DataExplorer.Core.Extensions;
using gView.Framework.DataExplorer.Abstraction;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Core.Services;

public class EventBusService
{
    public event Func<IExplorerObject, Task>? OnTreeItemClickAsync;
    public event Func<IExplorerObject, Task>? OnContentItemClickAsync;
    public event Func<IExplorerObject, Task>? OnContentItemDoubleClickAsync;

    public Task TreeItemClickAsync(IExplorerObject item)
        => OnTreeItemClickAsync?.FireAsync(item) ?? Task.CompletedTask;


    public Task ContentItemClickAsync(IExplorerObject item)
        => OnContentItemClickAsync?.Invoke(item) ?? Task.CompletedTask;


    public Task ContentItemDoubleClickAsync(IExplorerObject item)
        => OnContentItemDoubleClickAsync?.Invoke(item) ?? Task.CompletedTask;
}

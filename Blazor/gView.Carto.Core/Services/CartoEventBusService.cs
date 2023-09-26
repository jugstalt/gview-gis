using gView.Carto.Core.Extensions;
using System;
using System.Threading.Tasks;

namespace gView.Carto.Core.Services;
public class CartoEventBusService
{
    public event Func<bool, string, Task>? OnBusyStatusChangedAsync;
    public Task FireBusyStatusChanged(bool isBusy, string statusText)
        => OnBusyStatusChangedAsync?.FireAsync(isBusy, statusText) ?? Task.CompletedTask;
}

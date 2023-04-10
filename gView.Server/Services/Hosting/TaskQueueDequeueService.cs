using gView.Server.Services.Logging;
using gView.Server.Services.MapServer;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace gView.Server.Services.Hosting;

public class TaskQueueDequeueService : BackgroundService
{
    private readonly MapServiceManager _mapServerService;

    public TaskQueueDequeueService(MapServiceManager mapServerService)
    {
        _mapServerService = mapServerService;
    }

    async protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_mapServerService.TaskQueue.Dequeue(), stoppingToken);
        }
    }
}

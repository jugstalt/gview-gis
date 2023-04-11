using gView.Framework.system;
using gView.Server.Services.MapServer;
using Microsoft.AspNetCore.Mvc;

namespace gView.Server.Controllers;
public class Info : Controller
{
    private readonly MapServiceManager _mapServiceManager;
    public Info(MapServiceManager mapServerService)
    {
        _mapServiceManager = mapServerService;
    }

    public IActionResult Index()
    {
        return Json(new
        {
            version = SystemInfo.Version,
            queue = new
            {
                IdleDuration = _mapServiceManager.TaskQueue.IdleDuration,
                currentQueued = _mapServiceManager.TaskQueue.CurrentQueuedTasks,
                currentRunningTasks = _mapServiceManager.TaskQueue.CurrentRunningTasks
            }
        });
    }
}

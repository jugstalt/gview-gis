using gView.Facilities.Abstraction;
using gView.Server.Services.MapServer;
using System.Threading.Tasks;

namespace gView.Server.Services.Handlers;

public class ReloadMapMessageHandler(MapServiceDeploymentManager delolyManager) : IMessageHandler
{
    public const string Name = "ReloadMap"; 

    public Task InvokeAsync(string message)
        => delolyManager.ReloadMap(message);
}

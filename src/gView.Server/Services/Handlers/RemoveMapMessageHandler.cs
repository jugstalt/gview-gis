using gView.Facilities.Abstraction;
using gView.Server.Services.MapServer;
using System.Threading.Tasks;

namespace gView.Server.Services.Handlers;

public class RemoveMapMessageHandler(MapServiceDeploymentManager delolyManager) : IMessageHandler
{
    public const string Name = "RemoveMap";

    public Task InvokeAsync(string message)
    {
        delolyManager.RemoveMap(message);

        return Task.CompletedTask;
    }
}

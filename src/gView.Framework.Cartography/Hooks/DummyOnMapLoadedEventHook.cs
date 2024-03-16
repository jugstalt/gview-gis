using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.IO;
using System.Threading.Tasks;

namespace gView.Framework.Cartography.Hooks;

[RegisterPlugIn("063F7899-D9CE-44FD-871A-6C229DC687B9")]
public class DummyOnMapLoadedEventHook : IMapEventHook
{
    public HookEventType Type => HookEventType.OnLoaded;

    public Task InvokeAsync(IMap map)
    {
        return Task.CompletedTask;
    }

    public void Load(IPersistStream stream)
    {

    }

    public void Save(IPersistStream stream)
    {

    }
}

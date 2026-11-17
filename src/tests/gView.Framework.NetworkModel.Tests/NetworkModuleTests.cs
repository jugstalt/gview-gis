using Xunit;
using gView.Framework.Core.Carto;
using gView.Framework.IO;
using gView.Framework.NetworkModel.Module;
using System.Reflection;

namespace gView.Framework.NetworkModel.Tests;

public class NetworkModuleTests
{
    /// <summary>An IMap that is never asked anything.</summary>
    public class MapProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => throw new NotSupportedException(targetMethod?.Name);
    }

    [Fact]
    public void OnCreate_AfterLoad_KeepsTheLoadedConfig()
    {
        // the server loads a module from the map document first and calls OnCreate afterwards
        var stream = new XmlStream("Module");
        stream.Save("config", """{ "name": "net", "nodes": [ { "layer": "Trafo", "layerId": 1, "type": [ "a" ] } ] }""");

        var module = new NetworkModule();
        module.Load(stream);
        module.OnCreate(DispatchProxy.Create<IMap, MapProxy>());

        Assert.True(module.IsConfigured);
        Assert.Equal("net", module.Config.Name);
    }
}

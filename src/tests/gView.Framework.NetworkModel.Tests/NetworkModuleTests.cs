using Xunit;
using gView.Framework.Core.Carto;
using gView.Framework.IO;
using gView.Framework.NetworkModel.Module;
using gView.Framework.NetworkModel.Store;
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

    [Fact]
    public void GetStatus_MapSavedBeforeTheBuild_IsNotStale_WhateverTheTimeZone()
    {
        var folder = Path.Combine(Path.GetTempPath(), "gview-network-status-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var mapPath = Path.Combine(folder, "map.mxl");
            File.WriteAllText(mapPath, "<map />");
            File.SetLastWriteTimeUtc(mapPath, DateTime.UtcNow.AddMinutes(-1));

            var module = new NetworkModule();
            using (var writer = new FileNetworkStore(module.Config.ResolveDirectory(mapPath)).CreateWriter())
            {
                writer.Commit(new NetworkMetadata { Name = "net", Created = DateTime.UtcNow });
            }

            var status = module.GetStatus(mapPath);
            Assert.True(status.Built);
            Assert.False(status.Stale, status.StaleReason);
            Assert.Equal(DateTimeKind.Local, status.Created!.Value.Kind);

            File.SetLastWriteTimeUtc(mapPath, DateTime.UtcNow.AddMinutes(1));
            Assert.True(module.GetStatus(mapPath).Stale);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void AsUtc_TreatsTimesWithoutKindAsUtc()
    {
        var utc = new DateTime(2026, 11, 22, 20, 6, 53, DateTimeKind.Utc);
        Assert.Equal(utc, NetworkModule.AsUtc(utc));
        Assert.Equal(utc, NetworkModule.AsUtc(DateTime.SpecifyKind(utc, DateTimeKind.Unspecified)));
        Assert.Equal(utc, NetworkModule.AsUtc(utc.ToLocalTime()));
    }
}

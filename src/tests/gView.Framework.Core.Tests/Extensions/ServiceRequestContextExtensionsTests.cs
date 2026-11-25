using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Extensions;
using gView.Framework.Core.IO;
using gView.Framework.Core.MapServer;

namespace gView.Framework.Core.Tests.Extensions;

public class ServiceRequestContextExtensionsTests
{
    [Fact]
    public async Task LogServiceException_InternalException_LogsDetailsAndReturnsGenericText()
    {
        var context = new FakeContext();
        var ex = new InvalidOperationException(@"connection to C:\data\secret.gdb failed");

        string message = await context.LogServiceException(ex, "Query");

        Assert.Equal($"internal server error (id: {context.ServiceRequest.ErrorId})", message);
        var log = Assert.Single(context.FakeMapServer.Logs);
        Assert.Equal(loggingMethod.error, log.method);
        Assert.Equal("Query", log.header);
        Assert.Contains(context.ServiceRequest.ErrorId, log.msg);
        Assert.Contains(@"C:\data\secret.gdb", log.msg);
    }

    [Fact]
    public async Task LogServiceException_MapServerException_ReturnsMessageAndLogsMessageOnly()
    {
        var context = new FakeContext();
        var ex = new MapServerException("Unknown layer id: 7");

        string message = await context.LogServiceException(ex, "Query");

        Assert.Equal("Unknown layer id: 7", message);
        Assert.Equal("Unknown layer id: 7", Assert.Single(context.FakeMapServer.Logs).msg);
    }

    [Fact]
    public async Task LogServiceException_WithoutMapServer_StillReturnsGenericText()
    {
        var context = new FakeContext(withMapServer: false);

        string message = await context.LogServiceException(new Exception("internal"), "Query");

        Assert.StartsWith("internal server error (id: ", message);
    }

    #region Fakes

    internal class FakeContext : IServiceRequestContext
    {
        public FakeContext(bool withMapServer = true)
        {
            FakeMapServer = new FakeMapServer();
            MapServer = withMapServer ? FakeMapServer : null!;
        }

        public FakeMapServer FakeMapServer { get; }
        public IMapServer MapServer { get; }
        public IServiceRequestInterpreter ServiceRequestInterpreter => null!;
        public ServiceRequest ServiceRequest { get; } = new ServiceRequest("service", "", "");
        public IDictionary<string, double> Metrics { get; } = new Dictionary<string, double>();

        public void SetContextMetadata<T>(string key, T value) { }
        public T GetContextMetadata<T>(string key, T defaultValue = default!) => defaultValue;
        public Task<IServiceMap> CreateServiceMapInstance() => throw new NotImplementedException();
        public Task<IMetadataProvider> GetMetadtaProviderAsync(Guid metadataProviderId) => throw new NotImplementedException();
    }

    internal class FakeMapServer : IMapServer
    {
        public List<(string header, loggingMethod method, string msg)> Logs { get; } = new();

        public Task LogAsync(string mapName, string header, loggingMethod methode, string msg)
        {
            Logs.Add((header, methode, msg));
            return Task.CompletedTask;
        }

        public Task LogAsync(IServiceRequestContext context, string header, loggingMethod methode, string msg)
            => LogAsync(context.ServiceRequest.Service, header, methode, msg);

        public bool LoggingEnabled(loggingMethod methode) => true;

        public Task<IEnumerable<IMapService>> Maps(IIdentity identity) => throw new NotImplementedException();
        public Task<IServiceMap> GetServiceMapAsync(string name, string folder) => throw new NotImplementedException();
        public Task<IServiceMap> GetServiceMapAsync(IMapService service) => throw new NotImplementedException();
        public Task<IServiceMap> GetServiceMapAsync(IServiceRequestContext context) => throw new NotImplementedException();
        public Task<IMetadataProvider> GetMetadtaProviderAsync(IServiceRequestContext context, Guid metadataProviderId) => throw new NotImplementedException();
        public IMapService GetMapService(string name, string folder) => throw new NotImplementedException();
        public bool IsLoaded(string name, string folder) => false;
        public string OutputPath => "";
        public string EtcPath => "";
        public string TileCachePath => "";
    }

    #endregion
}

using gView.Framework.Common.Json;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.IO;
using gView.Framework.Core.MapServer;
using gView.Interoperability.GeoServices.Exceptions;
using gView.Interoperability.GeoServices.Extensions;
using gView.Interoperability.GeoServices.Rest.DTOs;
using gView.Interoperability.GeoServices.Rest.DTOs.FeatureServer;

namespace gView.Interoperability.GeoServices.Tests.Extensions;

public class ServiceRequestContextExtensionsTests
{
    [Fact]
    public async Task HandleMapServerException_NullReferenceException_NoStackTraceForClient()
    {
        var context = new FakeContext();
        Exception ex;
        try
        {
            string? s = null;
            _ = s!.Length;
            throw new InvalidOperationException();
        }
        catch (Exception caught)
        {
            ex = caught;
        }

        await context.HandleMapServerException(ex);

        var error = Assert.IsType<JsonErrorDTO>(context.ServiceRequest.Response).Error;
        Assert.Equal(500, error.Code);
        Assert.Equal($"internal server error (id: {context.ServiceRequest.ErrorId})", error.Message);
        Assert.False(context.ServiceRequest.Succeeded);

        // the details are in the log
        var log = Assert.Single(context.FakeMapServer.Logs);
        Assert.Contains(context.ServiceRequest.ErrorId, log);
        Assert.Contains(nameof(HandleMapServerException_NullReferenceException_NoStackTraceForClient), log);
    }

    [Fact]
    public async Task HandleMapServerException_MapServerException_MessageWithCode400()
    {
        var context = new FakeContext();

        await context.HandleMapServerException(new MapServerException("Can't parse layer definitions"));

        var error = Assert.IsType<JsonErrorDTO>(context.ServiceRequest.Response).Error;
        Assert.Equal(400, error.Code);
        Assert.Equal("Can't parse layer definitions", error.Message);
    }

    [Fact]
    public async Task HandleMapServerException_GeoServicesException_OwnCodeAndMessage()
    {
        var context = new FakeContext();

        await context.HandleMapServerException(new GeoServicesException("Invalid token", 498));

        var error = Assert.IsType<JsonErrorDTO>(context.ServiceRequest.Response).Error;
        Assert.Equal(498, error.Code);
        Assert.Equal("Invalid token", error.Message);
    }

    [Fact]
    public async Task HandleFeatureServerException_InternalException_GenericDescription()
    {
        var context = new FakeContext();

        await context.HandleFeatureServerException(new Exception("ERROR: duplicate key value violates unique constraint \"parcels_pkey\""));

        var response = JSerializer.Deserialize<JsonFeatureServerResponseDTO>((string)context.ServiceRequest.Response);
        var error = Assert.Single(response!.AddResults).Error;
        Assert.Equal(500, error.Code);
        Assert.DoesNotContain("parcels_pkey", error.Description);
        Assert.Contains(context.ServiceRequest.ErrorId, error.Description);
    }

    #region Fakes

    private class FakeContext : IServiceRequestContext
    {
        public FakeMapServer FakeMapServer { get; } = new();
        public IMapServer MapServer => FakeMapServer;
        public IServiceRequestInterpreter ServiceRequestInterpreter => null!;
        public ServiceRequest ServiceRequest { get; } = new ServiceRequest("service", "", "") { Method = "query" };
        public IDictionary<string, double> Metrics { get; } = new Dictionary<string, double>();

        public void SetContextMetadata<T>(string key, T value) { }
        public T GetContextMetadata<T>(string key, T defaultValue = default!) => defaultValue;
        public Task<IServiceMap> CreateServiceMapInstance() => throw new NotImplementedException();
        public Task<IMetadataProvider> GetMetadtaProviderAsync(Guid metadataProviderId) => throw new NotImplementedException();
    }

    private class FakeMapServer : IMapServer
    {
        public List<string> Logs { get; } = new();

        public Task LogAsync(string mapName, string header, loggingMethod methode, string msg)
        {
            Logs.Add(msg);
            return Task.CompletedTask;
        }

        public Task LogAsync(IServiceRequestContext context, string header, loggingMethod methode, string msg)
            => LogAsync("", header, methode, msg);

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

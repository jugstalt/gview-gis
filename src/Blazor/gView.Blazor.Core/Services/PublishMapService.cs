using gView.Blazor.Core.Services.Abstraction;
using gView.Blazor.Models.MapServer;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gView.Interoperability.Server.Admin;

namespace gView.Blazor.Core.Services;

public class PublishMapService
{
    private readonly PublishMapServiceOptions _options;
    private readonly IAppIdentityProvider _identityProvider;

    public PublishMapService(IAppIdentityProvider identityProvider,
                             IOptions<PublishMapServiceOptions> options)
    {
        _options = options.Value;
        _identityProvider = identityProvider;
    }

    public bool HasPublishServers
        => _identityProvider?.Identity?.IsAdministrator == true
        && _options.Services?.Any() == true;

    public IEnumerable<ServerInstanceModel> Servers
        => HasPublishServers
        ? _options.Services.ToArray()
        : Array.Empty<ServerInstanceModel>();

    public Task<bool> PublishMap(
                    ServerInstanceModel server, 
                    string folder,
                    string serviceName,
                    string mxl,
                    string? client = null,
                    string? secret = null
                )
        => CreatePublisher(server, client, secret).Publish(folder, serviceName, mxl);
    
    public Task<string[]> GetFolders(
                    ServerInstanceModel server, 
                    string? client = null, 
                    string? secret = null
                )
        => CreatePublisher(server, client, secret).GetFolders();

    public Task<string[]> GetServiceNames(
                    ServerInstanceModel server, 
                    string folder,
                    string? client = null,
                    string? secret = null
                )
        => CreatePublisher(server, client, secret).GetServiceNames(folder);

    private Publisher CreatePublisher(
                    ServerInstanceModel server,
                    string? client,
                    string? secret
                )
        => new Publisher(
                        server.Url, 
                        String.IsNullOrEmpty(client) ? server.Client : client, 
                        String.IsNullOrEmpty(secret) ? server.Secret : secret
                    );
}

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
                    string mxl
                )
        => CreatePublisher(server).Publish(folder, serviceName, mxl);
    
    public Task<string[]> GetFolders(ServerInstanceModel server)
        => CreatePublisher(server).GetFolders();

    public Task<string[]> GetServiceNames(ServerInstanceModel server, string folder)
        => CreatePublisher(server).GetServiceNames(folder);

    private Publisher CreatePublisher(ServerInstanceModel server)
        => new Publisher(server.Url, server.Client, server.Secret);
}

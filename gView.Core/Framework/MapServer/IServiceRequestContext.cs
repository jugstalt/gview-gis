using gView.Framework.Carto;
using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.Threading.Tasks;

namespace gView.MapServer
{
    public interface IServiceRequestContext : IContext
    {
        IMapServer MapServer { get; }
        IServiceRequestInterpreter ServiceRequestInterpreter { get; }
        ServiceRequest ServiceRequest { get; }

        Task<IServiceMap> CreateServiceMapInstance();

        Task<IMetadataProvider> GetMetadtaProviderAsync(Guid metadataProviderId);
    }
}

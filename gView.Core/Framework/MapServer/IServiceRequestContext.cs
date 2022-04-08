using System;
using gView.Framework.Carto;
using gView.Framework.system;
using gView.Framework.IO;
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

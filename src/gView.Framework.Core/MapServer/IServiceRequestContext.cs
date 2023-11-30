using gView.Framework.Core.Carto;
using gView.Framework.Core.IO;
using gView.Framework.Core.system;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Core.MapServer
{
    public interface IServiceRequestContext : IContext
    {
        IMapServer MapServer { get; }
        IServiceRequestInterpreter ServiceRequestInterpreter { get; }
        ServiceRequest ServiceRequest { get; }
        IDictionary<string, double> Metrics { get; }

        void SetContextMetadata<T>(string key, T value);
        T GetContextMetadata<T>(string key, T defaultValue = default);

        Task<IServiceMap> CreateServiceMapInstance();

        Task<IMetadataProvider> GetMetadtaProviderAsync(Guid metadataProviderId);
    }
}

using System;
using System.Collections.Generic;
using gView.Framework.Carto;
using gView.Framework.system;
using gView.Framework.IO;
using System.Threading.Tasks;

namespace gView.MapServer
{
    public interface IMapServer
    {
        Task<IEnumerable<IMapService>> Maps(IIdentity identity);
        Task<IServiceMap> GetServiceMapAsync(string name, string folder);
        Task<IServiceMap> GetServiceMapAsync(IMapService service);
        Task<IServiceMap> GetServiceMapAsync(IServiceRequestContext context);

        Task<IMetadataProvider> GetMetadtaProviderAsync(IServiceRequestContext context, Guid metadataProviderId);

        IMapService GetMapService(string name, string folder);
        bool IsLoaded(string name, string folder);

        bool LoggingEnabled(loggingMethod methode);
        Task LogAsync(string mapName, string header, loggingMethod methode, string msg);
        Task LogAsync(IServiceRequestContext context, string header, loggingMethod methode, string msg);

        //string OutputUrl { get; }
        string OutputPath { get; }

        string EtcPath { get; }

        string TileCachePath { get; }

        int FeatureQueryLimit { get; }
    }
}

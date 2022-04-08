using System;
using gView.Framework.system;
using System.Threading.Tasks;

namespace gView.MapServer
{
    public interface IMapService
    {
        string Name { get; }
        string Folder { get; }
        MapServiceType Type { get; }
        //IServiceMap Map { get; }

        string Fullname { get; }

        Task<bool> RefreshRequired();
        void ServiceRefreshed();
        DateTime? RunningSinceUtc { get; }

        Task<IMapServiceSettings> GetSettingsAsync();
        Task SaveSettingsAsync();

        Task CheckAccess(IServiceRequestContext context);
        Task CheckAccess(IIdentity identity, IServiceRequestInterpreter interpreter);
        Task<bool> HasAnyAccess(IIdentity identity);
        Task<AccessTypes> GetAccessTypes(IIdentity identity);

        Task<bool> HasPublishAccess(IIdentity identity);
    }
}

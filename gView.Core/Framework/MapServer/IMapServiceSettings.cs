using System;

namespace gView.MapServer
{
    public interface IMapServiceSettings
    {
        MapServiceStatus Status { get; set; }

        IMapServiceAccess[] AccessRules { get; set; }

        DateTime RefreshService { get; set; }

        string OnlineResource { get; set; }
        string OutputUrl { get; set; }
    }
}

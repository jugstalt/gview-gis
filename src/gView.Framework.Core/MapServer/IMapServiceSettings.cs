using System;

namespace gView.Framework.Core.MapServer
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

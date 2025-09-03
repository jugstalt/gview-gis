using gView.Framework.Core.Carto;
using System;

namespace gView.Server.AppCode.Extensions;

static internal class ServiceMapExtensions
{
    static public IServiceMap ThrowIfNull(this IServiceMap serviceMap)
    {
        if (serviceMap is null)
        {
            throw new Exception("Can't load service. Check service logs for details");
        }

        return serviceMap;
    }
}

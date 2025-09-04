using gView.Framework.Core.Carto;
using gView.Framework.Core.Exceptions;
using System;

namespace gView.Server.AppCode.Extensions;

static internal class ServiceMapExtensions
{
    static public IServiceMap ThrowIfNull(this IServiceMap serviceMap)
    {
        if (serviceMap is null)
        {
            throw new MapServerException("Can't load service. Check service logs for details");
        }

        return serviceMap;
    }

    static public IServiceMap ThrowIfNull(this IServiceMap serviceMap, string id)
    {
        if (serviceMap is null)
        {
            throw new MapServerException($"Unable to create map: {id}. Check log file for details", 500);
        }

        return serviceMap;
    }
}

using gView.Framework.Logging.ResourceLogging;
using gView.Interoperability.GeoServices.Rest.Json;
using gView.Interoperability.GeoServices.Rest.Json.Response;
using gView.Server.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Extensions
{
    static public class PerformanceLoggerExtension
    {
        static public void AddPerformanceLoggerItem(this JsonStopWatch response,
                                                    PerformanceLoggerService logger,
                                                    string folder,
                                                    string serviceId,
                                                    string method)
        {
            if (logger.UseLogging && !String.IsNullOrEmpty(folder) && !String.IsNullOrEmpty(serviceId))
            {
                var item = new PerforamceLoggerServiceRequestItem()
                {
                    PartitionKey = $"gview.{ folder.ToLower() }",
                    RowKey = $"{(long.MaxValue - DateTime.UtcNow.Ticks).ToString().PadLeft(19, '0')}_{new Random().Next(9999).ToString().PadLeft(4, '0')}",

                    Created = DateTime.UtcNow,
                    ServiceId = serviceId?.ToLower(),
                    Milliseconds = (int)response.DurationMilliseconds,
                    ContentSize = response.SizeBytes.HasValue ? response.SizeBytes.Value : 0,
                    Count = 1,
                    TypeName = $"Geoservices.{ method }"
                };

                logger.Log(item);
            }
        }
    }
}

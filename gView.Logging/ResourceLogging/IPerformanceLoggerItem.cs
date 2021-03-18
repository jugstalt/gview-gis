using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace gView.Framework.Logging.ResourceLogging
{
    public interface IPerformanceLoggerItem : ITableEntity
    {
        int Milliseconds { get; set; }
        int ContentSize { get; set; }
        string TypeName { get; set; }
        int Count { get; set; }


        string ServiceId { get; set; }

        DateTime Created { get; set; }

        bool CanAggregate(IPerformanceLoggerItem item);
        void Aggregate(IPerformanceLoggerItem item);
    }
}

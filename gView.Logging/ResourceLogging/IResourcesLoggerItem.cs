using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace gView.Logging.ResourceLogging
{
    public interface IResourcesLoggerItem : ITableEntity
    {
        int Milliseconds { get; set; }
        int ContentSize { get; set; }
        string TypeName { get; set; }
        int Count { get; set; }

        DateTime Created { get; set; }

        bool CanAggregate(IResourcesLoggerItem item);
        void Aggregate(IResourcesLoggerItem item);
    }
}

using Azure;
using Azure.Data.Tables;
using System;

namespace gView.Framework.Logging.ResourceLogging
{
    public class PerforamceLoggerServiceRequestItem : ITableEntity, IPerformanceLoggerItem
    {
        #region ITableEntity

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        #endregion

        #region IResourcesLoggerItem

        public int Milliseconds { get; set; }
        public int ContentSize { get; set; }
        public string WatchId { get; set; }
        public string TypeName { get; set; }
        public int Count { get; set; }
        public DateTime Created { get; set; }

        public bool CanAggregate(IPerformanceLoggerItem item)
        {
            if (item is PerforamceLoggerServiceRequestItem)
            {
                return this.PartitionKey == item.PartitionKey &&
                       this.TypeName == item.TypeName &&
                       this.WatchId == item.WatchId;
            }

            return false;
        }

        public void Aggregate(IPerformanceLoggerItem item)
        {
            if (item is PerforamceLoggerServiceRequestItem)
            {
                this.Count += item.Count;
                this.Milliseconds += item.Milliseconds;
                this.ContentSize += item.ContentSize;
            }
        }

        #endregion
    }
}

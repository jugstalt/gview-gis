using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Logging.ResourceLogging
{
    public class AzureTableResourceLogger : IResourcesLogger
    {
        private Azure.Storage.TableStorage _tableStorage = null;
        private ConcurrentBag<IResourcesLoggerItem> _bag = new ConcurrentBag<IResourcesLoggerItem>();

        const string tableName = "webgisresourcelogs";
        const string tableNameAggregated = tableName + "agg";

        #region IResourcesLogger

        async public Task Init(string initialParameter)
        {
            try
            {
                _tableStorage = new Azure.Storage.TableStorage();
                _tableStorage.Init(initialParameter);
                await _tableStorage.CreateTableAsync(tableName);
                await _tableStorage.CreateTableAsync(tableNameAggregated);
            }
            catch /*(Exception ex)*/
            {
                //Console.WriteLine("Can't init AzureTableResourceLogger:");
                //Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.StackTrace);
                _tableStorage = null;
            }
        }

        static object _locker = new object();
        async public Task Log(IResourcesLoggerItem item)
        {
            if (_tableStorage != null && item != null)
            {
                try
                {
                    _bag.Add(item);

                    if (_bag.Count > 100)
                    {
                        await Flush();
                    }
                }
                catch /*(Exception ex)*/
                {
                    //Console.WriteLine("Can't add ResourceLogItem:");
                    //Console.WriteLine(ex.Message);
                    //Console.WriteLine(ex.StackTrace);
                }
            }
        }

        async public Task Flush()
        {
            try
            {
                ConcurrentBag<IResourcesLoggerItem> currentBag = null;
                lock (_locker)
                {
                    currentBag = _bag;
                    _bag = new ConcurrentBag<IResourcesLoggerItem>();
                }

                await _tableStorage.InsertEntitiesAsync<IResourcesLoggerItem>(tableName, currentBag.ToArray());
                await _tableStorage.InsertEntitiesAsync<IResourcesLoggerItem>(tableNameAggregated,
                    Aggregate(currentBag.ToArray()).ToArray());
            }
            catch { }
        }

        #endregion

        async public Task<IEnumerable<T>> GetLogs<T>(string id, int seconds, bool aggregated)
            where T : ITableEntity, new()
        {
            if (_tableStorage != null)
            {
                var logs = await _tableStorage.AllEntitiesAsync<T>(aggregated == true ? tableNameAggregated : tableName, id);
                return logs;
            }

            return null;
        }

        #region Helper

        private IEnumerable<IResourcesLoggerItem> Aggregate(IEnumerable<IResourcesLoggerItem> items)
        {
            List<IResourcesLoggerItem> list = new List<IResourcesLoggerItem>(items);

            List<IResourcesLoggerItem> aggregated = new List<IResourcesLoggerItem>();

            try
            {
                while (list.Count > 0)
                {
                    var aggregateItem = list.First();
                    list.Remove(aggregateItem);

                    foreach (var item in list.Where(i =>
                                 i.Created.Hour == aggregateItem.Created.Hour &&
                                 i.Created.Day == aggregateItem.Created.Day &&
                                 i.Created.Month == aggregateItem.Created.Month &&
                                 i.Created.Year == aggregateItem.Created.Year &&
                                 i.CanAggregate(aggregateItem)).ToArray())
                    {
                        list.Remove(item);
                        aggregateItem.Aggregate(item);
                    }

                    aggregated.Add(aggregateItem);
                }
            }
            catch { }
            return aggregated;
        }

        #endregion
    }
}

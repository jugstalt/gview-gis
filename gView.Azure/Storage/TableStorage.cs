using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.Azure.Storage
{
    public class TableStorage
    {
        private string _connectionString;

        public bool Init(string initialParameter)
        {
            _connectionString = initialParameter;
            return true;
        }

        async public Task<bool> CreateTableAsync(string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            CloudTable table = tableClient.GetTableReference(tableName);

            await table.CreateIfNotExistsAsync();

            return true;
        }

        #region Insert

        async private Task<bool> InsertEntity(string tableName, TableEntity entity, bool mergeIfExists = false)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            // Create the TableOperation object that inserts the customer entity.
            TableOperation inserOperation = mergeIfExists ?
                TableOperation.InsertOrMerge(entity) :
                TableOperation.Insert(entity);

            // Execute the insert operation.
            await table.ExecuteAsync(inserOperation);

            return true;
        }

        async public Task<bool> InsertEntitiesAsync<T>(string tableName, T[] entities)
            where T : ITableEntity
        {
            if (entities == null || entities.Length == 0)
                return true;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            var partitionKeys = entities.Select(e => e.PartitionKey).Distinct();
            if (partitionKeys != null)
            {
                foreach (string partitionKey in partitionKeys)
                {
                    if (String.IsNullOrWhiteSpace(partitionKey))
                        continue;

                    // Create the batch operation.
                    TableBatchOperation batchOperation = new TableBatchOperation();

                    int counter = 0;
                    foreach (var entity in entities.Where(e => e?.PartitionKey == partitionKey))
                    {
                        batchOperation.Insert(entity);
                        if (++counter == 100)
                        {
                            var result = await table.ExecuteBatchAsync(batchOperation);
                            batchOperation = new TableBatchOperation();
                            counter = 0;
                        }
                    }

                    if (counter > 0)
                    {
                        // Execute the batch operation.
                        var result = await table.ExecuteBatchAsync(batchOperation);
                    }
                }
            }
            return true;
        }

        #endregion

        #region Query

        async public Task<T[]> AllEntitiesAsync<T>(string tableName)
            where T : ITableEntity, new()
        {
            return await AllTableEntities<T>(tableName, String.Empty);
        }

        async public Task<T[]> AllEntitiesAsync<T>(string tableName, string partitionKey)
            where T : ITableEntity, new()
        {
            return await AllTableEntities<T>(tableName, partitionKey);
        }

        async private Task<T[]> AllTableEntities<T>(string tableName, string partitionKey)
            where T : ITableEntity, new()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            TableQuery<T> query = String.IsNullOrWhiteSpace(partitionKey) ?
                     new TableQuery<T>() :
                     new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            List<T> entities = new List<T>();
            foreach (var entity in await ExecuteQueryAsync<T>(table, query))
            {
                entities.Add(entity/*.ConvertTo<T>()*/);
            }

            return entities.ToArray();
        }

        async private Task<List<T>> ExecuteQueryAsync<T>(CloudTable table, TableQuery<T> query)
            where T : ITableEntity, new()
        {
            List<T> results = new List<T>();
            TableQuerySegment<T> currentSegment = null;

            if (query.TakeCount > 0)
            {
                // Damit Top Query funktioniert
                while (results.Count < query.TakeCount && (currentSegment == null || currentSegment.ContinuationToken != null))
                {
                    currentSegment = await table.ExecuteQuerySegmentedAsync(query, currentSegment != null ? currentSegment.ContinuationToken : null);
                    results.AddRange(currentSegment.Results);
                }
            }
            else
            {
                TableContinuationToken continuationToken = null;
                do
                {
                    currentSegment = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                    continuationToken = currentSegment.ContinuationToken;
                    results.AddRange(currentSegment.Results);
                }
                while (continuationToken != null);
            }

            return results;
        }

        #endregion

        #region Helper

        private string QueryComp(QueryComparer[] whereComparer, int index)
        {
            if (whereComparer == null)
                return QueryComparisons.Equal;

            switch (whereComparer[index])
            {
                case QueryComparer.Equal:
                    return QueryComparisons.Equal;
                case QueryComparer.GreaterThan:
                    return QueryComparisons.GreaterThan;
                case QueryComparer.GreaterThanOrEqual:
                    return QueryComparisons.GreaterThanOrEqual;
                case QueryComparer.LessThan:
                    return QueryComparisons.LessThan;
                case QueryComparer.LessThanOrEqual:
                    return QueryComparisons.LessThanOrEqual;
                case QueryComparer.NotEqual:
                    return QueryComparisons.NotEqual;
            }

            return QueryComparisons.Equal;
        }

        #endregion
    }
}

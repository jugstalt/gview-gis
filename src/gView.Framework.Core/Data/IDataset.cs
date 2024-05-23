using gView.Framework.Core.FDB;
using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IDataset : IDisposable, IMetadata, IErrorMessage, IPersistableLoadAsync
    {
        string ConnectionString
        {
            get;
        }
        Task<bool> SetConnectionString(string connectionString);

        string DatasetGroupName
        {
            get;
        }
        string DatasetName
        {
            get;
        }

        string ProviderName
        {
            get;
        }

        DatasetState State { get; }

        Task<bool> Open();

        Task<List<IDatasetElement>> Elements();
        Task<IDatasetElement> Element(string title);

        string Query_FieldPrefix { get; }
        string Query_FieldPostfix { get; }

        IDatabase Database { get; }

        Task RefreshClasses();
    }
}
using gView.Framework.Core.IO;
using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public interface IServiceableDataset : IPersistable
    {
        string Name { get; }
        string Provider { get; }

        List<IDataset> Datasets { get; }
        bool GenerateNew();
    }
}
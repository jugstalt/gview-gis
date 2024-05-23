using gView.Framework.Core.IO;
using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public interface ITableRelations : IEnumerable<ITableRelation>, IPersistable
    {
        void Add(ITableRelation tableRelation);
        bool Remove(ITableRelation tableRelation);

        IEnumerable<ITableRelation> GetRelations(IDatasetElement element);
    }
}
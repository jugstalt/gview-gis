using System.Collections.Generic;

namespace gView.Framework.Data
{
    public interface IGlobalRowIDFilter : IQueryFilter
    {
        List<long> IDs { get; set; }
        string RowIDWhereClause { get; }
    }
}

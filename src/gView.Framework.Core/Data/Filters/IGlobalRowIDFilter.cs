using System.Collections.Generic;

namespace gView.Framework.Core.Data.Filters
{
    public interface IGlobalRowIDFilter : IQueryFilter
    {
        List<long> IDs { get; set; }
        string RowIDWhereClause { get; }
    }
}

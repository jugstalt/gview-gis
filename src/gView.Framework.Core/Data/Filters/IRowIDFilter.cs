using System.Collections.Generic;

namespace gView.Framework.Core.Data.Filters
{
    public interface IRowIDFilter : IQueryFilter
    {
        List<int> IDs { get; set; }
        string RowIDWhereClause { get; }
        string IdFieldName { get; }
    }
}

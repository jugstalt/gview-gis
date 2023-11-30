using gView.Framework.Core.Data.Filters;
using gView.Framework.Data.Filters;

namespace gView.Framework.Data.Extensions
{
    public static class ExtensionsSql
    {
        static public string AppendWhereClause(this string filter, string appendFilter)
        {
            if (string.IsNullOrWhiteSpace(appendFilter))
            {
                return filter;
            }
            if (string.IsNullOrWhiteSpace(filter))
            {
                return appendFilter;
            }

            if (filter.ToLower().Contains(" or "))
            {
                filter = $"({filter})";
            }

            if (appendFilter.ToLower().Contains(" or "))
            {
                appendFilter = $"({appendFilter})";
            }

            return $"{filter} and {appendFilter}";
        }

        static public IQueryFilter AppendWhereClause(this IQueryFilter queryFilter, string appendFilter)
        {
            if (string.IsNullOrWhiteSpace(appendFilter))
            {
                return queryFilter;
            }

            if (queryFilter == null)
            {
                return new QueryFilter() { WhereClause = appendFilter };
            }

            var result = queryFilter.Clone() as IQueryFilter;
            if (result != null)
            {
                result.WhereClause = queryFilter.WhereClause.AppendWhereClause(appendFilter);
            }

            return result;
        }
    }
}

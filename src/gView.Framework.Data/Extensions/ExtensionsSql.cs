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

            // always in parentheses: an OR in one part (e.g. "a=1\tOR b=2" or "(a=1)OR(b=2)")
            // must not override the other part (e.g. the layer filter of the service)
            return $"({filter}) and ({appendFilter})";
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

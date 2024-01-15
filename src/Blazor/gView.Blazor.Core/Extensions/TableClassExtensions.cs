using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Data.Filters;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace gView.Blazor.Core.Extensions;

static public class TableClassExtensions
{
    static async public Task<IEnumerable<IRow>> GetSelectedRows(
            this ITableClass tableClass,
            ISelectionSet selectionSet,
            string orderByClause = "",
            int skip = 0,
            int take = 10000
        )
    {
        if (selectionSet is IIDSelectionSet { Count: > 0 } idSelectionSet)
        {
            var filter = new RowIDFilter(tableClass.IDFieldName, idSelectionSet.IDs.Skip(skip).Take(take).ToList())
            {
                SubFields = "*",
                OrderBy = orderByClause
            };

            using ICursor cursor = await tableClass.Search(filter);
            IRow? row = null;
            List<IRow> rows = new();

            while ((row = cursor switch
            {
                IFeatureCursor fCursor => await fCursor.NextFeature(),
                IRowCursor rCursor => await rCursor.NextRow(),
                _ => null
            }
                  ) != null)
            {
                rows.Add(row);
            }

            return rows;
        }

        return Array.Empty<IRow>();
    }

    static async public Task<bool> SelectRows(
            this ITableClass tableClass,
            ISelectionSet selectionSet,
            string whereClause
        )
    {
        if(selectionSet is IIDSelectionSet idSelectionSet)
        {
            idSelectionSet.Clear();

            var filter = new QueryFilter()
            {
                SubFields = tableClass.IDFieldName,
                WhereClause = whereClause
            };

            using ICursor cursor = await tableClass.Search(filter);
            IRow? row = null;
            List<IRow> rows = new();

            while ((row = cursor switch
            {
                IFeatureCursor fCursor => await fCursor.NextFeature(),
                IRowCursor rCursor => await rCursor.NextRow(),
                _ => null
            }
                  ) != null)
            {
                idSelectionSet.AddID(row.OID);
            }

            return true;
        }

        return false;
    }
}

using System.Collections.Generic;

namespace gView.Framework.Core.Data.Statistics
{
    public class StatisticDefinition
    {
        public StatisticType Type { get; set; }

        /// <summary>Name of the (already validated) table field the statistic is computed on.</summary>
        public string OnField { get; set; } = "";

        /// <summary>Name of the output column. Unique within a <see cref="StatisticsQuery"/>.</summary>
        public string OutName { get; set; } = "";

        /// <summary>Field type of the source field. Used to derive the output column type.</summary>
        public FieldType OnFieldType { get; set; }
    }

    public class StatisticsOrderBy
    {
        /// <summary>A group-by field or an <see cref="StatisticDefinition.OutName"/>.</summary>
        public string Name { get; set; } = "";
        public bool Descending { get; set; }
    }

    /// <summary>
    /// Describes an aggregation (ArcGIS <c>outStatistics</c> / <c>groupByFieldsForStatistics</c>).
    /// All names are validated against the table fields by the caller, so implementations
    /// (in-memory or SQL based) can use them without further sanitizing beyond quoting.
    /// The where clause / spatial filter is not part of this class; it is passed as <see cref="Filters.IQueryFilter"/>.
    /// </summary>
    public class StatisticsQuery
    {
        public IList<StatisticDefinition> Statistics { get; set; } = new List<StatisticDefinition>();

        /// <summary>Group-by fields (may be empty => one result row).</summary>
        public IList<string> GroupByFields { get; set; } = new List<string>();

        /// <summary>Field types of the group-by fields, same order as <see cref="GroupByFields"/>.</summary>
        public IList<FieldType> GroupByFieldTypes { get; set; } = new List<FieldType>();

        public IList<StatisticsOrderBy> OrderBy { get; set; } = new List<StatisticsOrderBy>();

        /// <summary>Number of result rows to skip (applies to aggregated rows).</summary>
        public int Offset { get; set; }

        /// <summary>Max. number of aggregated rows to return. <c>0</c> = no limit.</summary>
        public int Limit { get; set; }

        /// <summary>
        /// Max. number of source rows an in-memory implementation may read.
        /// <c>0</c> = no limit. SQL based implementations can ignore it.
        /// </summary>
        public int MaxScanRecords { get; set; }
    }

    public class StatisticsColumn
    {
        public string Name { get; set; } = "";
        public FieldType Type { get; set; }
        public int Size { get; set; }
        public string Alias { get; set; } = "";
    }

    public class StatisticsResult
    {
        /// <summary>Group-by columns first, then statistics columns.</summary>
        public IList<StatisticsColumn> Columns { get; set; } = new List<StatisticsColumn>();

        /// <summary>Column name => value (null for SQL NULL).</summary>
        public IList<IDictionary<string, object>> Rows { get; set; } = new List<IDictionary<string, object>>();

        /// <summary>True if more aggregated rows exist beyond <see cref="StatisticsQuery.Limit"/>.</summary>
        public bool ExceededLimit { get; set; }
    }
}

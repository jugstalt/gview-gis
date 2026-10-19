using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Data.Statistics;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    /// <summary>
    /// Implemented by table classes that can aggregate on their own (e.g. SQL <c>GROUP BY</c>).
    /// Table classes without this interface are aggregated in memory by
    /// <c>StatisticsAggregator</c>.
    /// </summary>
    public interface IStatisticsTableClass : ITableClass
    {
        /// <param name="filter">Where clause and spatial filter only; sub fields / limit are ignored.</param>
        /// <param name="query">Offset, Limit and OrderBy refer to the aggregated rows.</param>
        Task<StatisticsResult> ExecuteStatistics(IQueryFilter filter, StatisticsQuery query);
    }
}

using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.IO;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface ITableRelation : IPersistable
    {
        string RelationName { get; }
        IDatasetElement LeftTable { get; }
        IDatasetElement RightTable { get; }
        string LeftTableField { get; }
        string RightTableField { get; }

        string LogicalOperator { get; }

        Task<ICursor> GetLeftRows(string leftFields, object rightValue);
        Task<ICursor> GetRightRows(string rightFields, object leftValue);

        IQueryFilter GetLeftFilter(string leftFields, object rightValue);
        IQueryFilter GetRightFilter(string rightFields, object leftValue);
    }
}
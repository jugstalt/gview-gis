using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface ITableClass : IClass
    {
        //IQueryResult QueryResult { get ; set ; }
        //IQueryResult Search(IQueryFilter filter,bool queryGeometry);

        Task<ICursor> Search(IQueryFilter filter);
        Task<ISelectionSet> Select(IQueryFilter filter);

        IFieldCollection Fields { get; }
        IField FindField(string name);

        string IDFieldName { get; }
    }
}
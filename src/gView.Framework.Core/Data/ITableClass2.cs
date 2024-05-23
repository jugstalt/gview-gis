using gView.Framework.Core.Data.Filters;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface ITableClass2 : ITableClass
    {
        Task<int> ExecuteCount(IQueryFilter filter);
    }
}
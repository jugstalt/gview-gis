using gView.Framework.Core.Data.Filters;

namespace gView.Framework.Core.Data
{
    public interface IBeforeQueryEventHandler
    {
        event BeforeQueryEventHandler BeforeQuery;
        void FireBeforeQureyEvent(ref IQueryFilter filter);
    }
}
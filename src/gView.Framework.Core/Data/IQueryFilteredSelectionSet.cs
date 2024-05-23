using gView.Framework.Core.Data.Filters;

namespace gView.Framework.Core.Data
{
    public interface IQueryFilteredSelectionSet : ISelectionSet
    {
        IQueryFilter QueryFilter { get; }
    }
}
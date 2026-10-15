namespace gView.Framework.Core.Data
{
    /// <summary>
    /// Marker for table classes whose data source honors <see cref="Filters.IQueryFilter.BeginRecord"/>
    /// and <see cref="Filters.IQueryFilter.Limit"/> natively (e.g. with SQL LIMIT/OFFSET).
    /// Clients like OGC API Features fall back to skipping records in the cursor for classes
    /// without this marker.
    /// </summary>
    public interface ISupportsQueryPaging { }
}

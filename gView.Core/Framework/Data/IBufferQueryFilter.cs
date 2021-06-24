namespace gView.Framework.Data
{
    public interface IBufferQueryFilter : IQueryFilter
    {
        IQueryFilter RootFilter { get; }
        IFeatureClass RootFeatureClass { get; }

        double BufferDistance { get; }
        gView.Framework.Carto.GeoUnits BufferUnits { get; }
    }
}

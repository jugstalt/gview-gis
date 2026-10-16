using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Interoperability.OgcApi.Extensions;

namespace gView.Interoperability.OgcApi.Features;

/// <summary>
/// A layer of a map service exposed as OGC API collection ("c{layerId}").
/// Feature layers have items and a map, raster layers only a map.
/// </summary>
public sealed class OgcCollectionLayer
{
    public const string IdPrefix = "c";

    public OgcCollectionLayer(
                ILayer layer,
                string title,
                string? description,
                IEnvelope? nativeExtent,
                ISpatialReference? spatialReference)
    {
        Layer = layer;
        Title = title;
        Description = description;
        NativeExtent = nativeExtent;
        SpatialReference = spatialReference;
    }

    public static string ToCollectionId(int layerId) => $"{IdPrefix}{layerId}";

    public string Id => ToCollectionId(LayerId);
    public int LayerId => Layer.ID;
    public string Title { get; }
    public string? Description { get; }

    public ILayer Layer { get; }
    public IFeatureLayer? FeatureLayer => Layer as IFeatureLayer;
    public IFeatureClass? FeatureClass => FeatureLayer?.Class as IFeatureClass;

    /// <summary>
    /// Feature collections have items, raster collections only a map
    /// </summary>
    public bool HasItems => FeatureClass is not null;

    /// <summary>
    /// The extent of the data in <see cref="SpatialReference"/>
    /// </summary>
    public IEnvelope? NativeExtent { get; }

    /// <summary>
    /// The native spatial reference of the data (layer class or the map's layer default)
    /// </summary>
    public ISpatialReference? SpatialReference { get; }

    public string? FilterWhereClause => FeatureLayer?.FilterQuery?.WhereClause;

    /// <summary>
    /// The visible attribute fields of the layer (no id, no geometry)
    /// </summary>
    public IEnumerable<IField> PropertyFields
    {
        get
        {
            if (FeatureLayer is null || FeatureClass is null)
            {
                return [];
            }

            var fields = FeatureLayer.Fields is not null && FeatureLayer.Fields.Count > 0
                ? FeatureLayer.Fields
                : FeatureClass.Fields;

            return fields?.ToEnumerable()
                          .Where(f => f.visible)
                          .Where(f => !f.IsIdOrGeometryField(FeatureClass))
                       ?? [];
        }
    }

    /// <summary>
    /// The SQL dialect of the where clauses (date/boolean literals)
    /// </summary>
    public SqlDialect SqlDialect
        => (FeatureClass as ISupportsSqlDialect)?.SqlDialect ?? SqlDialect.Unknown;

    /// <summary>
    /// Fields usable in property filter query parameters and CQL2 filters (Part 3 queryables)
    /// </summary>
    public IEnumerable<IField> Queryables
        => PropertyFields.Where(f => f.IsQueryableType(SqlDialect));

    public IField? FindQueryable(string name)
        => Queryables.FirstOrDefault(f => f.name == name);
}

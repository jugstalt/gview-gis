using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Interoperability.OgcApi.Extensions;

namespace gView.Interoperability.OgcApi.Features;

/// <summary>
/// A feature layer of a map service exposed as OGC API collection ("c{layerId}").
/// </summary>
public sealed class OgcCollectionLayer
{
    public const string IdPrefix = "c";

    public OgcCollectionLayer(
                int layerId,
                string title,
                string? description,
                IFeatureLayer featureLayer,
                IFeatureClass featureClass,
                ISpatialReference? spatialReference)
    {
        LayerId = layerId;
        Title = title;
        Description = description;
        FeatureLayer = featureLayer;
        FeatureClass = featureClass;
        SpatialReference = spatialReference;
    }

    public static string ToCollectionId(int layerId) => $"{IdPrefix}{layerId}";

    public string Id => ToCollectionId(LayerId);
    public int LayerId { get; }
    public string Title { get; }
    public string? Description { get; }
    public IFeatureLayer FeatureLayer { get; }
    public IFeatureClass FeatureClass { get; }

    /// <summary>
    /// The native spatial reference of the data (feature class or the map's layer default)
    /// </summary>
    public ISpatialReference? SpatialReference { get; }

    public string? FilterWhereClause => FeatureLayer.FilterQuery?.WhereClause;

    /// <summary>
    /// The visible attribute fields of the layer (no id, no geometry)
    /// </summary>
    public IEnumerable<IField> PropertyFields
    {
        get
        {
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
    /// Fields usable as property filter query parameters (Part 3 queryables)
    /// </summary>
    public IEnumerable<IField> Queryables
        => PropertyFields.Where(f => f.IsQueryableType());

    public IField? FindQueryable(string name)
        => Queryables.FirstOrDefault(f => f.name == name);
}

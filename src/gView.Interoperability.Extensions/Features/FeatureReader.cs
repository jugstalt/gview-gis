#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Extensions;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace gView.Interoperability.Extensions.Features;

/// <summary>
/// Reads the features of a collection (OGC API items, WFS GetFeature): only the property fields
/// of the layer, the layer's filter query, paging and the geometries in the output CRS.
/// The features are streamed, so writers can encode them one by one.
/// </summary>
public static class FeatureReader
{
    /// <summary>
    /// Reads at most <paramref name="take"/> features, starting after <paramref name="offset"/> features.
    /// The where clause of <paramref name="filter"/> must already contain the layer filter query
    /// (see <see cref="CombineWhereClauses"/>).
    /// </summary>
    /// <param name="outSref">The CRS of the geometries (gView x/y order), null: the CRS of the data</param>
    /// <param name="take">The maximum number of features, 0 or less: no limit</param>
    /// <exception cref="ArgumentException">The collection has no feature class</exception>
    public static async IAsyncEnumerable<OgcFeature> ReadAsync(
                IServiceMap? map,
                OgcCollectionLayer collection,
                IQueryFilter filter,
                ISpatialReference? outSref,
                int offset,
                int take,
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var featureClass = collection.FeatureClass
            ?? throw new ArgumentException($"Collection '{collection.Id}' has no features");
        var propertyFields = collection.PropertyFields.ToList();
        var datumTransformations = map?.Display?.DatumTransformations;

        if (String.IsNullOrWhiteSpace(filter.WhereClause))
        {
            filter.WhereClause = "1=1";
        }

        filter.SubFields = String.Join(",",
            new[] { featureClass.IDFieldName }
                .Concat(propertyFields.Select(f => f.name))
                .Where(n => !String.IsNullOrEmpty(n))
                .Distinct());
        if (!String.IsNullOrEmpty(featureClass.ShapeFieldName))
        {
            filter.AddField(featureClass.ShapeFieldName);
        }

        #region Paging

        int skip = 0;
        if (featureClass is ISupportsQueryPaging)
        {
            filter.BeginRecord = offset + 1;  // 1-based by IQueryFilter definition
            filter.Limit = Math.Max(take, 0);  // 0 => no limit
            if (!String.IsNullOrEmpty(featureClass.IDFieldName) && String.IsNullOrEmpty(filter.OrderBy))
            {
                filter.OrderBy = featureClass.IDFieldName;  // stable pages
            }
        }
        else
        {
            // data source can't page => read from the beginning and skip the records here
            filter.BeginRecord = 1;
            filter.Limit = take > 0 ? offset + take : 0;
            skip = offset;
        }

        #endregion

        #region Spatial Reference

        IGeometricTransformer? transformer = null;

        if (outSref is null)
        {
            // geometries in the CRS of the data
        }
        else if (featureClass.SpatialReference is not null)
        {
            filter.SetFeatureSpatialReference(outSref, datumTransformations);
        }
        else if (collection.SpatialReference is not null)
        {
            // feature class without spatial reference => use the map's layer default spatial reference
            transformer = GeometricTransformerFactory.Create(datumTransformations);
            transformer.SetSpatialReferences(collection.SpatialReference, outSref);

            if (filter is ISpatialFilter spatialFilter
                && spatialFilter.FilterSpatialReference is not null
                && spatialFilter.FilterSpatialReference.EpsgCode != collection.SpatialReference.EpsgCode)
            {
                using var filterTransformer = GeometricTransformerFactory.Create(datumTransformations);
                filterTransformer.SetSpatialReferences(spatialFilter.FilterSpatialReference, collection.SpatialReference);
                spatialFilter.Geometry = filterTransformer.Transform2D(spatialFilter.Geometry) as IGeometry;
                spatialFilter.FilterSpatialReference = collection.SpatialReference;
            }
        }

        #endregion

        try
        {
            using var cursor = await featureClass.GetFeatures(filter);
            if (cursor is null)
            {
                yield break;
            }

            int count = 0;
            IFeature feature;
            while ((take <= 0 || count < take) && (feature = await cursor.NextFeature()) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (skip > 0)
                {
                    skip--;
                    continue;
                }

                var ogcFeature = new OgcFeature()
                {
                    Geometry = transformer is not null && feature.Shape is not null
                        ? transformer.Transform2D(feature.Shape) as IGeometry
                        : feature.Shape
                };

                foreach (var fieldValue in feature.Fields)
                {
                    if (fieldValue.Name == featureClass.IDFieldName)
                    {
                        ogcFeature.Id = fieldValue.Value is DBNull ? null : fieldValue.Value;
                    }
                }

                foreach (var field in propertyFields)
                {
                    var value = feature[field.name];
                    ogcFeature.Properties.Add(new KeyValuePair<string, object?>(field.name, value is DBNull ? null : value));
                }

                if (ogcFeature.Id is null && feature.OID > 0)
                {
                    ogcFeature.Id = feature.OID;
                }

                count++;
                yield return ogcFeature;
            }
        }
        finally
        {
            transformer?.Dispose();
        }
    }

    /// <summary>
    /// Reads all features of <see cref="ReadAsync"/> into a list
    /// </summary>
    public static async System.Threading.Tasks.Task<List<OgcFeature>> ReadListAsync(
                IServiceMap? map,
                OgcCollectionLayer collection,
                IQueryFilter filter,
                ISpatialReference? outSref,
                int offset,
                int take)
    {
        var result = new List<OgcFeature>();
        await foreach (var feature in ReadAsync(map, collection, filter, outSref, offset, take))
        {
            result.Add(feature);
        }
        return result;
    }

    /// <summary>
    /// Combines where clauses with AND, each in brackets. Empty clauses are ignored.
    /// </summary>
    public static string CombineWhereClauses(params string?[] whereClauses)
    {
        var clauses = whereClauses.Where(w => !String.IsNullOrWhiteSpace(w)).ToArray();

        return clauses.Length switch
        {
            0 => "",
            1 => clauses[0]!,
            _ => String.Join(" AND ", clauses.Select(c => $"({c})"))
        };
    }
}

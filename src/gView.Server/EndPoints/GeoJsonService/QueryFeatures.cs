#nullable enable

using gView.Framework.Common.Extensions;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;
using gView.Framework.GeoJsonService.Extensions;
using gView.Framework.Geometry;
using gView.GeoJsonService.DTOs;
using gView.Server.AppCode.Extensions;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService;

public class QueryFeatures : BaseApiEndpoint
{
    public QueryFeatures()
        : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.GetFeatures}/{{id}}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.GetFeatures}/{{id}}"
                ],
                Handler,
                HttpMethod.Get | HttpMethod.Post
            )
    {
    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManagerService,
                [FromServices] MapServiceManager mapServerService,
                [FromServices] ILogger<QueryFeatures> logger,
                int id,
                string folder = "",
                string service = ""
            ) => HandleSecureAsync<GetFeaturesRequest>(httpContext, mapServerService, loginManagerService, logger, folder, service,
            async (serviceRequestContext, mapService, identity, queryRequest) =>
            {
                using var serviceMap = (await mapServerService.Instance.GetServiceMapAsync(mapService)).ThrowIfNull();

                int maxRecordCount = queryRequest.Command == QueryCommand.IdsOnly  // return all Ids!
                                ? int.MaxValue
                                : serviceMap.MapServiceProperties.MaxRecordCount;
                string filterQuery;

                var tableClasses = serviceMap.FindTableClass(id, out filterQuery);
                if (tableClasses is null || tableClasses.Count == 0)
                {
                    // before, an unknown layer or a group layer answered an empty feature collection
                    throw serviceMap.MapElements.Any(e => e.ID == id)
                        ? new MapServerException($"Layer {id} is not queryable", 400)
                        : new MapServerException($"Unknown layer {id}", 404);
                }

                string? requestWhereClause = queryRequest.Filter?
                            .WhereClauseWithResolveParameters();

                // CQL2 (text or JSON): translated per table class, against its fields and SQL dialect
                var cql2Predicate = queryRequest.Filter?.Cql2Predicate();

                // object ids are numbers: anything else is a client error (400), not an internal one
                var objectIds = queryRequest.ObjectIds?
                            .Where(oid => !String.IsNullOrWhiteSpace(oid))
                            .Select(oid => int.TryParse(oid.Trim(), out var value)
                                ? value
                                : throw new MapServerException($"Invalid object id '{oid}': object ids are integers"))
                            .ToList();

                if (queryRequest.Command == QueryCommand.Distinct && queryRequest.SpatialFilter is not null)
                {
                    throw new MapServerException("DISTINCT can't be combined with a spatial filter");
                }

                // statistics: one table (the rows of several tables can't be aggregated together); having is a CQL2
                // filter on the result rows, applied here
                GetFeaturesStatisticsResponse? statisticsResponse = null;
                var having = queryRequest.Command == QueryCommand.Statistics
                    ? AttributeFilterExtensions.Cql2PredicateOf(queryRequest.Having)
                    : null;
                if (queryRequest.Command == QueryCommand.Statistics && tableClasses.Count != 1)
                {
                    throw new MapServerException("Statistics can't be used with aggregated tables");
                }

                int featureCount = 0;
                int pageSize = int.MaxValue;
                var geoJsonFeatures = new List<gView.GeoJsonService.DTOs.Feature>();
                string? objectIdFieldName = null;
                ISpatialReference outSref = queryRequest.OutCRS.ToSpatialReferenceOrDefault();

                foreach (var tableClass in tableClasses)
                {
                    requestWhereClause?.CheckWhereClauseForSqlInjection(tableClass);
                    string? cql2WhereClause = cql2Predicate is not null
                        ? tableClass.Cql2ToWhereClause(cql2Predicate)
                        : null;

                    objectIdFieldName = tableClass.IDFieldName;

                    #region Filter

                    IQueryFilter? filter;
                    string? objectIdsWhereClause = null;

                    if (queryRequest.SpatialFilter is not null)
                    {
                        var spatialFilter = new Framework.Data.Filters.SpatialFilter();

                        if (queryRequest.SpatialFilter.Geometry is not null
                           && queryRequest.SpatialFilter.BBox is not null)
                        {
                            throw new MapServerException("Invalid spatial filter. Set either geometry or bbox");
                        }
                        else if (queryRequest.SpatialFilter.Geometry is not null)
                        {
                            spatialFilter.Geometry = queryRequest.SpatialFilter.Geometry.ToGeometry();
                        }
                        else if (queryRequest.SpatialFilter.BBox is not null)
                        {
                            spatialFilter.Geometry = queryRequest.SpatialFilter.BBox.ToEnvelope();
                        }

                        spatialFilter.FilterSpatialReference =
                             queryRequest.SpatialFilter.CRS.ToSpatialReferenceOrDefault();
                        spatialFilter.Geometry.CheckLatitudes(spatialFilter.FilterSpatialReference, "spatialFilter");
                        spatialFilter.SpatialRelation = queryRequest.SpatialFilter.Operator.ToSpatialRelation();

                        filter = spatialFilter;
                    }
                    else if (queryRequest.Command == QueryCommand.Distinct)
                    {
                        queryRequest.OutFields = queryRequest.OutFields.ProjectNamesAndCheckIfFieldsExists(tableClass).ToArray();
                        filter = new DistinctFilter(queryRequest.OutFields.First());
                    }
                    else if (objectIds?.Count > 0)
                    {
                        filter = new RowIDFilter(tableClass.IDFieldName, objectIds);
                    }
                    else
                    {
                        filter = new QueryFilter();
                    }

                    // object ids together with a spatial filter or DISTINCT: as a where clause (before, they were ignored)
                    if (objectIds?.Count > 0 && filter is not RowIDFilter)
                    {
                        if (String.IsNullOrEmpty(tableClass.IDFieldName))
                        {
                            throw new MapServerException("Can't query object ids: the table has no object id field");
                        }
                        objectIdsWhereClause = $"{tableClass.IDFieldName} in ({String.Join(",", objectIds)})";
                    }

                    #endregion

                    #region Prepair Where Filter

                    filter.WhereClause = requestWhereClause.AppendWhereClause(cql2WhereClause)
                                                           .AppendWhereClause(filterQuery)
                                                           .AppendWhereClause(objectIdsWhereClause)
                                                           .OrTake("1=1");

                    #endregion

                    #region Subfields

                    if (queryRequest.Command == QueryCommand.CountOnly)
                    {
                        if (tableClass is ITableClass2 && !String.IsNullOrWhiteSpace(tableClass.IDFieldName))
                        {
                            #region Fast Count

                            featureCount += await ((ITableClass2)tableClass).ExecuteCount(filter);
                            continue;

                            #endregion
                        }
                        else
                        {
                            filter.SubFields = !String.IsNullOrWhiteSpace(tableClass.IDFieldName) ? tableClass.IDFieldName : "*";
                        }
                    }
                    else if (queryRequest.Command == QueryCommand.IdsOnly)
                    {
                        if (String.IsNullOrEmpty(tableClass.IDFieldName))
                        {
                            throw new Exception("Can't query IdsOnly. Table has no ID-Field.");
                        }

                        filter.SubFields = tableClass.IDFieldName;
                    }
                    else if (queryRequest.Command == QueryCommand.Distinct)
                    {
                        if (queryRequest.ReturnGeometry != GeometryResult.None)
                        {
                            throw new MapServerException("Geometry is not supported with DISTINCT.");
                        }
                    }
                    else if (queryRequest.Command == QueryCommand.Statistics)
                    {
                        // the statistics name their fields
                    }
                    else
                    {
                        var outFields = queryRequest.OutFields
                                                    .ProjectNamesAndCheckAllowedFunctions(tableClass, false);

                        filter.SubFields = String.Join(",", outFields);
                        if (queryRequest.ReturnGeometry != GeometryResult.None)
                        {
                            if (tableClass is IFeatureClass featureClass &&
                                !filter.HasField(featureClass.ShapeFieldName))
                            {
                                filter.AddField(featureClass.ShapeFieldName);
                            }
                        }
                    }

                    #endregion

                    #region Spatial Reference

                    filter.SetFeatureSpatialReference(outSref, serviceMap.Display?.DatumTransformations);

                    #endregion

                    #region Limit/Begin/Order

                    pageSize = queryRequest.Command == QueryCommand.CountOnly
                        ? int.MaxValue                                 // count queries always consider all matching features
                        : queryRequest.Limit.HasValue && queryRequest.Limit.Value > 0
                            ? Math.Min(queryRequest.Limit.Value, maxRecordCount)
                            : maxRecordCount;
                    // select reads one more feature: it tells whether there are more (exceededLimit)
                    filter.Limit = queryRequest.Command == QueryCommand.Select && pageSize < int.MaxValue
                        ? pageSize + 1
                        : pageSize;

                    filter.BeginRecord = queryRequest.Command == QueryCommand.CountOnly
                                ? 1
                                : queryRequest.Offset.HasValue
                                    ? queryRequest.Offset.Value + 1
                                    : 1;  // Start is 1 by IQueryFilter definition

                    filter.OrderBy = queryRequest.OrderByFields is not null && queryRequest.Command != QueryCommand.Statistics  // statistics order their rows
                                ? queryRequest.OrderByFields.OrderByClause(tableClass)
                                : "";

                    #endregion

                    bool transform = false;

                    using (var geoTransfromer = GeometricTransformerFactory.Create(serviceMap.Display?.DatumTransformations))
                    {
                        if (tableClass is IFeatureClass featureClass
                            && featureClass.SpatialReference == null
                            && filter.FeatureSpatialReference != null
                            && serviceMap.LayerDefaultSpatialReference != null)
                        {
                            geoTransfromer.SetSpatialReferences(serviceMap.LayerDefaultSpatialReference, filter.FeatureSpatialReference);
                            transform = true;

                            #region Transform Filter Geometry

                            if (filter is ISpatialFilter spatialFilter
                                && spatialFilter.FilterSpatialReference != null
                                && !serviceMap.LayerDefaultSpatialReference.EpsgCode.Equals(spatialFilter.FilterSpatialReference?.EpsgCode))
                            {
                                using (var filterTransformer = GeometricTransformerFactory.Create(serviceMap.Display?.DatumTransformations))
                                {
                                    filterTransformer.SetSpatialReferences(spatialFilter.FilterSpatialReference, serviceMap.LayerDefaultSpatialReference);
                                    spatialFilter.Geometry = filterTransformer.Transform2D(spatialFilter.Geometry) as IGeometry;
                                }
                            }

                            #endregion
                        }

                        if (queryRequest.Command == QueryCommand.Statistics)
                        {
                            int statisticsLimit = queryRequest.Limit is > 0
                                ? (maxRecordCount > 0 ? Math.Min(queryRequest.Limit.Value, maxRecordCount) : queryRequest.Limit.Value)
                                : Math.Max(0, maxRecordCount);
                            var statisticsQuery = queryRequest.ToStatisticsQuery(tableClass, statisticsLimit, withHaving: having is not null);

                            var statisticsResult = tableClass is IStatisticsTableClass statisticsTableClass
                                ? await statisticsTableClass.ExecuteStatistics(filter, statisticsQuery)
                                : await gView.Framework.Data.Statistics.StatisticsAggregator.ExecuteAsync(tableClass, filter, statisticsQuery);

                            statisticsResponse = statisticsResult.ToStatisticsResponse(having, queryRequest.Offset ?? 0, statisticsLimit);
                            continue;
                        }

                        #region Query

                        using (var cursor = await tableClass.Search(filter))
                        {
                            if (cursor is IFeatureCursor)
                            {
                                IFeature feature;
                                IFeatureCursor featureCursor = (IFeatureCursor)cursor;

                                while ((feature = await featureCursor.NextFeature()) != null)
                                {
                                    featureCount++;

                                    if (queryRequest.Command == QueryCommand.CountOnly)
                                    {
                                        continue;  // slow-count fallback: only the number of features matters
                                    }

                                    if (transform && queryRequest.ReturnGeometry != GeometryResult.None)
                                    {
                                        feature.Shape = geoTransfromer.Transform2D(feature.Shape) as IGeometry;
                                    }

                                    var geoJsonFeature = feature.ToGeoJsonFeature(objectIdFieldName, queryRequest.ReturnGeometry,
                                        new GeometryOptions(queryRequest.MaxAllowableOffset, queryRequest.GeometryPrecision));
                                    geoJsonFeatures.Add(geoJsonFeature);
                                }
                            }
                        }

                        #endregion
                    }
                }

                if (queryRequest.Command == QueryCommand.Statistics)
                {
                    return statisticsResponse!;
                }
                if (queryRequest.Command == QueryCommand.CountOnly)
                {
                    return new GetFeaturesCountOnlyResponse() { Count = featureCount };
                }
                else if (queryRequest.Command == QueryCommand.IdsOnly)
                {
                    if (String.IsNullOrEmpty(objectIdFieldName))
                    {
                        throw new MapServerException("Can't return Object Ids: not Object ID field found in table.");
                    }
                    return new GetFeaturesIdsOnlyResponse()
                    {
                        ObjectIdFieldName = objectIdFieldName,
                        ObjectIds = geoJsonFeatures
                                      .Select(f => Convert.ToInt32(f.Properties[objectIdFieldName]))
                                      .ToArray()
                    };
                }
                else if (queryRequest.Command == QueryCommand.Distinct == true)
                {
                    var distinctField = queryRequest.OutFields.First();
                    return new GetFeaturesDistinctResponse()
                    {
                        DistinctField = distinctField,
                        DistinctValues = geoJsonFeatures
                                            .Select(f => f.Properties[distinctField]!)
                                            .ToArray()
                    };
                }

                bool exceededLimit = geoJsonFeatures.Count > pageSize;
                if (exceededLimit)
                {
                    geoJsonFeatures.RemoveRange(pageSize, geoJsonFeatures.Count - pageSize);
                }

                return new FeatureCollection()
                {
                    CRS = CoordinateReferenceSystem.CreateByName(outSref.Name),
                    Features = geoJsonFeatures,
                    ExceededLimit = exceededLimit
                };
            });
}

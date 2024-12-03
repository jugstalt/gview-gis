#nullable enable

using gView.Framework.Common.Extensions;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;
using gView.Framework.GeoJsonService.Extensions;
using gView.Framework.Geometry;
using gView.GeoJsonService.DTOs;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService;

public class GetFeatures : BaseApiEndpoint
{
    public GetFeatures()
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
                int id,
                string folder = "",
                string service = ""
            ) => HandleSecureAsync<GetFeaturesRequest>(httpContext, loginManagerService,
            async (identity, queryRequest) =>
            {
                var mapService = mapServerService.Instance.GetMapService(service, folder);
                if (mapService == null)
                {
                    throw new MapServerException("Unknown service");
                }
                if (await mapService.HasAnyAccess(identity) == false)
                {
                    throw new MapServerAuthException("service forbidden");
                }

                using var serviceMap = await mapServerService.Instance.GetServiceMapAsync(mapService);

                int maxRecordCount = queryRequest.Command == QueryCommand.IdsOnly  // return all Ids!
                                ? int.MaxValue
                                : serviceMap.MapServiceProperties.MaxRecordCount;
                string filterQuery;

                var tableClasses = FindTableClass(serviceMap, id.ToString(), out filterQuery);
                if (tableClasses is null)
                {
                    throw new MapServerException($"Can't find any tableclass with layer {id}");
                }
                //if (tableClasses.Count > 1)
                //{
                //    throw new MapServerException("FeatureService can't be used with aggregated feature classes");
                //}

                if (tableClasses is null)
                {
                    throw new MapServerException($"Layer with id {id} is not queryable");
                }

                string? requestWhereClause = queryRequest.Filter?
                            .WhereClauseWithResolveParameters()
                            .CheckWhereClauseForSqlInjection();

                int featureCount = 0;
                var geoJsonFeatures = new List<gView.GeoJsonService.DTOs.Feature>();
                string? objectIdFieldName = null;
                ISpatialReference? outSref = null;

                foreach (var tableClass in tableClasses)
                {
                    objectIdFieldName = tableClass.IDFieldName;

                    #region Filter

                    IQueryFilter? filter;

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
                             queryRequest.SpatialFilter.CRS is not null
                             ? SpatialReference.FromID(queryRequest.SpatialFilter.CRS.ToSpatialReferenceName())
                             : serviceMap.Display.SpatialReference;

                        filter = spatialFilter;
                    }
                    else if (queryRequest.Command == QueryCommand.Distinct)
                    {
                        queryRequest.OutFields = queryRequest.OutFields.ProjectNamesAndCheckIfFieldsExists(tableClass).ToArray();
                        filter = new DistinctFilter(queryRequest.OutFields.First());
                    }
                    else if (queryRequest.ObjectIds?.Any() == true)
                    {
                        filter = new RowIDFilter(tableClass.IDFieldName,
                            queryRequest.ObjectIds.Select(id => int.Parse(id)).ToList());
                    }
                    else
                    {
                        filter = new QueryFilter();
                    }

                    #endregion

                    #region Prepair Where Filter

                    filter.WhereClause = requestWhereClause.AppendWhereClause(filterQuery).OrTake("1=1");

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
                        if (queryRequest.ReturnGeometry == true)
                        {
                            throw new MapServerException("Geometry is not supported with DISTINCT.");
                        }
                    }
                    else
                    {
                        var outFields = queryRequest.OutFields
                                                    .ProjectNamesAndCheckAllowedFunctions(tableClass, false);

                        filter.SubFields = String.Join(",", outFields);
                        if (queryRequest.ReturnGeometry == true)
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

                    if (queryRequest.OutCRS is not null)
                    {
                        filter.FeatureSpatialReference = SpatialReference.FromID(queryRequest.OutCRS.ToSpatialReferenceName());
                    }
                    else if (tableClass is IFeatureClass)
                    {
                        filter.FeatureSpatialReference = ((IFeatureClass)tableClass).SpatialReference;
                    }

                    outSref ??= filter.FeatureSpatialReference;
                    if (filter.FeatureSpatialReference != null)
                    {
                        if (outSref is not null && outSref.Name != filter.FeatureSpatialReference.Name)
                        {
                            throw new MapServerException("Mixed spatial references in response are not allowed");
                        }
                        outSref ??= filter.FeatureSpatialReference;
                    }

                    #endregion

                    #region Limit/Begin/Order

                    filter.Limit = queryRequest.Limit.HasValue && queryRequest.Limit.Value > 0
                        ? Math.Min(queryRequest.Limit.Value, maxRecordCount)
                        : maxRecordCount;

                    filter.BeginRecord = queryRequest.Offset.HasValue
                                ? queryRequest.Offset.Value + 1
                                : 1;  // Start is 1 by IQueryFilter definition

                    filter.OrderBy = queryRequest.OrderByFields is not null
                                ? String.Join(",", queryRequest.OrderByFields.ProjectNamesAndCheckIfFieldsExists(tableClass))
                                : "";

                    #endregion

                    bool transform = false;

                    using (var geoTransfromer = GeometricTransformerFactory.Create())
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
                                using (var filterTransformer = GeometricTransformerFactory.Create())
                                {
                                    filterTransformer.SetSpatialReferences(spatialFilter.FilterSpatialReference, serviceMap.LayerDefaultSpatialReference);
                                    spatialFilter.Geometry = filterTransformer.Transform2D(spatialFilter.Geometry) as IGeometry;
                                }
                            }

                            #endregion
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

                                    var geoJsonFeature = new gView.GeoJsonService.DTOs.Feature();

                                    #region Shape

                                    if (queryRequest.ReturnGeometry != true)
                                    {
                                        feature.Shape = null;  // do not geometry
                                    }
                                    else if (transform)
                                    {
                                        feature.Shape = geoTransfromer.Transform2D(feature.Shape) as IGeometry;
                                    }
                                    if (queryRequest.ReturnGeometryAsBBox == true)
                                    {
                                        feature.Shape = feature.Shape?.Envelope;
                                    }

                                    geoJsonFeature.Geometry = feature.Shape?.ToGeoJsonGeometry();
                                    geoJsonFeature.BBox = feature.Shape?.Envelope.ToBBox();

                                    #endregion

                                    #region Properties

                                    foreach (var field in feature.Fields)
                                    {
                                        if (field.Name == objectIdFieldName)
                                        {
                                            geoJsonFeature.Oid = field.Value;
                                        }

                                        geoJsonFeature.Properties[field.Name] = field.Value;
                                    }

                                    #endregion

                                    geoJsonFeatures.Add(geoJsonFeature);
                                }
                            }
                        }

                        #endregion
                    }
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
                                            .Select(f => f.Properties[distinctField])
                                            .ToArray()
                    };
                }

                return new FeatureCollection()
                {
                    CRS = outSref is not null
                            ? CoordinateReferenceSystem.CreateByName(outSref.Name)
                            : null,
                    Features = geoJsonFeatures
                };
            });

    private const bool UseTOC = true;

    static private List<ITableClass>? FindTableClass(IServiceMap map, string id, out string filterQuery)
    {
        filterQuery = String.Empty;
        if (map == null)
        {
            return null;
        }

        List<ITableClass> classes = new List<ITableClass>();

        foreach (ILayer element in MapServerHelper.FindMapLayers(map, UseTOC, id))
        {
            if (element.Class is ITableClass tableClass)
            {
                classes.Add(tableClass);
            }

            if (element is IFeatureLayer)
            {
                if (((IFeatureLayer)element).FilterQuery != null)
                {
                    string fquery = ((IFeatureLayer)element).FilterQuery.WhereClause;
                    if (String.IsNullOrWhiteSpace(filterQuery))
                    {
                        filterQuery = fquery;
                    }
                    else if (filterQuery != fquery)
                    {
                        filterQuery = $"({filterQuery}) AND ({fquery})";
                    }
                }
            }
        }
        return classes;
    }
}

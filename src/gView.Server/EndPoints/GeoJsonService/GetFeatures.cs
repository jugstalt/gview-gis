#nullable enable

using gView.Framework.Common.Extensions;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.OGC.GeoJson;
using gView.GeoJsonService.DTOs;
using gView.Interoperability.GeoServices.Exceptions;
using gView.Interoperability.GeoServices.Extensions;
using gView.Interoperability.GeoServices.Rest.DTOs;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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

                int maxRecordCount = queryRequest.IdsOnly == true  // return all Ids!
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

                // Todo: resolve parameters
                string? requestWhereClause = queryRequest.Filter?.WhereClause;
                if (!String.IsNullOrWhiteSpace(requestWhereClause))
                {
                    requestWhereClause.CheckWhereClauseForSqlInjection();
                }

                int featureCount = 0;
                List<IFeature> returnGeoJsonFeatures = new List<IFeature>();
                string? objectIdFieldName = null;

                foreach (var tableClass in tableClasses)
                {
                    objectIdFieldName = tableClass.IDFieldName;
                    //GeometryType? jsonGeometryType;
                    //if (tableClass is IFeatureClass)
                    //{
                    //    var geometryType = ((IFeatureClass)tableClass).GeometryType;
                    //    if (geometryType == Framework.Core.Geometry.GeometryType.Unknown)
                    //    {
                    //        var featureLayer = serviceMap.MapElements.FirstOrDefault(l => l.ID == id) as IFeatureLayer;
                    //        if (featureLayer != null)
                    //        {
                    //            geometryType = featureLayer.LayerGeometryType;
                    //        }
                    //    }

                    //    jsonGeometryType = JsonLayerDTO.ToGeometryType(geometryType);
                    //}

                    #region Filter

                    IQueryFilter? filter;

                    if (queryRequest.SpatialFilter?.Geometry is not null)
                    {
                        filter = new Framework.Data.Filters.SpatialFilter();
                        //var jsonGeometry = query.Geometry.ToJsonGeometry();
                        //var filterGeometry = jsonGeometry.ToGeometry();

                        //((SpatialFilter)filter).Geometry = filterGeometry;
                        //((SpatialFilter)filter).FilterSpatialReference =
                        //    SRef(query.InSRef) ??
                        //    (filterGeometry.Srs > 0 ? SpatialReference.FromID($"epsg:{filterGeometry.Srs}") : null);
                    }
                    else if (queryRequest.Distinct == true)
                    {
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

                    if (queryRequest.CountOnly == true)
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
                    else if (queryRequest.IdsOnly == true)
                    {
                        if (String.IsNullOrEmpty(tableClass.IDFieldName))
                        {
                            throw new Exception("Can't query IdsOnly. Table has no ID-Field.");
                        }

                        filter.SubFields = tableClass.IDFieldName;
                    }
                    else if (queryRequest.Distinct == true)
                    {
                        if (queryRequest.ReturnGeometry == true)
                        {
                            throw new MapServerException("Geometry is not supported with DISTINCT.");
                        }
                    }
                    else
                    {
                        var outFields = queryRequest.OutFields
                                                    .CheckAllowedFunctions(tableClass, false);

                        filter.SubFields = String.Join(",", outFields);
                        if (queryRequest.ReturnGeometry == true)
                        {
                            if (tableClass is IFeatureClass && !filter.HasField(((IFeatureClass)tableClass).ShapeFieldName))
                            {
                                filter.AddField(((IFeatureClass)tableClass).ShapeFieldName);
                            }
                        }
                    }

                    #endregion

                    #region Spatial Reference

                    if (!String.IsNullOrEmpty(queryRequest.OutCRS))
                    {
                        filter.FeatureSpatialReference = Framework.Geometry.SpatialReference.FromID(queryRequest.OutCRS);
                    }
                    else if (tableClass is IFeatureClass)
                    {
                        filter.FeatureSpatialReference = ((IFeatureClass)tableClass).SpatialReference;
                    }
                    //if (filter.FeatureSpatialReference != null)
                    //{
                    //    try
                    //    {
                    //        featureSref = new JsonSpatialReferenceDTO()
                    //        {
                    //            Wkid = int.Parse(filter.FeatureSpatialReference.Name.Split(':')[1])
                    //        };
                    //    }
                    //    catch { }
                    //}

                    #endregion

                    #region Limit/Begin/Order

                    filter.Limit = queryRequest.Limit.HasValue && queryRequest.Limit.Value > 0
                        ? Math.Min(queryRequest.Limit.Value, maxRecordCount)
                        : maxRecordCount;

                    filter.BeginRecord = queryRequest.Offset.HasValue
                                ? queryRequest.Offset.Value + 1
                                : 1;  // Start is 1 by IQueryFilter definition

                    filter.OrderBy =  queryRequest.OrderByFields is not null
                                ? String.Join(",", queryRequest.OutFields) // todo: check if fields exists (sql injection?)
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

                                    #endregion

                                    returnGeoJsonFeatures.Add(feature);
                                }
                            }
                        }

                        #endregion
                    }
                }

                if (queryRequest.CountOnly == true)
                {
                    return new GetFeaturesCountOnlyResponse() { Count = featureCount };
                }
                else if (queryRequest.IdsOnly == true)
                {
                    return new GetFeaturesIdsOnlyResponse()
                    {
                        ObjectIdFieldName = objectIdFieldName,
                        ObjectIds = returnGeoJsonFeatures.Select(f => Convert.ToInt32(f[objectIdFieldName]))
                    };
                }
                else if (queryRequest.Distinct == true)
                {
                    var distinctField = queryRequest.OutFields.First();
                    return new GetFeaturesDistinctResponse()
                    {
                        DistinctField = distinctField,
                        DistinctValues = returnGeoJsonFeatures.Select(f => f[distinctField])
                    };
                }

                return Results.Text(GeoJsonHelper.ToGeoJson(returnGeoJsonFeatures), "application/json");
            });

    private const bool _useTOC = true;

    static private List<ITableClass>? FindTableClass(IServiceMap map, string id, out string filterQuery)
    {
        filterQuery = String.Empty;
        if (map == null)
        {
            return null;
        }

        List<ITableClass> classes = new List<ITableClass>();

        foreach (ILayer element in MapServerHelper.FindMapLayers(map, _useTOC, id))
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

#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Interoperability.Extensions.Cql2;
using gView.Interoperability.Extensions.Features;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Interoperability.OGC.Request.WFS;

/// <summary>
/// The result of a GetFeature query
/// </summary>
public sealed class WfsQueryResult
{
    public List<WfsFeatureSet> FeatureSets { get; } = new();

    /// <summary>
    /// The number of features matching the query (all pages), null if not counted
    /// </summary>
    public int? NumberMatched { get; set; }

    public int NumberReturned => FeatureSets.Sum(s => s.Features.Count);

    /// <summary>
    /// There are more features after this page
    /// </summary>
    public bool HasMore { get; set; }
}

/// <summary>
/// Executes the queries of a GetFeature request: filters (FILTER, BBOX, RESOURCEID), sorting,
/// property selection, paging over one or more feature types and counting (numberMatched, hits).
/// </summary>
public class WfsQuery
{
    private readonly IServiceMap? _map;
    private readonly WfsRequestParameters _parameters;
    private readonly WfsVersion _version;

    /// <param name="outCrs">The CRS of the request (SRSNAME or the default CRS), also for filter geometries without srsName</param>
    public WfsQuery(IServiceMap? map, WfsRequestParameters parameters, WfsVersion version, CrsReference? outCrs)
    {
        _map = map;
        _parameters = parameters;
        _version = version;
        OutCrs = outCrs;
        OutSpatialReference = outCrs?.ToSpatialReference();
    }

    public CrsReference? OutCrs { get; }
    public ISpatialReference? OutSpatialReference { get; }

    /// <summary>
    /// The context of the request, set as user data of the filters (data sources use it, e.g. for logging)
    /// </summary>
    public object? Context { get; set; }

    /// <param name="count">The maximum number of features (0: no limit)</param>
    /// <param name="countMatched">Count the features matching the query (numberMatched)</param>
    /// <param name="hitsOnly">resultType=hits: only count</param>
    public async Task<WfsQueryResult> ExecuteAsync(IReadOnlyList<OgcCollectionLayer> collections, int count, bool countMatched, bool hitsOnly)
    {
        var result = new WfsQueryResult();

        // the filters are checked first, so errors don't depend on the data
        foreach (var collection in collections)
        {
            CreateFilter(collection);
        }

        #region Count

        int[]? matched = null;
        if (hitsOnly || countMatched || (_parameters.StartIndex > 0 && collections.Count > 1))
        {
            matched = new int[collections.Count];
            for (int i = 0; i < collections.Count; i++)
            {
                matched[i] = await FeatureReader.CountAsync(collections[i], CreateFilter(collections[i]));
            }
            result.NumberMatched = matched.Sum();
        }

        if (hitsOnly)
        {
            return result;
        }

        #endregion

        #region Read (one more feature than requested to know if there is a next page)

        int offset = _parameters.StartIndex;
        int read = 0;

        for (int i = 0; i < collections.Count; i++)
        {
            var collection = collections[i];

            if (offset > 0 && matched is not null && collections.Count > 1)
            {
                if (offset >= matched[i])
                {
                    offset -= matched[i];
                    continue;
                }
            }

            int take = count > 0 ? count + 1 - read : 0;
            var (properties, includeGeometry) = SelectedProperties(collection);

            var features = await FeatureReader.ReadListAsync(
                _map, collection, CreateFilter(collection), OutSpatialReference,
                offset, take, properties, includeGeometry);

            offset = 0;
            read += features.Count;

            if (count > 0 && read > count)
            {
                features.RemoveAt(features.Count - 1);
                result.HasMore = true;
            }
            if (features.Count > 0)
            {
                result.FeatureSets.Add(new WfsFeatureSet(collection, features));
            }
            if (count > 0 && read >= count)
            {
                break;
            }
        }

        if (!result.HasMore && result.NumberMatched.HasValue)
        {
            result.HasMore = _parameters.StartIndex + result.NumberReturned < result.NumberMatched.Value;
        }

        #endregion

        return result;
    }

    #region Filter

    /// <summary>
    /// The query filter of a feature type: FILTER, BBOX or RESOURCEID, the layer's filter query and SORTBY
    /// </summary>
    public IQueryFilter CreateFilter(OgcCollectionLayer collection)
    {
        var featureClass = collection.FeatureClass
            ?? throw WfsException.InvalidParameterValue("typeNames", $"{collection.Id} has no features");

        WfsFilter wfsFilter;
        if (_parameters.BBox is not null)
        {
            wfsFilter = new WfsFilter(null, BBoxCondition(_parameters.BBox, _version, OutCrs), null);
        }
        else if (_parameters.ResourceIds.Count > 0)
        {
            wfsFilter = new WfsFilter(null, null, _parameters.ResourceIds);
        }
        else
        {
            wfsFilter = new FesFilterParser(collection, _version, OutCrs).Parse(_parameters.Filter);
        }

        IQueryFilter filter = wfsFilter.Spatial is { } spatial
            ? new SpatialFilter()
            {
                Geometry = spatial.Geometry,
                SpatialRelation = spatial.Relation,
                FilterSpatialReference = spatial.SpatialReference ?? OutSpatialReference ?? collection.SpatialReference
            }
            : new QueryFilter();

        string? where = null;
        if (wfsFilter.Predicate is not null)
        {
            try
            {
                where = new CqlSqlTranslator(collection.FindQueryable, collection.SqlDialect).Translate(wfsFilter.Predicate);
            }
            catch (CqlException ex)
            {
                throw WfsException.InvalidParameterValue("filter", ex.Message.Replace("CQL2 filter", "filter"));
            }
        }
        else if (wfsFilter.ResourceIds is not null)
        {
            where = FesFilterParser.ResourceIdWhereClause(collection, wfsFilter.ResourceIds);
        }

        // The where clause is written by the CQL2 translator (checked field names, escaped literals)
        filter.WhereClause = FeatureReader.CombineWhereClauses(collection.FilterWhereClause, where);
        filter.OrderBy = OrderBy(collection);

        if (Context is not null)
        {
            filter.SetUserData("IServiceRequestContext", Context);
        }

        return filter;
    }

    /// <summary>
    /// BBOX=minx,miny,maxx,maxy[,crs] in the axis order of the crs (default: the CRS of the request)
    /// </summary>
    internal static WfsSpatialCondition BBoxCondition(string bbox, WfsVersion version, CrsReference? defaultCrs)
    {
        var parts = bbox.Split(',');
        if (parts.Length < 4)
        {
            throw WfsException.InvalidParameterValue("bbox", $"Invalid bbox: {bbox}");
        }

        var values = new double[4];
        for (int i = 0; i < 4; i++)
        {
            if (!Double.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]))
            {
                throw WfsException.InvalidParameterValue("bbox", $"Invalid bbox: {bbox}");
            }
        }

        var crs = parts.Length > 4
            ? WFSRequest.ParseCrs(String.Join(",", parts.Skip(4)).Trim(), "bbox")
            : defaultCrs;
        var sref = crs?.ToSpatialReference();

        var envelope = version.SwapAxes(crs, sref)
            ? new Envelope(values[1], values[0], values[3], values[2])
            : new Envelope(values[0], values[1], values[2], values[3]);

        return new WfsSpatialCondition(spatialRelation.SpatialRelationIntersects, envelope, sref);
    }

    #endregion

    #region Sort, properties

    /// <summary>
    /// SORTBY as ORDER BY of property fields (or the id), with the id as last key for stable pages
    /// </summary>
    private string OrderBy(OgcCollectionLayer collection)
    {
        if (_parameters.SortBy.Count == 0)
        {
            return "";
        }

        var featureClass = collection.FeatureClass!;
        var keys = new List<string>();

        foreach (var (property, descending) in _parameters.SortBy)
        {
            string name = FesFilterParser.LocalPropertyName(property);
            string fieldName = FindPropertyField(collection, name)?.name
                ?? (name.Equals(featureClass.IDFieldName, StringComparison.OrdinalIgnoreCase) ? featureClass.IDFieldName : null)
                ?? throw WfsException.InvalidParameterValue("sortBy", $"Unknown property {property}");

            keys.Add(descending ? $"{fieldName} DESC" : fieldName);
        }

        if (!String.IsNullOrEmpty(featureClass.IDFieldName) && !keys.Any(k => k.Split(' ')[0] == featureClass.IDFieldName))
        {
            keys.Add(featureClass.IDFieldName);
        }

        return String.Join(",", keys);
    }

    /// <summary>
    /// PROPERTYNAME: the selected property fields and whether the geometry is selected (all if empty)
    /// </summary>
    internal (IReadOnlyCollection<IField>? properties, bool includeGeometry) SelectedProperties(OgcCollectionLayer collection)
    {
        if (_parameters.PropertyNames.Count == 0)
        {
            return (null, true);
        }

        var properties = new List<IField>();
        bool includeGeometry = false;
        string geometryName = WfsFeatureTypes.GeometryElementName(collection);

        foreach (var propertyName in _parameters.PropertyNames)
        {
            string name = FesFilterParser.LocalPropertyName(propertyName);
            if (name.Equals(geometryName, StringComparison.OrdinalIgnoreCase))
            {
                includeGeometry = true;
            }
            else if (FindPropertyField(collection, name) is { } field)
            {
                if (!properties.Contains(field))
                {
                    properties.Add(field);
                }
            }
            else
            {
                throw WfsException.InvalidParameterValue("propertyName", $"Unknown property {propertyName}");
            }
        }

        return (properties, includeGeometry);
    }

    private static IField? FindPropertyField(OgcCollectionLayer collection, string name)
    {
        var fields = collection.PropertyFields.ToList();
        return fields.FirstOrDefault(f => f.name == name || WfsFeatureTypes.ElementName(f.name) == name)
            ?? fields.FirstOrDefault(f => f.name.Equals(name, StringComparison.OrdinalIgnoreCase)
                                       || WfsFeatureTypes.ElementName(f.name).Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    #endregion
}

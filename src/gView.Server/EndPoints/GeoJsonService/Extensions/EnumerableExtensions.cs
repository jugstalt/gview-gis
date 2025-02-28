#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.UI;
using gView.Framework.GeoJsonService.Extensions;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class EnumerableExtensions
{
    static private string[] IdStrings = Enumerable.Range(0, 1000).Select(i => i.ToString()).ToArray();
    static private string[] ExcludeIdStrings = Enumerable.Range(0, 1000).Select(i => $"-{i}").ToArray();
    static private string[] IncludeIdStrings = Enumerable.Range(0, 1000).Select(i => $"+{i}").ToArray();

    public static MapLayerVisibility LayerOrParentIsInArray(this string[] layerIds, ITocElement tocElement)
    {
        while (tocElement != null)
        {
            foreach (var layer in tocElement.Layers ?? [])
            {
                var visibility = layerIds.ContainsId(layer.ID);
                if (visibility != MapLayerVisibility.Invisible)
                {
                    return visibility;
                }
            }

            tocElement = tocElement.ParentGroup;
        }

        return MapLayerVisibility.Invisible;
    }

    public static MapLayerVisibility ContainsId(this string[] layerIds, int layerId)
    {
        var id = layerId >= 0 && layerId < IdStrings.Length
                    ? IdStrings[layerId]
                    : layerId.ToString();

        if (layerIds.Contains(id))
        {
            return MapLayerVisibility.Visible;
        }

        id = layerId >= 0 && layerId < IdStrings.Length
                    ? ExcludeIdStrings[layerId]
                    : $"-{layerId}";

        if (layerIds.Contains(id))
        {
            return MapLayerVisibility.Exclude;
        }

        id = layerId >= 0 && layerId < IdStrings.Length
                    ? IncludeIdStrings[layerId]
                    : $"+{layerId}";

        if (layerIds.Contains(id))
        {
            return MapLayerVisibility.Include;
        }

        return MapLayerVisibility.Invisible;
    }

    public static MapLayerVisibilityPattern GetVisibilityPattern(this string[] layerIds)
    {
        if (layerIds is null || layerIds.Length == 0)
        {
            return MapLayerVisibilityPattern.Defaults;
        }

        var countIncludeExcludes = layerIds.Count(l => l.StartsWith("-") || l.StartsWith("+"));

        if (countIncludeExcludes == 0) // no include/exclude layers
        {
            return MapLayerVisibilityPattern.Normal;
        }

        if (countIncludeExcludes == layerIds.Length)  // all are include or exclude
        {
            return MapLayerVisibilityPattern.IncludeExclude;
        }

        throw new MapServerException($"Invalid layer ids: dont mix include/exclude ids with normal ids");
    }

    public static IEnumerable<string> ProjectNamesAndCheckAllowedFunctions(
                this IEnumerable<string> fieldNames,
                ITableClass tableClass,
                bool isFeatureServer)
    {
        if (isFeatureServer && fieldNames.Count() == 1 && fieldNames.First() == "*")
        {
            fieldNames = tableClass.Fields.ToEnumerable()
                    .Select(f => f.name)
                    .Where(n => !n.IsFieldFunction())  // FeatureServer Query do not return FieldFunctions link STArea, STLength, ...
                    .ToArray();
        }

        var tableFields = tableClass.Fields.ToEnumerable();

        foreach (var fieldName in fieldNames)
        {
            if (fieldName == "*")
            {
                yield return fieldName;
            }
            else
            {
                var field = tableFields.FirstOrDefault(f => f.name == fieldName)
                            ?? tableFields.FirstOrDefault(f => f.name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

                yield return field switch
                {
                    null when fieldName.IsFieldFunction()
                         => throw new MapServerException($"Forbidden field function detected: {fieldName}"),
                    null => throw new MapServerException($"Field '{fieldName}' not contained in table"),
                    _ => field.name
                };
            }
        }
    }

    static private bool IsFieldFunction(this string fieldName)
        => !String.IsNullOrEmpty(fieldName)
        && (
                fieldName.Contains("(")
                || fieldName.Contains(")")
                || fieldName.Contains(" ")
                || fieldName.Contains(".")
        );

    static public IEnumerable<string> ProjectNamesAndCheckIfFieldsExists(this IEnumerable<string> fieldNames, ITableClass tableClass)
    {
        var tableFields = tableClass.Fields.ToEnumerable();

        foreach (var fieldName in fieldNames)
        {
            if (fieldName == "*")
            {
                yield return fieldName;
            }

            var field = tableFields.FirstOrDefault(f => f.name == fieldName)
                        ?? tableFields.FirstOrDefault(f => f.name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

            if (field is null)
            {
                throw new MapServerException($"Field '{fieldName}' not contained in table");
            }

            yield return field.name;
        }
    }

    #region Features

    static public IEnumerable<IFeature> GetFeatrues(
                this IEnumerable<gView.GeoJsonService.DTOs.Feature> geoJsonFeatures,
                IFeatureClass featureClass,
                ISpatialReference? geoJsonFeaturesSRef = null)
    {
        int? fcSrs = featureClass.SpatialReference?.EpsgCode;

        foreach (var geoJsonFeature in geoJsonFeatures)
        {
            var feature = geoJsonFeature.ToFeature(featureClass);

            if (feature.Shape is not null
                && geoJsonFeaturesSRef is not null
                && fcSrs.HasValue
                && geoJsonFeaturesSRef.EpsgCode != fcSrs.Value)
            {
                using (var transformer = GeometricTransformerFactory.Create())
                {
                    var fromSrs = SpatialReference.FromID(geoJsonFeaturesSRef.Name);
                    var toSrs = SpatialReference.FromID($"epsg:{fcSrs}");

                    transformer.SetSpatialReferences(fromSrs, toSrs);

                    var transformedShape = transformer.Transform2D(feature.Shape) as IGeometry;
                    if(transformedShape is null)
                    {
                        throw new MapServerException("Error on transform shape");
                    }
                    transformedShape.Srs = fcSrs.Value;

                    feature.Shape = transformedShape;
                }
            }

            yield return feature;
        }
    }

    private static Feature ToFeature(this gView.GeoJsonService.DTOs.Feature geoJsonFeature, IFeatureClass fc)
    {
        var feature = new Feature();

        feature.OID = geoJsonFeature.Oid != null
                        ? Convert.ToInt32(feature.OID) 
                        : 0;
        feature.Shape = geoJsonFeature.Geometry?.ToGeometry();

        if (geoJsonFeature.Properties == null)
        {
            throw new ArgumentException("No features properties!");
        }

        for (int f = 0, fieldCount = fc.Fields.Count; f < fieldCount; f++)
        {
            var field = fc.Fields[f];

            if (geoJsonFeature.Properties.ContainsKey(field.name))
            {
                switch (field.type)
                {
                    case FieldType.ID:
                        throw new MapServerException($"Parse property {field.name}: Object Id as property is not allowed. Set ObjectID using feature.Oid.");
                        //feature.Fields.Add(new FieldValue(field.name, geoJsonFeature.Properties[field.name]));
                        //feature.OID = Convert.ToInt32(geoJsonFeature.Properties[field.name]);
                        //break;
                    case FieldType.Date:
                        object val = geoJsonFeature.Properties[field.name]!;
                        if (val is not null && val is string)
                        {
                            if (val.ToString()?.Contains(" ") == true)
                            {
                                val = DateTime.ParseExact(val.ToString()!,
                                    new string[]{
                                        "dd.MM.yyyy HH:mm:ss",
                                        "dd.MM.yyyy HH:mm",
                                        "yyyy.MM.dd HH:mm:ss",
                                        "yyyy.MM.dd HH:mm",
                                        "yyyy-MM-dd HH:mm:ss",
                                        "yyyy-MM-dd HH:mm"
                                    },
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.None);
                            }
                            else
                            {
                                val = DateTime.ParseExact(val.ToString()!,
                                    new string[]{
                                        "dd.MM.yyyy",
                                        "yyyy.MM.dd",
                                        "yyyy-MM-dd"
                                        },
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.None);
                            }
                        }
                        else if (val is long || val is int)
                        {
                            long esriDate = Convert.ToInt64(val);
                            val = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(esriDate);
                        }
                        feature.Fields.Add(new FieldValue(field.name, val));
                        break;
                    default:
                        feature.Fields.Add(new FieldValue(field.name, geoJsonFeature.Properties[field.name]));
                        break;
                }
            }
        }

        return feature;
    }

    static public void GeometryMakeValid(this IEnumerable<IFeature> features, IServiceMap map, IFeatureClass featureClass)
    {
        if (features == null || features.Count() == 0)
        {
            return;
        }

        if (map.GetGeometryType(featureClass) == GeometryType.Polygon)
        {
            foreach (var feature in features)
            {
                var polygon = feature.Shape as IPolygon;

                if (polygon == null)
                {
                    continue;
                }

                var sRef = polygon.Srs.HasValue && !polygon.Srs.Equals(featureClass.SpatialReference?.EpsgCode) ?
                    SpatialReference.FromID($"epsg:{polygon.Srs.Value}") :
                    featureClass.SpatialReference ?? map.LayerDefaultSpatialReference;

                if (sRef != null)
                {
                    feature.Shape.Clean(CleanGemetryMethods.IdentNeighbors | CleanGemetryMethods.ZeroParts);
                }
            }
        }
    }

    static public GeometryType GetGeometryType(this IServiceMap map, IFeatureClass featureClass)
    {
        if (featureClass.GeometryType != GeometryType.Unknown)
        {
            return featureClass.GeometryType;
        }

        var layer = map.MapElements.Where(e => e.Class == featureClass).FirstOrDefault() as IFeatureLayer;

        if (layer != null)
        {
            return layer.LayerGeometryType;
        }

        return GeometryType.Unknown;
    }

    #endregion
}

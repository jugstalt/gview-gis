using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Editor.Core;
using gView.Interoperability.Extensions.Editing;
using gView.Interoperability.OgcApi.Extensions;
using gView.Interoperability.OgcApi.Features;
using gView.Interoperability.OgcApi.GeoJson;
using System.Globalization;
using System.Text.Json;

namespace gView.Interoperability.OgcApi;

/// <summary>
/// OGC API - Features Part 4 (Create, Replace, Update, Delete). The edit rules (allowed statements,
/// validation, projection, geometry cleaning, edit sessions) are the same as for GeoServices REST.
/// </summary>
public sealed partial class OgcApiFeatureService
{
    /// <summary>
    /// The edit statements of a collection: INSERT => POST, UPDATE => PUT/PATCH, DELETE => DELETE
    /// </summary>
    public EditStatements EditStatements(string collectionId)
    {
        try
        {
            var collection = GetFeatureCollectionLayer(collectionId);
            var editService = FeatureEditService.Create(Map, collection.LayerId);

            return editService.IsEditable ? editService.AllowedStatements : Framework.Editor.Core.EditStatements.NONE;
        }
        catch (MapServerException)
        {
            return Framework.Editor.Core.EditStatements.NONE;
        }
    }

    /// <returns>the id of the new feature (null, if the database can't report it)</returns>
    public async Task<int?> CreateItemAsync(string collectionId, string body, string? contentCrs)
    {
        var (collection, editService) = EditContext(collectionId, Framework.Editor.Core.EditStatements.INSERT);
        var geoJsonFeature = ReadFeature(collection, body, contentCrs, out int srs);

        var feature = new Feature();
        feature.Shape = CheckGeometry(editService, geoJsonFeature.Geometry, srs, required: true);
        SetProperties(collection, feature, geoJsonFeature.Properties, setMissingToNull: false);

        var features = new List<IFeature> { feature };
        editService.PrepareInserts(features);

        await ExecuteAsync(editService, session => editService.Insert(features, session));

        return feature.OID > 0 ? feature.OID : null;
    }

    /// <summary>
    /// PUT: replaces the feature. Properties missing in the body are set to null.
    /// </summary>
    public async Task ReplaceItemAsync(string collectionId, string featureId, string body, string? contentCrs)
    {
        var (collection, editService) = EditContext(collectionId, Framework.Editor.Core.EditStatements.UPDATE);
        int oid = await ExistingObjectId(collection, featureId);
        var geoJsonFeature = ReadFeature(collection, body, contentCrs, out int srs);

        var feature = new Feature() { OID = oid };
        feature.Shape = CheckGeometry(editService, geoJsonFeature.Geometry, srs, required: true);
        SetProperties(collection, feature, geoJsonFeature.Properties, setMissingToNull: true);

        var features = new List<IFeature> { feature };
        editService.PrepareUpdates(features);

        await ExecuteAsync(editService, session => editService.Update(features, session));
    }

    /// <summary>
    /// PATCH (JSON merge patch): only the properties and the geometry in the body are changed
    /// </summary>
    public async Task UpdateItemAsync(string collectionId, string featureId, string body, string? contentCrs)
    {
        var (collection, editService) = EditContext(collectionId, Framework.Editor.Core.EditStatements.UPDATE);
        int oid = await ExistingObjectId(collection, featureId);
        var geoJsonFeature = ReadFeature(collection, body, contentCrs, out int srs);

        var feature = new Feature() { OID = oid };
        if (geoJsonFeature.HasGeometry)
        {
            feature.Shape = CheckGeometry(editService, geoJsonFeature.Geometry, srs, required: true);
        }
        SetProperties(collection, feature, geoJsonFeature.Properties, setMissingToNull: false);

        if (feature.Shape is null && feature.Fields.Count == 0)
        {
            return;  // nothing to change
        }

        var features = new List<IFeature> { feature };
        editService.PrepareUpdates(features);

        await ExecuteAsync(editService, session => editService.Update(features, session));
    }

    public async Task DeleteItemAsync(string collectionId, string featureId)
    {
        var (collection, editService) = EditContext(collectionId, Framework.Editor.Core.EditStatements.DELETE);
        int oid = await ExistingObjectId(collection, featureId);

        await ExecuteAsync(editService, session => editService.Delete(oid, session));
    }

    #region Helper

    private (OgcCollectionLayer collection, FeatureEditService editService) EditContext(string collectionId, EditStatements statement)
    {
        var collection = GetFeatureCollectionLayer(collectionId);

        FeatureEditService editService;
        try
        {
            editService = FeatureEditService.Create(Map, collection.LayerId);
        }
        catch (MapServerException ex)
        {
            throw new OgcApiException(405, "MethodNotAllowed", ex.Message);
        }

        if (!editService.IsEditable || !editService.AllowedStatements.HasFlag(statement))
        {
            throw new OgcApiException(405, "MethodNotAllowed", $"{statement} is not allowed for collection '{collectionId}'");
        }

        return (collection, editService);
    }

    private async Task<int> ExistingObjectId(OgcCollectionLayer collection, string featureId)
    {
        // throws 404, if the feature doesn't exist (or is outside the layer filter)
        var existing = await FeatureQuery.QueryItemAsync(Map, collection, featureId, CrsReference.Crs84);

        return Convert.ToInt32(existing.Id, CultureInfo.InvariantCulture);
    }

    private GeoJsonFeature ReadFeature(OgcCollectionLayer collection, string body, string? contentCrs, out int srs)
    {
        var crs = CrsReference.Crs84;
        if (!String.IsNullOrWhiteSpace(contentCrs))
        {
            string value = contentCrs.Trim().TrimStart('<').TrimEnd('>');
            if (!CrsReference.TryParse(value, out crs) || !Map.SupportedCrs(collection).Contains(crs))
            {
                throw OgcApiException.BadRequest($"Unsupported Content-Crs: {contentCrs}");
            }
        }

        srs = crs.EpsgCode;
        return GeoJsonReader.ReadFeature(body, crs.HasNorthingFirst(crs.ToSpatialReference()));
    }

    /// <summary>
    /// The geometry type has to fit the feature class. A point becomes a multipoint for multipoint classes.
    /// </summary>
    private static IGeometry? CheckGeometry(FeatureEditService editService, IGeometry? geometry, int srs, bool required)
    {
        if (geometry is null)
        {
            return required
                ? throw OgcApiException.BadRequest("The feature needs a geometry")
                : null;
        }

        var geometryType = editService.FeatureClass.GeometryType;

        if (geometryType == GeometryType.Multipoint && geometry is IPoint point)
        {
            var multiPoint = new Framework.Geometry.MultiPoint();
            multiPoint.AddPoint(point);
            geometry = multiPoint;
        }

        bool valid = geometryType switch
        {
            GeometryType.Point => geometry is IPoint,
            GeometryType.Multipoint => geometry is IMultiPoint,
            GeometryType.Polyline => geometry is IPolyline,
            GeometryType.Polygon => geometry is IPolygon,
            _ => true
        };

        if (!valid)
        {
            throw OgcApiException.BadRequest($"Invalid geometry type for a {geometryType} collection");
        }

        geometry.Srs = srs;
        return geometry;
    }

    /// <summary>
    /// Only the editable, visible attribute fields of the layer are written. Unknown properties are ignored.
    /// </summary>
    private static void SetProperties(OgcCollectionLayer collection, IFeature feature, IReadOnlyDictionary<string, JsonElement>? properties, bool setMissingToNull)
    {
        foreach (var field in collection.PropertyFields.Where(f => f.IsEditable))
        {
            if (properties is not null && properties.TryGetValue(field.name, out var value))
            {
                feature.Fields.Add(new FieldValue(field.name, ToFieldValue(field, value)));
            }
            else if (setMissingToNull)
            {
                feature.Fields.Add(new FieldValue(field.name, null));
            }
        }
    }

    internal static object? ToFieldValue(IField field, JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        try
        {
            if (field.IsIntegerType())
            {
                long longValue = value.ValueKind switch
                {
                    JsonValueKind.String => long.Parse(value.GetString()!, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture),
                    JsonValueKind.True => 1,   // booleans stored as integer (e.g. GeoPackage, SQLite)
                    JsonValueKind.False => 0,
                    _ => value.GetInt64()
                };

                return field.type switch
                {
                    FieldType.smallinteger => (object)Convert.ToInt16(longValue),
                    FieldType.integer => Convert.ToInt32(longValue),
                    _ => longValue
                };
            }

            if (field.IsFloatingPointType())
            {
                double doubleValue = value.ValueKind == JsonValueKind.String
                    ? double.Parse(value.GetString()!, NumberStyles.Float, CultureInfo.InvariantCulture)
                    : value.GetDouble();

                return field.type == FieldType.Float ? (object)(float)doubleValue : doubleValue;
            }

            switch (field.type)
            {
                case FieldType.boolean:
                    return value.ValueKind switch
                    {
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Number => value.GetInt32() != 0,
                        _ => bool.Parse(value.GetString()!)
                    };

                case FieldType.Date:
                    string date = value.GetString()!;
                    if (DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
                    {
                        return dateOnly.ToDateTime(TimeOnly.MinValue);
                    }
                    var timestamp = DateTimeOffset.Parse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                    return timestamp.UtcDateTime;

                case FieldType.guid:
                    return Guid.Parse(value.GetString()!);
            }

            return value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : value.GetRawText();
        }
        catch (Exception ex) when (ex is FormatException or InvalidOperationException or OverflowException)
        {
            throw OgcApiException.BadRequest($"Invalid value for property '{field.name}': {value.GetRawText()}");
        }
    }

    /// <summary>
    /// Runs the edit in an edit session (one transaction), if the database supports it
    /// </summary>
    private static async Task ExecuteAsync(FeatureEditService editService, Func<Framework.Core.FDB.IFeatureEditSession?, Task<bool>> edit)
    {
        var session = await editService.BeginEditSession();

        if (session is null)
        {
            if (!await edit(null))
            {
                throw new OgcApiException(500, "ServerError", editService.LastErrorMessage() ?? "edit failed");
            }
            return;
        }

        await using (session)
        {
            if (!await edit(session) || !await session.Commit())
            {
                throw new OgcApiException(500, "ServerError", editService.LastErrorMessage(session) ?? "edit failed");
            }
        }
    }

    #endregion
}

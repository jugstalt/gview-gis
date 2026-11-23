using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.FDB;
using gView.Framework.Data.Filters;
using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Data;
using gView.Pipe.Abstraction.Nodes;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis;

public enum GisWriteMode
{
    [PipeEnumValue("Adds every item as a new feature")]
    Insert,

    [PipeEnumValue("Changes the features with the item's key; only the properties the item has, the geometry only when it has one")]
    Update,

    [PipeEnumValue("Changes the features with the item's key, and adds items whose key isn't there yet")]
    Upsert,

    [PipeEnumValue("Removes the features with the item's key")]
    Delete
}

public sealed class WriteFeaturesParameters
{
    [Required, PipeCredential(GisDatasourceCredential.TypeId)]
    [PipeParameter(Label = "Data source", Order = 1, Expressions = false, Description = "gView Datasource credential, e.g. the PostGIS database of the grid")]
    public string? CredentialId { get; set; }

    [Required, PipeParameter(Label = "Feature class", Order = 2, Expressions = false, Placeholder = "lv_lines",
        Description = "Name of the feature class (table) to write, e.g. public.lv_lines")]
    public string FeatureClass { get; set; } = "";

    [PipeParameter(Label = "Mode", Order = 3, Expressions = false, Description = "Add, change or remove features")]
    public GisWriteMode Mode { get; set; } = GisWriteMode.Insert;

    [PipeParameter(Label = "Key field", Order = 4, Expressions = false, ShowWhen = nameof(Mode), ShowWhenValues = ["update", "upsert", "delete"],
        Description = "Field that identifies a feature, e.g. line_id, with its value in the item's properties; empty uses the object id in the item's id")]
    public string KeyField { get; set; } = "";

    [PipeParameter(Label = "Item SRS", Order = 5, Expressions = false, Advanced = true,
        Description = "EPSG code of the items' geometries when they have no srs, e.g. 4326; 0 means that of the feature class")]
    public int ItemSrs { get; set; }

    [PipeParameter(Label = "Batch size", Order = 6, Expressions = false, Advanced = true,
        Description = "Features per write call, each in its own transaction; 0 uses the suggestion of the data source")]
    [Range(0, 100000)]
    public int BatchSize { get; set; }
}

[PipeNode("8d2f4b6a-1c3e-4f5a-9b7d-0e2c4a6b8d13", "Write Features",
    Category = "GIS",
    Description = "Writes GeoJSON items as features into a gView data source (PostGIS, SDE, shape files, …): "
                  + "insert, update, upsert or delete by a key field. Geometries are projected into the coordinate system of the feature class.")]
[PipeNodeExamples]
public sealed class WriteFeaturesNode : IPipeNode<WriteFeaturesParameters>
{
    public async Task<PipeNodeResult> ExecuteAsync(IPipeNodeContext context, CancellationToken cancellationToken)
    {
        var items = context.GetInput();
        if (items.Count == 0)
        {
            return PipeNodeResult.FromMain([]);
        }

        // one feature class for all items: these parameters take no expressions
        var parameters = context.GetParameters<WriteFeaturesParameters>(items[0]);
        if (string.IsNullOrEmpty(parameters.CredentialId))
        {
            throw new PipeException("Select a data source");
        }
        var credential = await context.GetCredentialAsync<GisDatasourceCredential>(parameters.CredentialId, cancellationToken);

        using var dataset = await GisDatasets.OpenAsync(credential);
        var featureClass = await GisDatasets.FeatureClassAsync(dataset, parameters.FeatureClass);
        var updater = GisDatasets.Updater(featureClass);
        var batchSize = parameters.BatchSize > 0
            ? parameters.BatchSize
            : Math.Max(updater.SuggestedInsertFeatureCountPerTransaction, 100);
        var writer = new Writer(featureClass, updater, parameters, batchSize);

        var output = parameters.Mode switch
        {
            GisWriteMode.Insert => await writer.InsertAsync(items, cancellationToken),
            GisWriteMode.Delete => await writer.DeleteAsync(items, cancellationToken),
            _ => await writer.UpdateAsync(items, upsert: parameters.Mode == GisWriteMode.Upsert, cancellationToken)
        };

        context.Logger.LogInformation("{Mode}: {Count} feature(s) of {Items} item(s) written to {FeatureClass}",
            parameters.Mode, output.Count, items.Count, featureClass.Name);
        return PipeNodeResult.FromMain(output);
    }

    private sealed class Writer(IFeatureClass featureClass, IFeatureUpdater updater, WriteFeaturesParameters parameters, int batchSize)
    {
        private readonly int? _classSrs = GisSpatialReferences.Epsg(featureClass);

        public async Task<List<PipeItem>> InsertAsync(IReadOnlyList<PipeItem> items, CancellationToken cancellationToken)
        {
            var features = items.Select(item => ToFeature(item, oid: 0)).ToList();
            foreach (var batch in features.Chunk(batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();
                Check(await updater.Insert(featureClass, batch.ToList(), returnIds: true), "insert");
            }

            return items.Select((item, i) => Written(item, features[i].OID)).ToList();
        }

        public async Task<List<PipeItem>> UpdateAsync(IReadOnlyList<PipeItem> items, bool upsert, CancellationToken cancellationToken)
        {
            var updates = new List<(PipeItem Item, IFeature Feature)>();
            var inserts = new List<(PipeItem Item, IFeature Feature)>();
            foreach (var item in items)
            {
                var oids = await FindAsync(item);
                if (oids.Count == 0)
                {
                    if (!upsert)
                    {
                        throw new PipeException($"No feature with the key {KeyText(item)} in '{featureClass.Name}'");
                    }
                    inserts.Add((item, ToFeature(item, oid: 0)));
                }
                updates.AddRange(oids.Select(oid => (item, ToFeature(item, oid))));
            }

            foreach (var batch in updates.Chunk(batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();
                Check(await updater.Update(featureClass, batch.Select(u => u.Feature).ToList()), "update");
            }
            foreach (var batch in inserts.Chunk(batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();
                Check(await updater.Insert(featureClass, batch.Select(u => u.Feature).ToList(), returnIds: true), "insert");
            }

            return updates.Concat(inserts).Select(u => Written(u.Item, u.Feature.OID)).ToList();
        }

        public async Task<List<PipeItem>> DeleteAsync(IReadOnlyList<PipeItem> items, CancellationToken cancellationToken)
        {
            var output = new List<PipeItem>();
            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var oid in await FindAsync(item))
                {
                    Check(await updater.Delete(featureClass, oid), "delete");
                    output.Add(Written(item, oid));
                }
            }
            return output;
        }

        // object ids of the features with the item's key
        private async Task<List<int>> FindAsync(PipeItem item)
        {
            if (string.IsNullOrWhiteSpace(parameters.KeyField))
            {
                return item.Json["id"] is JsonValue id && id.TryGetValue<int>(out var oid) && oid > 0
                    ? [oid]
                    : throw new PipeException("The item has no object id in 'id'; set a key field or read the features first");
            }

            var field = GisGeoJson.ToSqlField(featureClass, parameters.KeyField);
            var value = (item.Json["properties"] as JsonObject)?[field] ?? item.Json[field];
            var filter = new QueryFilter
            {
                SubFields = featureClass.IDFieldName,
                WhereClause = $"{field} = {GisGeoJson.ToSqlLiteral(value)}"
            };

            var oids = new List<int>();
            using var cursor = await featureClass.GetFeatures(filter);
            IFeature? feature;
            while (cursor is not null && (feature = await cursor.NextFeature()) is not null)
            {
                oids.Add(feature.OID);
            }
            return oids;
        }

        private string KeyText(PipeItem item)
            => string.IsNullOrWhiteSpace(parameters.KeyField)
                ? $"id = {item.Json["id"]?.ToJsonString()}"
                : $"{parameters.KeyField} = {((item.Json["properties"] as JsonObject)?[parameters.KeyField] ?? item.Json[parameters.KeyField])?.ToJsonString()}";

        private IFeature ToFeature(PipeItem item, int oid)
        {
            var feature = new Feature { OID = oid };
            if (item.Json["properties"] is JsonObject properties)
            {
                foreach (var (name, value) in properties)
                {
                    var field = featureClass.FindField(name);
                    if (field is null
                        || field.name.Equals(featureClass.IDFieldName, StringComparison.OrdinalIgnoreCase)
                        || field.name.Equals(featureClass.ShapeFieldName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;   // properties the feature class doesn't have are left out
                    }
                    feature.Fields.Add(new FieldValue(field.name, GisGeoJson.ToFieldValue(value)));
                }
            }

            if (GisGeoJson.ToGeometry(item.Json["geometry"]) is { } geometry)
            {
                var itemSrs = GisSpatialReferences.ToEpsg(item.Json["srs"])
                    ?? (parameters.ItemSrs > 0 ? parameters.ItemSrs : _classSrs);
                feature.Shape = GisSpatialReferences.Project(geometry, itemSrs, _classSrs);
            }

            return feature;
        }

        private PipeItem Written(PipeItem item, int oid)
        {
            var written = item.Clone();
            written.Json["id"] = oid;
            return written;
        }

        private void Check(bool succeeded, string operation)
        {
            if (!succeeded)
            {
                throw new PipeException($"Can't {operation} features of '{featureClass.Name}': {updater.LastErrorMessage}");
            }
        }
    }
}

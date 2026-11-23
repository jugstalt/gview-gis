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

public enum GisWriteOnError
{
    [PipeEnumValue("The first item that can't be written stops the node; batches written before stay written")]
    Stop,

    [PipeEnumValue("Items that can't be written go to the Error output with the reason; the others are written", Label = "Skip item")]
    SkipItem
}

public enum GisWriteGeometry
{
    [PipeEnumValue("Writes the geometry of items that have one; an update leaves the geometry of the others", Label = "If present")]
    IfPresent,

    [PipeEnumValue("Never writes the geometry, e.g. to change only attributes")]
    Never,

    [PipeEnumValue("Every item needs a geometry; an item without one can't be written")]
    Always
}

public enum GisUnknownProperties
{
    [PipeEnumValue("Leaves out properties the feature class has no field for")]
    Ignore,

    [PipeEnumValue("An item with a property the feature class has no field for can't be written")]
    Fail
}

public sealed class WriteFeaturesParameters
{
    [Required, PipeCredential(GisDatasourceCredential.TypeId)]
    [PipeParameter(Label = "Data source", Order = 1, Expressions = false, Description = "gView Datasource credential, e.g. the PostGIS database of the grid")]
    public string? CredentialId { get; set; }

    [Required, PipeOptions(Refreshable = true), PipeParameter(Label = "Feature class", Order = 2, Expressions = false,
        Description = "Feature class (table) to write, one of the data source")]
    public string FeatureClass { get; set; } = "";

    [PipeParameter(Label = "Mode", Order = 3, Expressions = false, Description = "Add, change or remove features")]
    public GisWriteMode Mode { get; set; } = GisWriteMode.Insert;

    [PipeParameter(Label = "Key field", Order = 4, Expressions = false, ShowWhen = nameof(Mode), ShowWhenValues = ["update", "upsert", "delete"],
        Description = "Field that identifies a feature, e.g. line_id, with its value in the item's properties; empty, id or the id field of the feature class (e.g. FDB_OID) uses the object id in the item's id")]
    public string KeyField { get; set; } = "";

    [PipeParameter(Label = "Write fields", Order = 5, Expressions = false, ShowWhen = nameof(Mode), ShowWhenValues = ["insert", "update", "upsert"],
        Description = "Comma-separated fields to write, e.g. status, checked_at; empty writes all properties. Other properties of the item stay as they are")]
    public string WriteFields { get; set; } = "";

    [PipeParameter(Label = "Write geometry", Order = 6, Expressions = false, ShowWhen = nameof(Mode), ShowWhenValues = ["insert", "update", "upsert"],
        Description = "Whether the geometry is written; Never changes only attributes")]
    public GisWriteGeometry WriteGeometry { get; set; } = GisWriteGeometry.IfPresent;

    [PipeParameter(Label = "On error", Order = 7, Expressions = false, Description = "What an item that can't be written does")]
    public GisWriteOnError OnError { get; set; } = GisWriteOnError.Stop;

    [PipeParameter(Label = "Field mapping", Order = 8, Expressions = false, Advanced = true, ShowWhen = nameof(Mode), ShowWhenValues = ["insert", "update", "upsert", "delete"],
        Description = "Renames properties on the way: field of the feature class → property of the item, e.g. line_id → name. Other properties keep their names")]
    public Dictionary<string, string> FieldMapping { get; set; } = new();

    [PipeParameter(Label = "Unknown properties", Order = 9, Expressions = false, Advanced = true, ShowWhen = nameof(Mode), ShowWhenValues = ["insert", "update", "upsert"],
        Description = "What to do with properties the feature class has no field for")]
    public GisUnknownProperties UnknownProperties { get; set; } = GisUnknownProperties.Ignore;

    [PipeParameter(Label = "Item SRS", Order = 10, Expressions = false, Advanced = true,
        Description = "EPSG code of the items' geometries when they have no srs, e.g. 4326; 0 means that of the feature class")]
    public int ItemSrs { get; set; }

    [PipeParameter(Label = "Batch size", Order = 11, Expressions = false, Advanced = true,
        Description = "Features per write call, each in its own transaction; 0 uses the suggestion of the data source")]
    [Range(0, 100000)]
    public int BatchSize { get; set; }
}

[PipeNode("8d2f4b6a-1c3e-4f5a-9b7d-0e2c4a6b8d13", "Write Features",
    Category = "GIS",
    Description = "Writes GeoJSON items as features into a gView data source (PostGIS, SDE, shape files, …): "
                  + "insert, update, upsert or delete by a key field. Geometries are projected into the coordinate system of the feature class.")]
[PipeNodeExamples]
public sealed class WriteFeaturesNode : IPipeNode<WriteFeaturesParameters>, IPipeParameterOptions, IPipeDynamicPorts
{
    private readonly IServiceProvider? _services;

    public WriteFeaturesNode() { }

    /// <summary>The services let the editor list the feature classes of the selected data source.</summary>
    public WriteFeaturesNode(IServiceProvider services) { _services = services; }

    public Task<IReadOnlyList<PipeParameterOption>> GetOptionsAsync(string parameter, JsonObject parameters, bool refresh, CancellationToken cancellationToken)
        => GisFeatureClassOptions.GetAsync(_services, parameter, parameters, "featureClass", "credentialId", cancellationToken);

    public IReadOnlyList<PipePort> GetInputs(JsonObject parameters) => [new PipePort(PipePorts.Main)];

    // items that can't be written have an output of their own, in the way of the error output of the engine
    public IReadOnlyList<PipePort> GetOutputs(JsonObject parameters)
        => parameters["onError"]?.ToString().Equals("skipItem", StringComparison.OrdinalIgnoreCase) == true
            ? [new PipePort(PipePorts.Main), new PipePort(PipePorts.Error, "Error")]
            : [new PipePort(PipePorts.Main)];

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

        var result = parameters.Mode switch
        {
            GisWriteMode.Insert => await writer.InsertAsync(items, cancellationToken),
            GisWriteMode.Delete => await writer.DeleteAsync(items, cancellationToken),
            _ => await writer.UpdateAsync(items, upsert: parameters.Mode == GisWriteMode.Upsert, cancellationToken)
        };

        if (writer.IgnoredProperties.Count > 0)
        {
            context.Logger.LogWarning("Properties without a field in '{FeatureClass}' were not written: {Properties}",
                featureClass.Name, string.Join(", ", writer.IgnoredProperties.Order(StringComparer.OrdinalIgnoreCase)));
        }
        context.Logger.LogInformation("{Mode}: {Count} feature(s) of {Items} item(s) written to {FeatureClass}, {Errors} item(s) failed",
            parameters.Mode, result.Written.Count, items.Count, featureClass.Name, result.Errors.Count);

        return parameters.OnError == GisWriteOnError.SkipItem
            ? PipeNodeResult.FromPorts((PipePorts.Main, result.Written), (PipePorts.Error, result.Errors))
            : PipeNodeResult.FromMain(result.Written);
    }

    private sealed record WriteResult(List<PipeItem> Written, List<PipeItem> Errors);

    private sealed class Writer
    {
        private readonly IFeatureClass _featureClass;
        private readonly IFeatureUpdater _updater;
        private readonly WriteFeaturesParameters _parameters;
        private readonly int _batchSize;
        private readonly int? _classSrs;
        private readonly HashSet<string>? _writeFields;
        private readonly bool _skip;

        public Writer(IFeatureClass featureClass, IFeatureUpdater updater, WriteFeaturesParameters parameters, int batchSize)
        {
            _featureClass = featureClass;
            _updater = updater;
            _parameters = parameters;
            _batchSize = batchSize;
            _classSrs = GisSpatialReferences.Epsg(featureClass);
            _skip = parameters.OnError == GisWriteOnError.SkipItem;

            var fields = parameters.WriteFields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (fields.Length > 0)
            {
                _writeFields = new HashSet<string>(fields.Select(f => featureClass.FindField(f)?.name
                    ?? throw new PipeException($"The feature class '{featureClass.Name}' has no field '{f}' to write")), StringComparer.OrdinalIgnoreCase);
            }
            foreach (var target in parameters.FieldMapping.Keys)
            {
                if (featureClass.FindField(target) is null)
                {
                    throw new PipeException($"The feature class '{featureClass.Name}' has no field '{target}' (field mapping)");
                }
            }
        }

        /// <summary>Names of item properties the feature class has no field for.</summary>
        public HashSet<string> IgnoredProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

        public async Task<WriteResult> InsertAsync(IReadOnlyList<PipeItem> items, CancellationToken cancellationToken)
        {
            var errors = new List<PipeItem>();
            var entries = new List<(PipeItem Item, IFeature Feature)>();
            foreach (var item in items)
            {
                if (Try(() => ToFeature(item, oid: 0), item, errors) is { } feature)
                {
                    entries.Add((item, feature));
                }
            }

            var written = await WriteAsync(entries, insert: true, errors, cancellationToken);
            return new WriteResult(written.Select(w => Written(w.Item, w.Feature.OID)).ToList(), errors);
        }

        public async Task<WriteResult> UpdateAsync(IReadOnlyList<PipeItem> items, bool upsert, CancellationToken cancellationToken)
        {
            var errors = new List<PipeItem>();
            var updates = new List<(PipeItem Item, IFeature Feature)>();
            var inserts = new List<(PipeItem Item, IFeature Feature)>();
            foreach (var item in items)
            {
                try
                {
                    var oids = await FindAsync(item);
                    if (oids.Count == 0)
                    {
                        if (!upsert)
                        {
                            throw new PipeException($"No feature with the key {KeyText(item)} in '{_featureClass.Name}'");
                        }
                        inserts.Add((item, ToFeature(item, oid: 0)));
                    }
                    updates.AddRange(oids.Select(oid => (item, ToFeature(item, oid))));
                }
                catch (PipeException ex) when (_skip)
                {
                    errors.Add(Failed(item, ex.Message));
                }
            }

            var written = await WriteAsync(updates, insert: false, errors, cancellationToken);
            written.AddRange(await WriteAsync(inserts, insert: true, errors, cancellationToken));
            return new WriteResult(written.Select(w => Written(w.Item, w.Feature.OID)).ToList(), errors);
        }

        public async Task<WriteResult> DeleteAsync(IReadOnlyList<PipeItem> items, CancellationToken cancellationToken)
        {
            var written = new List<PipeItem>();
            var errors = new List<PipeItem>();
            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    foreach (var oid in await FindAsync(item))
                    {
                        Check(await _updater.Delete(_featureClass, oid), "delete");
                        written.Add(Written(item, oid));
                    }
                }
                catch (PipeException ex) when (_skip)
                {
                    errors.Add(Failed(item, ex.Message));
                }
            }
            return new WriteResult(written, errors);
        }

        // writes in batches; when a batch fails and items may be skipped, its items are written one by one
        private async Task<List<(PipeItem Item, IFeature Feature)>> WriteAsync(List<(PipeItem Item, IFeature Feature)> entries, bool insert,
            List<PipeItem> errors, CancellationToken cancellationToken)
        {
            var written = new List<(PipeItem Item, IFeature Feature)>();
            foreach (var batch in entries.Chunk(_batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await RunAsync(batch, insert))
                {
                    written.AddRange(batch);
                    continue;
                }

                if (!_skip)
                {
                    Check(false, insert ? "insert" : "update");
                }
                foreach (var entry in batch)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (await RunAsync([entry], insert))
                    {
                        written.Add(entry);
                    }
                    else
                    {
                        errors.Add(Failed(entry.Item, $"Can't {(insert ? "insert" : "update")} the feature: {_updater.LastErrorMessage}"));
                    }
                }
            }
            return written;
        }

        private async Task<bool> RunAsync((PipeItem Item, IFeature Feature)[] batch, bool insert)
        {
            var features = batch.Select(b => b.Feature).ToList();
            return insert
                ? await _updater.Insert(_featureClass, features, returnIds: true)
                : await _updater.Update(_featureClass, features);
        }

        // the feature of an item; an item that can't be converted is an error item when items may be skipped
        private IFeature? Try(Func<IFeature> convert, PipeItem item, List<PipeItem> errors)
        {
            try
            {
                return convert();
            }
            catch (PipeException ex) when (_skip)
            {
                errors.Add(Failed(item, ex.Message));
                return null;
            }
        }

        // object ids of the features with the item's key
        private async Task<List<int>> FindAsync(PipeItem item)
        {
            if (IsObjectIdKey(item))
            {
                return item.Json["id"] is JsonValue id && id.TryGetValue<int>(out var oid) && oid > 0
                    ? [oid]
                    : throw new PipeException("The item has no object id in 'id'; set a key field or read the features first");
            }

            var field = GisGeoJson.ToSqlField(_featureClass, _parameters.KeyField);
            var value = Properties(item)[field] ?? item.Json[field];
            var filter = new QueryFilter
            {
                SubFields = _featureClass.IDFieldName,
                WhereClause = $"{field} = {GisGeoJson.ToSqlLiteral(value)}"
            };

            var oids = new List<int>();
            using var cursor = await _featureClass.GetFeatures(filter);
            IFeature? feature;
            while (cursor is not null && (feature = await cursor.NextFeature()) is not null)
            {
                oids.Add(feature.OID);
            }
            return oids;
        }

        // the object id of the item is the key: no key field, or the id field of the feature class (e.g. FDB_OID) or "id" when the item has no property of that name
        private bool IsObjectIdKey(PipeItem item)
        {
            var key = _parameters.KeyField.Trim();
            return key.Length == 0
                || ((key.Equals("id", StringComparison.OrdinalIgnoreCase) || key.Equals(_featureClass.IDFieldName, StringComparison.OrdinalIgnoreCase))
                    && Properties(item)[key] is null);
        }

        private string KeyText(PipeItem item)
            => IsObjectIdKey(item)
                ? $"id = {item.Json["id"]?.ToJsonString()}"
                : $"{_parameters.KeyField} = {(Properties(item)[_parameters.KeyField] ?? item.Json[_parameters.KeyField])?.ToJsonString()}";

        // the properties of the item under the names of the fields (field mapping); the others keep their name
        private JsonObject Properties(PipeItem item)
        {
            var properties = item.Json["properties"] as JsonObject ?? new JsonObject();
            if (_parameters.FieldMapping.Count == 0)
            {
                return properties;
            }

            var mapped = new JsonObject();
            var sources = new HashSet<string>(_parameters.FieldMapping.Values, StringComparer.OrdinalIgnoreCase);
            foreach (var (name, value) in properties)
            {
                if (!sources.Contains(name))
                {
                    mapped[name] = value?.DeepClone();
                }
            }
            foreach (var (target, source) in _parameters.FieldMapping)
            {
                if (properties.FirstOrDefault(p => p.Key.Equals(source, StringComparison.OrdinalIgnoreCase)) is { Key: not null } property)
                {
                    mapped[target] = property.Value?.DeepClone();
                }
            }
            return mapped;
        }

        private IFeature ToFeature(PipeItem item, int oid)
        {
            var feature = new Feature { OID = oid };
            foreach (var (name, value) in Properties(item))
            {
                var field = _featureClass.FindField(name);
                if (field is null)
                {
                    if (_parameters.UnknownProperties == GisUnknownProperties.Fail)
                    {
                        throw new PipeException($"The feature class '{_featureClass.Name}' has no field '{name}'");
                    }
                    IgnoredProperties.Add(name);
                    continue;
                }
                if (field.name.Equals(_featureClass.IDFieldName, StringComparison.OrdinalIgnoreCase)
                    || field.name.Equals(_featureClass.ShapeFieldName, StringComparison.OrdinalIgnoreCase)
                    || (_writeFields is not null && !_writeFields.Contains(field.name)))
                {
                    continue;
                }
                feature.Fields.Add(new FieldValue(field.name, GisGeoJson.ToFieldValue(value)));
            }

            var geometry = _parameters.WriteGeometry == GisWriteGeometry.Never ? null : GisGeoJson.ToGeometry(item.Json["geometry"]);
            if (geometry is null && _parameters.WriteGeometry == GisWriteGeometry.Always)
            {
                throw new PipeException("The item has no geometry");
            }
            if (geometry is not null)
            {
                var itemSrs = GisSpatialReferences.ToEpsg(item.Json["srs"])
                    ?? (_parameters.ItemSrs > 0 ? _parameters.ItemSrs : _classSrs);
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

        static private PipeItem Failed(PipeItem item, string message)
        {
            var failed = item.Clone();
            failed.Json["error"] = new JsonObject { ["message"] = message, ["type"] = nameof(PipeException) };
            return failed;
        }

        private void Check(bool succeeded, string operation)
        {
            if (!succeeded)
            {
                throw new PipeException($"Can't {operation} features of '{_featureClass.Name}': {_updater.LastErrorMessage}");
            }
        }
    }
}

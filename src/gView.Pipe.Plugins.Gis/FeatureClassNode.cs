using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Geometry;
using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Data;
using gView.Pipe.Abstraction.Nodes;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis;

public enum GisFeatureClassOperation
{
    [PipeEnumValue("One item per feature class of the data source: name, geometry type and srs")]
    List,

    [PipeEnumValue("The schema of a feature class: geometry type, srs and fields, e.g. to create a copy of it")]
    Describe,

    [PipeEnumValue("Creates a feature class from a schema, e.g. the output of Describe; needs Allow schema changes")]
    Create,

    [PipeEnumValue("Creates the feature class only if it doesn't exist; else outputs its schema and changes nothing", Label = "Create if not exists")]
    CreateIfNotExists,

    [PipeEnumValue("Deletes all features; object ids are not reset. Needs Allow schema changes")]
    Truncate,

    [PipeEnumValue("Deletes the feature class with all its features; needs Allow schema changes")]
    Delete
}

public enum GisIfExists
{
    [PipeEnumValue("Stop with an error, e.g. so nothing gets overwritten by accident")]
    Fail,

    [PipeEnumValue("Delete the existing feature class with all its features and create it anew")]
    Replace
}

public sealed class FeatureClassParameters
{
    [Required, PipeCredential(GisDatasourceCredential.TypeId)]
    [PipeParameter(Label = "Data source", Order = 1, Expressions = false, Description = "gView Datasource credential, e.g. a test database")]
    public string? CredentialId { get; set; }

    [PipeParameter(Label = "Operation", Order = 2, Expressions = false, Description = "What to do with the feature class")]
    public GisFeatureClassOperation Operation { get; set; } = GisFeatureClassOperation.Describe;

    [PipeParameter(Label = "Feature class", Order = 3, Placeholder = "lv_lines",
        ShowWhen = nameof(Operation), ShowWhenValues = ["describe", "create", "createIfNotExists", "truncate", "delete"],
        Description = "Name of the feature class, e.g. lv_lines_test; for Create, empty takes the name of the schema")]
    public string FeatureClass { get; set; } = "";

    [PipeParameter(Label = "Schema", Order = 4, ShowWhen = nameof(Operation), ShowWhenValues = ["create", "createIfNotExists"],
        Description = "The schema to create, e.g. {{ $json.schema }}; empty takes the input item, e.g. the output of a Describe")]
    public JsonNode? Schema { get; set; }

    [PipeParameter(Label = "Additional fields", Order = 5, ShowWhen = nameof(Operation), ShowWhenValues = ["create", "createIfNotExists"],
        Description = "Fields to add to the schema, e.g. [{ \"name\": \"status\", \"type\": \"text\", \"size\": 20 }]; type is boolean, smallInteger, integer, bigInteger, float, double, text, date, guid or binary")]
    public JsonNode? AdditionalFields { get; set; }

    [PipeParameter(Label = "Exclude fields", Order = 6, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["create", "createIfNotExists"],
        Description = "Comma-separated fields of the schema to leave out, e.g. internal_note")]
    public string ExcludeFields { get; set; } = "";

    [PipeParameter(Label = "If it exists", Order = 7, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["create"],
        Description = "When the feature class is already there: stop, or replace it and lose its features")]
    public GisIfExists IfExists { get; set; } = GisIfExists.Fail;

    [PipeParameter(Label = "Target SRS", Order = 8, Expressions = false, ShowWhen = nameof(Operation), ShowWhenValues = ["create", "createIfNotExists"],
        Description = "EPSG code of the new feature class, e.g. 31256; 0 takes the srs of the schema")]
    [Range(0, int.MaxValue)]
    public int TargetSrs { get; set; }
}

[PipeNode("e7b1c9d3-5f2a-4e8b-a6c4-3d9f1b7e2a58", "Feature Class",
    Category = "GIS",
    Description = "Lists, describes, creates, truncates and deletes feature classes of a gView data source. "
                  + "Describe and Create pass the schema as item, so a feature class can be copied to another data source.")]
[PipeNodeExamples]
public sealed class FeatureClassNode : IPipeNode<FeatureClassParameters>
{
    public async Task<PipeNodeResult> ExecuteAsync(IPipeNodeContext context, CancellationToken cancellationToken)
    {
        var items = context.GetInput();
        var first = context.GetParameters<FeatureClassParameters>(items.Count > 0 ? items[0] : null);
        if (string.IsNullOrEmpty(first.CredentialId))
        {
            throw new PipeException("Select a data source");
        }
        var credential = await context.GetCredentialAsync<GisDatasourceCredential>(first.CredentialId, cancellationToken);

        using var dataset = await GisDatasets.OpenAsync(credential);

        if (first.Operation == GisFeatureClassOperation.List)
        {
            return PipeNodeResult.FromMain(await ListAsync(dataset));
        }

        var output = new List<PipeItem>();
        for (var i = 0; i < Math.Max(items.Count, 1); i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parameters = i == 0 ? first : context.GetParameters<FeatureClassParameters>(items[i], i);
            var item = parameters.Operation switch
            {
                GisFeatureClassOperation.Describe => GisSchema.ToItem(await GisDatasets.FeatureClassAsync(dataset, parameters.FeatureClass)),
                GisFeatureClassOperation.Create or GisFeatureClassOperation.CreateIfNotExists
                    => await CreateAsync(dataset, credential, parameters, Schema(parameters.Schema, items.Count > 0 ? items[i] : null)),
                GisFeatureClassOperation.Truncate => await TruncateAsync(dataset, credential, parameters.FeatureClass),
                GisFeatureClassOperation.Delete => await DeleteAsync(dataset, credential, parameters.FeatureClass),
                _ => throw new PipeException($"Unknown operation {parameters.Operation}")
            };
            context.Logger.LogInformation("{Operation} {FeatureClass}", parameters.Operation, item["name"]?.ToString());
            output.Add(new PipeItem(item));
        }

        return PipeNodeResult.FromMain(output);
    }

    static private async Task<List<PipeItem>> ListAsync(IFeatureDataset dataset)
        => (await dataset.Elements())
            .Select(e => e.Class)
            .OfType<IFeatureClass>()
            .OrderBy(fc => fc.Name, StringComparer.OrdinalIgnoreCase)
            .Select(fc => new PipeItem(new JsonObject
            {
                ["name"] = fc.Name,
                ["geometryType"] = GisSchema.GeometryName(fc.GeometryType),
                ["srs"] = GisSpatialReferences.Epsg(fc)
            }))
            .ToList();

    // the schema parameter, or the input item when it is empty; JSON text is parsed
    static private JsonObject Schema(JsonNode? schema, PipeItem? item)
    {
        if (schema is JsonValue value && value.TryGetValue<string>(out var text))
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                schema = null;
            }
            else
            {
                try
                {
                    schema = JsonNode.Parse(text);
                }
                catch (System.Text.Json.JsonException)
                {
                    throw new PipeException("The schema is text but no JSON object; use e.g. {{ $json.schema }}");
                }
            }
        }

        return (schema ?? item?.Json) as JsonObject
            ?? throw new PipeException("The schema must be an object, e.g. the output of Describe");
    }

    static internal async Task<JsonObject> CreateAsync(IFeatureDataset dataset, GisDatasourceCredential credential, FeatureClassParameters parameters, JsonObject schema)
    {
        var name = !string.IsNullOrWhiteSpace(parameters.FeatureClass)
            ? parameters.FeatureClass.Trim()
            : schema["name"]?.ToString().Trim() ?? "";
        if (name.Length == 0)
        {
            throw new PipeException("Enter the name of the feature class, or give the schema a name");
        }

        if (await FindAsync(dataset, name) is { } existing)
        {
            if (parameters.Operation == GisFeatureClassOperation.CreateIfNotExists)
            {
                return GisSchema.ToItem(existing);
            }
            if (parameters.IfExists == GisIfExists.Fail)
            {
                throw new PipeException($"The feature class '{existing.Name}' already exists");
            }
        }

        RequireSchemaChanges(credential, "create");
        var database = Database(dataset);
        var (geometryType, fields) = GisSchema.FromItem(GisSchema.Extend(schema, parameters.AdditionalFields, parameters.ExcludeFields));
        var srs = parameters.TargetSrs > 0 ? parameters.TargetSrs : GisSpatialReferences.ToEpsg(schema["srs"]);

        if (await FindAsync(dataset, name) is { } replaced && !await database.DeleteFeatureClass(replaced.Name))
        {
            throw new PipeException($"Can't delete the feature class '{replaced.Name}' to replace it: {database.LastErrorMessage}");
        }

        var geometryDef = new GeometryDef(geometryType, srs is > 0 ? GisSpatialReferences.FromEpsg(srs.Value) : null);
        if (await database.CreateFeatureClass(dataset.DatasetName, name, geometryDef, fields) < 0)
        {
            throw new PipeException($"Can't create the feature class '{name}': {database.LastErrorMessage}");
        }

        await dataset.RefreshClasses();
        // the database may have changed the case of the name
        var created = await FindAsync(dataset, name)
            ?? throw new PipeException($"The feature class '{name}' was created but can't be opened: {dataset.LastErrorMessage}");
        return GisSchema.ToItem(created);
    }

    static private async Task<JsonObject> TruncateAsync(IFeatureDataset dataset, GisDatasourceCredential credential, string name)
    {
        RequireSchemaChanges(credential, "truncate");
        var featureClass = await GisDatasets.FeatureClassAsync(dataset, name);
        var updater = GisDatasets.Updater(featureClass);

        var count = await featureClass.CountFeatures();
        if (!await updater.Delete(featureClass, "1=1"))
        {
            throw new PipeException($"Can't delete the features of '{featureClass.Name}': {updater.LastErrorMessage}");
        }
        return new JsonObject { ["name"] = featureClass.Name, ["deletedFeatures"] = Math.Max(count, 0) };
    }

    static private async Task<JsonObject> DeleteAsync(IFeatureDataset dataset, GisDatasourceCredential credential, string name)
    {
        RequireSchemaChanges(credential, "delete");
        var featureClass = await GisDatasets.FeatureClassAsync(dataset, name);
        var database = Database(dataset);

        if (!await database.DeleteFeatureClass(featureClass.Name))
        {
            throw new PipeException($"Can't delete the feature class '{featureClass.Name}': {database.LastErrorMessage}");
        }
        return new JsonObject { ["name"] = featureClass.Name, ["deleted"] = true };
    }

    static internal async Task<IFeatureClass?> FindAsync(IFeatureDataset dataset, string name)
    {
        if ((await dataset.Element(name))?.Class is IFeatureClass featureClass)
        {
            return featureClass;
        }
        return (await dataset.Elements())
            .Select(e => e.Class)
            .OfType<IFeatureClass>()
            .FirstOrDefault(fc => fc.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    static private IFeatureDatabase Database(IFeatureDataset dataset)
        => dataset.Database as IFeatureDatabase
           ?? throw new PipeException($"gView can't create or delete feature classes in a {dataset.ProviderName} data source");

    static private void RequireSchemaChanges(GisDatasourceCredential credential, string operation)
    {
        if (!credential.AllowSchemaChanges)
        {
            throw new PipeException($"The data source doesn't allow schema changes, so the node can't {operation} feature classes; switch on Allow schema changes in the credential");
        }
    }
}

using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.FDB;
using gView.Framework.Data;
using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Data;
using gView.Pipe.Abstraction.Nodes;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis;

public enum GisCopyMode
{
    [PipeEnumValue("Creates the target feature class from the schema of the source and copies the features; stops when it exists")]
    Create,

    [PipeEnumValue("Adds the features to the existing target feature class; fields the target hasn't are left out")]
    Append,

    [PipeEnumValue("Deletes the target feature class with all its features if it exists, creates it anew and copies the features")]
    Replace
}

public sealed class CopyFeatureClassParameters
{
    [Required, PipeCredential(GisDatasourceCredential.TypeId)]
    [PipeParameter(Label = "Source data source", Order = 1, Expressions = false, Description = "gView Datasource credential the features come from")]
    public string? SourceCredentialId { get; set; }

    [Required, PipeOptions(Refreshable = true), PipeParameter(Label = "Source feature class", Order = 2, Expressions = false,
        Description = "Feature class to copy, one of the source data source")]
    public string SourceFeatureClass { get; set; } = "";

    [Required, PipeCredential(GisDatasourceCredential.TypeId)]
    [PipeParameter(Label = "Target data source", Order = 3, Expressions = false, Description = "gView Datasource credential the features go to; may be the same")]
    public string? TargetCredentialId { get; set; }

    [PipeParameter(Label = "Target feature class", Order = 4, Expressions = false, Placeholder = "lv_lines_copy",
        Description = "Name of the target feature class; empty takes the name of the source")]
    public string TargetFeatureClass { get; set; } = "";

    [PipeParameter(Label = "Mode", Order = 5, Expressions = false, Description = "Create, add to or replace the target feature class")]
    public GisCopyMode Mode { get; set; } = GisCopyMode.Create;

    [PipeParameter(Label = "Where", Order = 6, Code = true, CodeLanguage = "sql",
        Description = "Fixed filter in the SQL dialect of the source, e.g. status = 'active'; not evaluated, so it holds no {{ }}")]
    public string Where { get; set; } = "";

    [PipeParameter(Label = "Spatial filter", Order = 7, Expressions = false, Description = "How the features relate to the filter geometry")]
    public GisSpatialFilter SpatialFilter { get; set; } = GisSpatialFilter.None;

    [PipeParameter(Label = "Filter geometry", Order = 8, ShowWhen = nameof(SpatialFilter), ShowWhenValues = ["intersects", "envelopeIntersects", "within", "contains"],
        Description = "A GeoJSON geometry or feature, or a bounding box [minx, miny, maxx, maxy]")]
    public JsonNode? FilterGeometry { get; set; }

    [PipeParameter(Label = "Filter geometry SRS", Order = 9, ShowWhen = nameof(SpatialFilter), ShowWhenValues = ["intersects", "envelopeIntersects", "within", "contains"],
        Description = "EPSG code of the filter geometry; empty means that of the source feature class")]
    public JsonNode? FilterSrs { get; set; }

    [PipeParameter(Label = "Additional fields", Order = 10, ShowWhen = nameof(Mode), ShowWhenValues = ["create", "replace"],
        Description = "Fields to add to the target, e.g. [{ \"name\": \"status\", \"type\": \"text\", \"size\": 20 }]; they stay empty")]
    public JsonNode? AdditionalFields { get; set; }

    [PipeParameter(Label = "Exclude fields", Order = 11, Expressions = false, ShowWhen = nameof(Mode), ShowWhenValues = ["create", "replace"],
        Description = "Comma-separated fields of the source to leave out, e.g. internal_note")]
    public string ExcludeFields { get; set; } = "";

    [PipeParameter(Label = "Target SRS", Order = 12, Expressions = false,
        Description = "EPSG code of the target; 0 takes that of the source (Create, Replace) or of the existing target feature class (Append)")]
    [Range(0, int.MaxValue)]
    public int TargetSrs { get; set; }

    [PipeParameter(Label = "Batch size", Order = 13, Expressions = false, Advanced = true,
        Description = "Features per write call, each in its own transaction; 0 uses the suggestion of the data source")]
    [Range(0, 100000)]
    public int BatchSize { get; set; }
}

[PipeNode("5a9c3e7b-4d1f-4b82-8e6a-7c0d2f4b9a31", "Copy Feature Class",
    Category = "GIS",
    Description = "Copies a feature class, with its schema and features, to the same or another gView data source. "
                  + "The features go from one data source to the other without passing through the workflow, so it suits volumes too large for items. "
                  + "Needs Allow schema changes in the credential of the target to create or replace it.")]
[PipeNodeExamples]
public sealed class CopyFeatureClassNode : IPipeNode<CopyFeatureClassParameters>, IPipeParameterOptions
{
    private readonly IServiceProvider? _services;

    public CopyFeatureClassNode() { }

    /// <summary>The services let the editor list the feature classes of the selected data source.</summary>
    public CopyFeatureClassNode(IServiceProvider services) { _services = services; }

    public Task<IReadOnlyList<PipeParameterOption>> GetOptionsAsync(string parameter, JsonObject parameters, bool refresh, CancellationToken cancellationToken)
        => GisFeatureClassOptions.GetAsync(_services, parameter, parameters, "sourceFeatureClass", "sourceCredentialId", cancellationToken);

    public async Task<PipeNodeResult> ExecuteAsync(IPipeNodeContext context, CancellationToken cancellationToken)
    {
        var items = context.GetInput();
        // the where clause is never evaluated
        var where = context.RawParameters["where"]?.GetValue<string>() ?? "";
        var parameters = context.GetParameters<CopyFeatureClassParameters>(items.Count > 0 ? items[0] : null);
        if (string.IsNullOrEmpty(parameters.SourceCredentialId) || string.IsNullOrEmpty(parameters.TargetCredentialId))
        {
            throw new PipeException("Select the source and the target data source");
        }

        var sourceCredential = await context.GetCredentialAsync<GisDatasourceCredential>(parameters.SourceCredentialId, cancellationToken);
        var targetCredential = await context.GetCredentialAsync<GisDatasourceCredential>(parameters.TargetCredentialId, cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        using var sourceDataset = await GisDatasets.OpenAsync(sourceCredential);
        using var targetDataset = await GisDatasets.OpenAsync(targetCredential);

        var source = await GisDatasets.FeatureClassAsync(sourceDataset, parameters.SourceFeatureClass);
        var targetName = string.IsNullOrWhiteSpace(parameters.TargetFeatureClass) ? source.Name : parameters.TargetFeatureClass.Trim();
        if (parameters.SourceCredentialId == parameters.TargetCredentialId && targetName.Equals(source.Name, StringComparison.OrdinalIgnoreCase))
        {
            throw new PipeException($"Source and target are the same feature class '{source.Name}'; give the target another name or data source");
        }

        // the schema first: a failure here changes nothing but the target
        var target = parameters.Mode == GisCopyMode.Append
            ? await GisDatasets.FeatureClassAsync(targetDataset, targetName)
            : await CreateTargetAsync(targetDataset, targetCredential, source, targetName, parameters);

        var count = await CopyAsync(source, target, parameters, where, cancellationToken);
        stopwatch.Stop();
        context.Logger.LogInformation("{Count} feature(s) copied from {Source} to {Target} in {Seconds:0.0} s",
            count, source.Name, target.Name, stopwatch.Elapsed.TotalSeconds);

        return PipeNodeResult.FromMain([new PipeItem(new JsonObject
        {
            ["source"] = source.Name,
            ["target"] = target.Name,
            ["copied"] = count,
            ["seconds"] = Math.Round(stopwatch.Elapsed.TotalSeconds, 2)
        })]);
    }

    static private async Task<IFeatureClass> CreateTargetAsync(IFeatureDataset dataset, GisDatasourceCredential credential, IFeatureClass source,
        string name, CopyFeatureClassParameters parameters)
    {
        // the same code as the node Feature Class: the schema of the source with the extra and left out fields
        await FeatureClassNode.CreateAsync(dataset, credential, new FeatureClassParameters
        {
            Operation = GisFeatureClassOperation.Create,
            FeatureClass = name,
            IfExists = parameters.Mode == GisCopyMode.Replace ? GisIfExists.Replace : GisIfExists.Fail,
            TargetSrs = parameters.TargetSrs,
            AdditionalFields = parameters.AdditionalFields,
            ExcludeFields = parameters.ExcludeFields
        }, GisSchema.ToItem(source));

        return await GisDatasets.FeatureClassAsync(dataset, name);
    }

    static private async Task<int> CopyAsync(IFeatureClass source, IFeatureClass target, CopyFeatureClassParameters parameters, string where,
        CancellationToken cancellationToken)
    {
        var updater = GisDatasets.Updater(target);
        var batchSize = parameters.BatchSize > 0
            ? parameters.BatchSize
            : Math.Max(updater.SuggestedInsertFeatureCountPerTransaction, 100);

        var filter = ReadFeaturesNode.CreateFilter(source, new ReadFeaturesParameters
        {
            SpatialFilter = parameters.SpatialFilter,
            FilterGeometry = parameters.FilterGeometry,
            FilterSrs = parameters.FilterSrs
        }, where);

        // the source projects the geometries into the coordinate system of the target
        var sourceSrs = GisSpatialReferences.Epsg(source);
        var targetSrs = GisSpatialReferences.Epsg(target);
        if (targetSrs is > 0 && sourceSrs != targetSrs)
        {
            if (sourceSrs is null)
            {
                throw new PipeException($"The coordinate system of '{source.Name}' is unknown; it can't be projected");
            }
            filter.SetFeatureSpatialReference(GisSpatialReferences.FromEpsg(targetSrs.Value), null);
        }

        var targetFields = target.Fields.ToEnumerable()
            .Where(f => !f.name.Equals(target.IDFieldName, StringComparison.OrdinalIgnoreCase)
                        && !f.name.Equals(target.ShapeFieldName, StringComparison.OrdinalIgnoreCase))
            .Select(f => f.name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var excluded = parameters.ExcludeFields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var total = 0;
        var batch = new List<IFeature>(batchSize);
        using var cursor = await source.GetFeatures(filter);
        IFeature? feature;
        while (cursor is not null && (feature = await cursor.NextFeature()) is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var copy = new Feature { Shape = feature.Shape };
            foreach (var field in feature.Fields)
            {
                if (targetFields.Contains(field.Name) && !excluded.Contains(field.Name))
                {
                    copy.Fields.Add(new FieldValue(target.FindField(field.Name)!.name, field.Value));
                }
            }
            batch.Add(copy);

            if (batch.Count >= batchSize)
            {
                total += await WriteAsync(updater, target, batch, total);
            }
        }

        if (batch.Count > 0)
        {
            total += await WriteAsync(updater, target, batch, total);
        }
        return total;
    }

    static private async Task<int> WriteAsync(IFeatureUpdater updater, IFeatureClass target, List<IFeature> batch, int copied)
    {
        var count = batch.Count;
        if (!await updater.Insert(target, batch))
        {
            throw new PipeException($"Can't insert features into '{target.Name}' after {copied} copied feature(s): {updater.LastErrorMessage}");
        }
        batch.Clear();
        return count;
    }
}

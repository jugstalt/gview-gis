using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Data.Filters;
using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Data;
using gView.Pipe.Abstraction.Nodes;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis;

public enum GisSpatialFilter
{
    [PipeEnumValue("No spatial filter: all features that match the other filters")]
    None,

    [PipeEnumValue("Features that touch or cross the filter geometry, e.g. the lines in a district")]
    Intersects,

    [PipeEnumValue("Features whose bounding box touches the filter geometry's; fast, but not exact", Label = "Envelope intersects")]
    EnvelopeIntersects,

    [PipeEnumValue("Features completely inside the filter geometry")]
    Within,

    [PipeEnumValue("Features that completely contain the filter geometry, e.g. the parcel a point lies in")]
    Contains
}

public sealed class ReadFeaturesParameters
{
    [Required, PipeCredential(GisDatasourceCredential.TypeId)]
    [PipeParameter(Label = "Data source", Order = 1, Expressions = false, Description = "gView Datasource credential, e.g. the PostGIS database of the grid")]
    public string? CredentialId { get; set; }

    [Required, PipeParameter(Label = "Feature class", Order = 2, Placeholder = "lv_lines",
        Description = "Name of the feature class (table) to read, e.g. public.lv_lines")]
    public string FeatureClass { get; set; } = "";

    [PipeParameter(Label = "Where", Order = 3, Code = true, CodeLanguage = "sql",
        Description = "Fixed filter in the SQL dialect of the source, e.g. status = 'active'; not evaluated, so it holds no {{ }}. Values of items go into Match field")]
    public string Where { get; set; } = "";

    [PipeParameter(Label = "Match field", Order = 4, Expressions = false,
        Description = "Only features whose field has the value of Match value, e.g. station_id")]
    public string MatchField { get; set; } = "";

    [PipeParameter(Label = "Match value", Order = 5,
        Description = "Value Match field must have, e.g. {{ $json.properties.id }}; a number or text, never inserted as SQL")]
    public JsonNode? MatchValue { get; set; }

    [PipeParameter(Label = "Spatial filter", Order = 6, Expressions = false, Description = "How the features relate to the filter geometry")]
    public GisSpatialFilter SpatialFilter { get; set; } = GisSpatialFilter.None;

    [PipeParameter(Label = "Filter geometry", Order = 7, ShowWhen = nameof(SpatialFilter), ShowWhenValues = ["intersects", "envelopeIntersects", "within", "contains"],
        Description = "A GeoJSON geometry or feature, e.g. {{ $json.geometry }}")]
    public JsonNode? FilterGeometry { get; set; }

    [PipeParameter(Label = "Filter geometry SRS", Order = 8, ShowWhen = nameof(SpatialFilter), ShowWhenValues = ["intersects", "envelopeIntersects", "within", "contains"],
        Description = "EPSG code of the filter geometry, e.g. {{ $json.srs }}; empty means that of the feature class")]
    public JsonNode? FilterSrs { get; set; }

    [PipeParameter(Label = "Fields", Order = 9, Expressions = false,
        Description = "Comma-separated fields to read, e.g. id, name, voltage; empty reads all")]
    public string Fields { get; set; } = "";

    [PipeParameter(Label = "Target SRS", Order = 10, Expressions = false,
        Description = "EPSG code to project the geometries into, e.g. 4326; 0 keeps the coordinate system of the source")]
    public int TargetSrs { get; set; }

    [PipeParameter(Label = "Limit", Order = 11, Expressions = false, Advanced = true, Description = "At most this many features per input item; 0 reads all")]
    [Range(0, int.MaxValue)]
    public int Limit { get; set; }

    [PipeParameter(Label = "Read geometry", Order = 12, Expressions = false, Advanced = true, Description = "Off reads only the attributes, e.g. for a list of ids")]
    public bool ReadGeometry { get; set; } = true;
}

[PipeNode("c3a5e7f1-2b4d-4c6e-8a0f-1d3b5c7e9f21", "Read Features",
    Category = "GIS",
    Description = "Reads features of a gView data source (PostGIS, SDE, shape files, …) as GeoJSON items, once per input item. "
                  + "Filters by a fixed where clause, a field value and a geometry.")]
[PipeNodeExamples]
public sealed class ReadFeaturesNode : IPipeNode<ReadFeaturesParameters>
{
    public async Task<PipeNodeResult> ExecuteAsync(IPipeNodeContext context, CancellationToken cancellationToken)
    {
        var items = context.GetInput();
        // the where clause is never evaluated: values of items only go through Match value
        var where = context.RawParameters["where"]?.GetValue<string>() ?? "";

        var first = context.GetParameters<ReadFeaturesParameters>(items.Count > 0 ? items[0] : null);
        if (string.IsNullOrEmpty(first.CredentialId))
        {
            throw new PipeException("Select a data source");
        }
        var credential = await context.GetCredentialAsync<GisDatasourceCredential>(first.CredentialId, cancellationToken);

        using var dataset = await GisDatasets.OpenAsync(credential);

        var output = new List<PipeItem>();
        for (var i = 0; i < Math.Max(items.Count, 1); i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parameters = i == 0 ? first : context.GetParameters<ReadFeaturesParameters>(items[i], i);
            var featureClass = await GisDatasets.FeatureClassAsync(dataset, parameters.FeatureClass);
            var filter = CreateFilter(featureClass, parameters, where);

            var sourceSrs = GisSpatialReferences.Epsg(featureClass);
            var srs = sourceSrs;
            if (parameters.TargetSrs > 0 && parameters.TargetSrs != sourceSrs)
            {
                if (sourceSrs is null)
                {
                    throw new PipeException($"The coordinate system of '{featureClass.Name}' is unknown; it can't be projected");
                }
                filter.SetFeatureSpatialReference(GisSpatialReferences.FromEpsg(parameters.TargetSrs), null);
                srs = parameters.TargetSrs;
            }

            var before = output.Count;
            using (var cursor = await featureClass.GetFeatures(filter))
            {
                IFeature? feature;
                while (cursor is not null && (feature = await cursor.NextFeature()) is not null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    output.Add(new PipeItem(GisGeoJson.ToItem(feature, featureClass, srs)));
                    if (parameters.Limit > 0 && output.Count - before >= parameters.Limit)
                    {
                        break;
                    }
                }
            }
            context.Logger.LogInformation("{Count} feature(s) read from {FeatureClass}", output.Count - before, featureClass.Name);
        }

        return PipeNodeResult.FromMain(output);
    }

    static private IQueryFilter CreateFilter(IFeatureClass featureClass, ReadFeaturesParameters parameters, string where)
    {
        IQueryFilter filter;
        if (parameters.SpatialFilter != GisSpatialFilter.None)
        {
            var geometry = GisGeoJson.ToGeometry(parameters.FilterGeometry)
                ?? throw new PipeException("The spatial filter needs a filter geometry");
            filter = new SpatialFilter
            {
                Geometry = geometry,
                SpatialRelation = parameters.SpatialFilter switch
                {
                    GisSpatialFilter.EnvelopeIntersects => spatialRelation.SpatialRelationEnvelopeIntersects,
                    GisSpatialFilter.Within => spatialRelation.SpatialRelationWithin,
                    GisSpatialFilter.Contains => spatialRelation.SpatialRelationContains,
                    _ => spatialRelation.SpatialRelationIntersects
                },
                FilterSpatialReference = GisSpatialReferences.ToEpsg(parameters.FilterSrs) is { } filterSrs
                    ? GisSpatialReferences.FromEpsg(filterSrs)
                    : featureClass.SpatialReference
            };
        }
        else
        {
            filter = new QueryFilter();
        }

        var clauses = new List<string>();
        if (!string.IsNullOrWhiteSpace(where))
        {
            clauses.Add($"({where.Trim()})");
        }
        if (!string.IsNullOrWhiteSpace(parameters.MatchField))
        {
            clauses.Add($"{GisGeoJson.ToSqlField(featureClass, parameters.MatchField)} = {GisGeoJson.ToSqlLiteral(parameters.MatchValue)}");
        }
        filter.WhereClause = string.Join(" AND ", clauses);

        var fields = parameters.Fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (fields.Length == 0)
        {
            filter.SubFields = "*";
        }
        else
        {
            filter.SubFields = string.Join(",", fields.Select(f => GisGeoJson.ToSqlField(featureClass, f)));
            if (!string.IsNullOrEmpty(featureClass.IDFieldName) && !filter.HasField(featureClass.IDFieldName))
            {
                filter.AddField(featureClass.IDFieldName);
            }
        }

        if (parameters.ReadGeometry && !string.IsNullOrEmpty(featureClass.ShapeFieldName) && fields.Length > 0)
        {
            filter.AddField(featureClass.ShapeFieldName);
        }
        else if (!parameters.ReadGeometry && fields.Length == 0)
        {
            // all fields but the geometry
            filter.SubFields = string.Join(",", featureClass.Fields.ToEnumerable()
                .Where(f => !f.name.Equals(featureClass.ShapeFieldName, StringComparison.OrdinalIgnoreCase))
                .Select(f => f.name));
        }

        if (parameters.Limit > 0)
        {
            filter.Limit = parameters.Limit;
        }

        return filter;
    }
}

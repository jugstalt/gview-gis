using System.Text.Json.Serialization;

namespace gView.Cmd.ElasticSearch.Lib.Models;

public class ImportConfig
{
    [JsonPropertyName("elastic_search_connection")]
    public ElasticSearchConnection? Connection
    {
        get; set;
    }

    [JsonPropertyName("index_schema")]
    public IndexSchema? Schema { get; set; }

    [JsonPropertyName("datasets")]
    public DatasetConnection[]? Datasets
    {
        get; set;
    }

    #region Classes

    public class ElasticSearchConnection
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = "";

        [JsonPropertyName("default_index")]
        public string DefaultIndex { get; set; } = "";

        [JsonPropertyName("delete_index")]
        public bool DeleteIndex { get; set; }

        public string MetaIndex => $"{this.DefaultIndex}.meta";
    }

    public class DatasetConnection
    {
        [JsonPropertyName("dataset_guid")]
        public Guid DatasetGuid { get; set; }

        [JsonPropertyName("connection_string")]
        public string ConnectionString { get; set; } = "";

        [JsonPropertyName("feature_classes")]
        public IEnumerable<FeatureClassDefinition>? FeatureClasses { get; set; }
    }

    public class FeatureClassDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("srs")]
        public int SRefId { get; set; }

        [JsonPropertyName("objectid_fieldname")]
        public string ObjectOidField { get; set; } = "";

        [JsonPropertyName("geometry")]
        public bool UserGeometry { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        [JsonPropertyName("index_proto")]
        public Item? IndexItemProto { get; set; }

        [JsonPropertyName("metadata")]
        public Meta? Meta { get; set; }

        [JsonPropertyName("replace")]
        public IEnumerable<Replacements>? Replacements { get; set; }

        [JsonPropertyName("filter")]
        public string Filter { get; set; } = "";
    }

    public class IndexSchema
    {
        [JsonPropertyName("fields")]
        public IEnumerable<IndexSchemaField>? Fields { get; set; }
    }
    public class IndexSchemaField
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("indexed")]
        public bool Indexed { get; set; }
    }

    public class Replacements
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = "";

        [JsonPropertyName("to")]
        public string To { get; set; } = "";
    }


    #endregion
}

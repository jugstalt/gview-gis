using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Cmd.FillElasticSearch
{
    public class ImportConfig
    {
        [JsonProperty(PropertyName = "elastic_search_connection")]
        public ElasticSearchConnection Connection
        {
            get; set;
        }

        [JsonProperty(PropertyName = "index_schema")]
        public IndexSchema Schema { get; set; }

        [JsonProperty(PropertyName = "datasets")]
        public DatasetConnection[] Datasets
        {
            get; set;
        }

        #region Classes

        public class ElasticSearchConnection
        {
            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "default_index")]
            public string DefaultIndex { get; set; }

            [JsonProperty(PropertyName = "delete_index")]
            public bool DeleteIndex { get; set; }
        }

        public class DatasetConnection
        {
            [JsonProperty(PropertyName = "dataset_guid")]
            public Guid DatasetGuid { get; set; }

            [JsonProperty(PropertyName = "connection_string")]
            public string ConnectionString { get; set; }

            [JsonProperty(PropertyName = "feature_classes")]
            public IEnumerable<FeatureClassDefinition> FeatureClasses { get; set; }
        }

        public class FeatureClassDefinition
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "srs")]
            public int SRefId { get; set; }

            [JsonProperty(PropertyName = "objectid_fieldname")]
            public string ObjectOidField { get; set; }

            [JsonProperty(PropertyName = "geometry")]
            public bool UserGeometry { get; set; }

            [JsonProperty(PropertyName = "category")]
            public string Category { get; set; }

            [JsonProperty(PropertyName = "index_proto")]
            public Item IndexItemProto { get; set; }

            [JsonProperty(PropertyName = "metadata")]
            public Meta Meta { get; set; }

            [JsonProperty(PropertyName = "replace")]
            public IEnumerable<Replacements> Replacements { get; set; }

            [JsonProperty(PropertyName = "filter")]
            public string Filter { get; set; }
        }

        public class IndexSchema
        {
            [JsonProperty(PropertyName = "fields")]
            public IEnumerable<IndexSchemaField> Fields { get; set; }
        }
        public class IndexSchemaField
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "indexed")]
            public bool Indexed { get; set; }
        }

        public class Replacements
        {
            [JsonProperty(PropertyName = "from")]
            public string From { get; set; }

            [JsonProperty(PropertyName = "to")]
            public string To { get; set; }
        }


        #endregion
    }
}

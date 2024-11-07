using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gView.Server.AppCode
{
    public class MapServerConfig
    {
        [JsonPropertyName("services-folder")]
        public string ServiceFolder { get; set; }

        [JsonPropertyName("output-path")]
        public string OuputPath { get; set; }
        [JsonPropertyName("output-url")]
        public string OutputUrl { get; set; }
        [JsonPropertyName("onlineresource-url")]
        public string OnlineResourceUrl { get; set; }

        [JsonPropertyName("tilecache-root")]
        public string TileCacheRoot { get; set; }

        [JsonPropertyName("security")]
        public SecurityConfig Security { get; set; }

        [JsonPropertyName("task-queue")]
        public TaskQueueConfig TaskQueue { get; set; }

        [JsonPropertyName("mapserver-defaults")]
        public MapServerDefaultsConfig MapServerDefaults { get; set; }

        [JsonPropertyName("external-auth-authority")]
        public ExtAuthAuthority ExternalAuthAuthority { get; set; }

        [JsonPropertyName("allowFormsLogin")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AllowFormsLogin { get; set; }

        [JsonPropertyName("force-https")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ForceHttps { get; set; }

        #region Classes

        public class SecurityConfig
        {

        }

        public class TaskQueueConfig
        {
            [JsonPropertyName("max-parallel-tasks")]
            public int MaxParallelTasks { get; set; }

            [JsonPropertyName("max-queue-length")]
            public int MaxQueueLength { get; set; }
        }

        public class MapServerDefaultsConfig
        {
            [JsonPropertyName("maxImageWidth")]
            public int MaxImageWidth { get; set; }

            [JsonPropertyName("maxImageHeight")]
            public int MaxImageHeight { get; set; }

            [JsonPropertyName("maxRecordCount")]
            public int MaxRecordCount { get; set; }
        }

        public class ExtAuthAuthority
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("allow-access-token")]
            public bool AllowAccessToken { get; set; }
        }

        #endregion
    }
}

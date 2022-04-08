using Newtonsoft.Json;

namespace gView.Server.AppCode
{
    public class MapServerConfig
    {
        [JsonProperty("services-folder")]
        public string ServiceFolder { get; set; }

        [JsonProperty("output-path")]
        public string OuputPath { get; set; }
        [JsonProperty("output-url")]
        public string OutputUrl { get; set; }
        [JsonProperty("onlineresource-url")]
        public string OnlineResourceUrl { get; set; }

        [JsonProperty("tilecache-root")]
        public string TileCacheRoot { get; set; }

        [JsonProperty("security")]
        public SecurityConfig Security { get; set; }

        [JsonProperty("task-queue")]
        public TaskQueueConfig TaskQueue { get; set; }

        [JsonProperty("external-auth-authority")]
        public ExtAuthAuthority ExternalAuthAuthority { get; set; }

        [JsonProperty("allowFormsLogin", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AllowFormsLogin { get; set; }

        [JsonProperty("force-https", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ForceHttps { get; set; }

        #region Classes

        public class SecurityConfig
        {

        }

        public class TaskQueueConfig
        {
            [JsonProperty("max-parallel-tasks")]
            public int MaxParallelTasks { get; set; }

            [JsonProperty("max-queue-length")]
            public int MaxQueueLength { get; set; }
        }

        public class ExtAuthAuthority
        {
            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("allow-access-token")]
            public bool AllowAccessToken { get; set; }
        }

        #endregion
    }
}

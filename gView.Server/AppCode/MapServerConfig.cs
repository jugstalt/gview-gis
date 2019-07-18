using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public TaskQueueConfig TaskQueue {get;set;}

        [JsonProperty("external-auth-service")]
        public ExtAuthService ExternalAuthService { get; set; }

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

        public class ExtAuthService
        {
            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("assembly")]
            public string AssemblyName { get; set; }

            [JsonProperty("instance")]
            public string InstanceName { get; set; }

            [JsonProperty("method")]
            string MethodName { get; set; }

            public bool IsValid
            {
                get
                {
                    return !String.IsNullOrWhiteSpace("url") &&
                           !String.IsNullOrWhiteSpace("assembly") &&
                           !String.IsNullOrWhiteSpace("instance") &&
                           !String.IsNullOrWhiteSpace("method");
                }
            }
        }

        #endregion
    }
}

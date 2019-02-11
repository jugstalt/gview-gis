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

        [JsonProperty("task-queue")]
        public TaskQueueConfig TaskQueue {get;set;}

        #region Classes

        public class TaskQueueConfig
        {
            [JsonProperty("max-parallel-tasks")]
            public int MaxParallelTasks { get; set; }

            [JsonProperty("max-queue-length")]
            public int MaxQueueLength { get; set; }
        }

        #endregion
    }
}

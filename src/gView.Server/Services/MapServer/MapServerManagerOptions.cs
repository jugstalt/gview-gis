namespace gView.Server.Services.MapServer
{
    public class MapServerManagerOptions
    {
        public bool IsValid { get; set; }

        public string AppRootPath { get; set; }

        public string ServicesPath { get; set; }
        public string OutputPath { get; set; }
        public string OutputUrl { get; set; }
        public string TileCachePath { get; set; }
        public string OnlineResource { get; set; }

        public string LoginManagerRootPath { get; set; }
        public string LoggingRootPath { get; set; }

        public bool LogServiceErrors { get; set; }
        public bool LogServiceRequests { get; set; }
        public bool LogServiceRequestDetails { get; set; }

        public bool AllowFormsLogin { get; set; }
        public bool ForceHttps { get; set; }

        public int TaskQueue_MaxThreads { get; set; }
        public int TaskQueue_QueueLength { get; set; }

        public int Port { get; set; }

    }
}

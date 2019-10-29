using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class Globals
    {
        private static int _maxThreads = 2, _queueLength = 100;

        public static bool HasValidConfig = true;
        public static string ConfigErrorMessage = String.Empty;

        public static string OutputPath
        {
            get
            {
                return InternetMapServer.OutputPath;
            }
        }

        public static string TileCachePath
        {
            get
            {
                return InternetMapServer.TileCachePath;
            }
        }
        public static string OutputUrl
        {
            get
            {
                return InternetMapServer.OutputUrl;
            }
        }

        public static string AppRootPath { get; set; }

        public static string LoginManagerRootPath { get; set; }

        public static string LoggingRootPath { get; set; }

        public static int MaxThreads
        {
            get { return _maxThreads; }
            set { _maxThreads = value; }
        }
        public static int QueueLength
        {
            get { return _queueLength; }
            set { _queueLength = value; }
        }

        public static bool ForceHttps { get; set; }

        public static bool log_requests
        {
            get
            {
                return false;
            }
        }
        public static bool log_request_details
        {
            get
            {
                return false;
            }
        }
        public static bool log_errors
        {
            get
            {
                return true;
            }
        }
        public static bool log_archive
        {
            get
            {
                return false;
            }
        }

        public static MapServerConfig.ExtAuthService ExternalAuthService = null;

        public static bool AllowFormsLogin { get; set; }

        //internal static string MasterPassword { get; set; }
    }
}

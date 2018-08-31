using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    class Globals
    {
        private static int _maxThreads = 2, _queueLength = 100;

        public static string outputPath
        {
            get
            {
                return InternetMapServer.OutputPath;
            }
        }

        public static string tileCachePath
        {
            get
            {
                return InternetMapServer.TileCachePath;
            }
        }
        public static string outputUrl
        {
            get
            {
                return InternetMapServer.OutputUrl;
            }
        }

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
    }
}

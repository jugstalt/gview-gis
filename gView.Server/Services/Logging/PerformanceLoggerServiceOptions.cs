using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Services.Logging
{
    public class PerformanceLoggerServiceOptions
    {
        public string ConnectionString { get; set; }
        public string LoggerType { get; set; }
    }
}

using gView.Framework.Logging.ResourceLogging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Services.Logging
{
    public class PerformanceLoggerService
    {
        private readonly IPerformanceLogger _resourceLogger;

        public PerformanceLoggerService(IOptionsMonitor<PerformanceLoggerServiceOptions> optionsMonitor)
        {
            var options = optionsMonitor.CurrentValue;

            switch (options?.LoggerType?.ToLower())
            {
                case "azuretable":
                    _resourceLogger = new AzureTablePerformanceLogger();
                    Console.WriteLine("Added AzureTable resource logger");
                    break;
            }

            if (_resourceLogger != null)
            {
                _resourceLogger.Init(options.ConnectionString);
            }
        }

        public void Log(IPerformanceLoggerItem item)
        {
            if(_resourceLogger!=null)
            {
                _resourceLogger.Log(item);  // run and forget
            }
        }

        public void Flush()
        {
            if(_resourceLogger!=null)
            {
                _resourceLogger.Flush();
            }
        }

        public bool UseLogging => _resourceLogger != null;
    }
}

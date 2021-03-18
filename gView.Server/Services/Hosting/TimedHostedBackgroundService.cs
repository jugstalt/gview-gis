using gView.Server.Services.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace gView.Server.Services.Hosting
{
    public class TimedHostedBackgroundService : IHostedService, IDisposable
    {
        private Timer _timer;
        private int counter = 0;

        private readonly PerformanceLoggerService _performanceLogger;

        public TimedHostedBackgroundService(PerformanceLoggerService performanceLogger)
        {
            _performanceLogger = performanceLogger;
        }

        #region IDisposable

        public void Dispose()
        {
            _timer?.Dispose();
        }

        #endregion

        #region IHostedService

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        #endregion

        private void DoWork(object state)
        {
            _performanceLogger.Flush();

            if (counter % 10 == 0)
            {
                // ToDo: Clean Output Directory
            }
            
            counter++;
            if (counter >= 1440)
                counter = 0;
        }
    }
}

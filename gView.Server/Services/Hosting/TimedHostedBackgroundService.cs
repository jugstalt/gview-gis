using gView.Server.Extensions;
using gView.Server.Services.Logging;
using gView.Server.Services.MapServer;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace gView.Server.Services.Hosting
{
    public class TimedHostedBackgroundService : IHostedService, IDisposable
    {
        private Timer _timer;
        private int counter = 0;

        private readonly MapServiceManager _mapServerService;
        private readonly PerformanceLoggerService _performanceLogger;

        public TimedHostedBackgroundService(MapServiceManager mapServerService,
                                            PerformanceLoggerService performanceLogger)
        {
            _mapServerService = mapServerService;
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
                string outputDirectory = _mapServerService?.Options?.OutputPath;

                outputDirectory.TryDeleteFilesOlderThan(DateTime.UtcNow.AddMinutes(-10));
            }

            counter++;
            if (counter >= 1440)
            {
                counter = 0;
            }
        }
    }
}

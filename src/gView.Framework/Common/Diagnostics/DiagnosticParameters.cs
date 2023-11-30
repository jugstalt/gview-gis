using System;
using System.Threading.Tasks;

namespace gView.Framework.Common.Diagnostics
{
    public class DiagnosticParameters
    {
        private long _idleTicks = 0;

        public T StopIdleTime<T>(Func<T> func)
        {
            long ticks = DateTime.Now.Ticks;
            try
            {
                return func();
            }
            finally
            {
                _idleTicks += DateTime.Now.Ticks - ticks;
            }
        }

        async public Task<T> StopIdleTimeAsync<T>(Task<T> task)
        {
            long ticks = DateTime.Now.Ticks;
            try
            {
                return await task;
            }
            finally
            {
                _idleTicks += DateTime.Now.Ticks - ticks;
            }
        }

        public double IdleMilliseconds =>
            _idleTicks / TimeSpan.TicksPerMillisecond;
    }
}

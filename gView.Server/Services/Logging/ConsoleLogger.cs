using Microsoft.Extensions.Logging;
using System;

namespace gView.Server.Services.Logging
{
    public class ConsoleLogger<T> : ILogger<T>
    {
        #region ILogger

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Scope<TState>(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine($"{ logLevel.ToString() }: { eventId.Name } - { exception.Message }");
        }

        #endregion

        #region Classes 

        private class Scope<TState> : IDisposable
        {
            public Scope(TState state)
            {

            }

            public void Dispose()
            {

            }
        }

        #endregion
    }
}

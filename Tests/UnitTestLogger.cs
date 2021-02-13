using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Tests
{
    internal class UnitTestLoggerScope : IDisposable
    {
        public void Dispose()
        {
            // Do nothing
        }
    }
    internal class UnitTestLogger<T> : ILogger<T>
    {
        private string _output = string.Empty;

        public string GetOutput()
        {
            var output = _output;
            _output = string.Empty;
            return output;
        }
        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return new UnitTestLoggerScope();
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var text = exception?.Message ?? state.ToString();
            _output += text;
        }
    }
}
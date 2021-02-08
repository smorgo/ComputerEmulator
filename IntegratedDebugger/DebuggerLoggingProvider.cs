using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace IntegratedDebugger
{

    [Microsoft.Extensions.Logging.ProviderAlias("EmulatorDebugger")]
    public class DebuggerLoggerProvider : LoggerProvider
    {
        private DebuggerLoggerOptions _options;
        private bool _terminated;

        ConcurrentQueue<LogEntry> InfoQueue = new ConcurrentQueue<LogEntry>();

        protected override void Dispose(bool disposing)
        {
            _terminated = true;
            base.Dispose(disposing);
        }

        public DebuggerLoggerProvider() : this(new DebuggerLoggerOptions())
        {
        }

        public DebuggerLoggerProvider(DebuggerLoggerOptions options)
        {
            _options = options;
        }

        public override bool IsEnabled(LogLevel logLevel)
        {
            bool Result = logLevel != LogLevel.None
                && _options.LogLevel != LogLevel.None
                && Convert.ToInt32(logLevel) >= Convert.ToInt32(_options.LogLevel);

            return Result;
        }

        public override void WriteLog(LogEntry Info)
        {

        }

    }
}
using Microsoft.Extensions.Logging;

namespace IntegratedDebugger
{
    public class DebuggerLoggerOptions
    {
        public LogLevel LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Information;
    }
}
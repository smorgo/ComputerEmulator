using System;
using IntegratedDebugger;

namespace Repl
{
    public class ReplSink : ILogSink
    {
        public EventHandler<LogEntry> OnWriteLog {get; set;}
        public void WriteLog(LogEntry entry)
        {
            OnWriteLog?.Invoke(this, entry);
        }
    }
}
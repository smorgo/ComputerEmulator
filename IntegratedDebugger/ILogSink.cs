using System;

namespace IntegratedDebugger
{
    public interface ILogSink
    {

        EventHandler<LogEntry> OnWriteLog {get; set;}
        void WriteLog(LogEntry entry);
    }
}
using System.Collections.Generic;

namespace IntegratedDebugger
{
    public class LogScopeInfo
    {
        public LogScopeInfo()
        {
        }

        public string Text { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
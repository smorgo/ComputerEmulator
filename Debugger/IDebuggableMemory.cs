using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Debugger
{
    public interface IDebuggableMemory
    {
        MemoryBreakpoints Breakpoints {get;}
        EventHandler<MemoryBreakpointEventArgs> BreakpointTriggered {get; set;}
        void ClearBreakpoints();
        bool AddBreakpoint(MemoryBreakpoint breakpoint);
        bool DeleteBreakpoint(MemoryBreakpoint breakpoint);
    }
}

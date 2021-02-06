using System.Collections.Generic;

namespace Debugger
{
    public class Breakpoints
    {
        public List<MemoryBreakpoint> MemoryBreakpoints {get; private set;}
        public List<ProgramBreakpoint> ProgramBreakpoints {get; private set;}
        public Breakpoints()
        {
            MemoryBreakpoints = new List<MemoryBreakpoint>();
            ProgramBreakpoints = new List<ProgramBreakpoint>();
        }

        public void Add(MemoryBreakpoint breakpoint)
        {
            MemoryBreakpoints.Add(breakpoint);
        }
        public void Add(ProgramBreakpoint breakpoint)
        {
            ProgramBreakpoints.Add(breakpoint);
        }
    }

}
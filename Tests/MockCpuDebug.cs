using System;
using System.Collections.Generic;
using Debugger;

namespace Tests
{
    public class MockCpuDebug : ICpuDebug
    {
        public bool DebugStop { get; set; }
        public EventHandler HasExecuted { get; set; }
        public EventHandler<CpuLog> Log { get; set; }
        public int Verbosity {get; set;}
        public ushort PC { get; set; }
        public byte SP { get; set; }
        public byte A { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public bool C { get; set; }
        public bool Z { get; set; }
        public bool D { get; set; }
        public bool I { get; set; }
        public bool V { get; set; }
        public bool N { get; set; }
        public bool B { get; set; }
        public bool B2 { get; set; }
        private List<ProgramBreakpoint> _breakpoints = new List<ProgramBreakpoint>();
        public IList<ProgramBreakpoint> Breakpoints => _breakpoints;
        public void ClearBreakpoints()
        {
            _breakpoints.Clear();
        }
        public bool AddBreakpoint(ProgramBreakpoint breakpoint)
        {
            // Should ensure it's not a duplicate
            _breakpoints.Add(breakpoint);
            return true;
        }
        public bool DeleteBreakpoint(ProgramBreakpoint breakpoint)
        {
            if(_breakpoints.Contains(breakpoint))
            {
                _breakpoints.Remove(breakpoint);
                return true;
            }

            return false;
        }
    }
}
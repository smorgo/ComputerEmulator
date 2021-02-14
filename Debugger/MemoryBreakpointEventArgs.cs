namespace Debugger
{
    public class MemoryBreakpointEventArgs
    {
        public MemoryBreakpoint Breakpoint {get; private set;}
        public ushort Address {get; private set;}
        public byte Value {get; private set;}
        public MemoryBreakpointEventArgs(MemoryBreakpoint breakpoint, ushort address, byte value)
        {
            Breakpoint = breakpoint;
            Address = address;
            Value = value;
        }
    }
}

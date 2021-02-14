namespace Debugger
{
    public class ProgramBreakpointEventArgs
    {
        public ProgramBreakpoint Breakpoint {get; private set;}
        public ushort Address {get; private set;}
        public byte Opcode {get; private set;}
        public ProgramBreakpointEventArgs(ProgramBreakpoint breakpoint, ushort address, byte opcode)
        {
            Breakpoint = breakpoint;
            Address = address;
            Opcode = opcode;
        }
    }
}

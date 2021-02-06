namespace Debugger
{
    public abstract class ProgramBreakpoint : IBreakpoint
    {
        public abstract string Type {get;}
        public abstract string Description {get;}
        public virtual bool Disabled {get; set;}
        public virtual bool ShouldBreakOnInstruction(ushort address, byte opcode)
        {
            return false;
        }
    }
}
namespace Debugger
{

    public abstract class ProgramBreakpoint : IBreakpoint
    {
        public int Id;
        public abstract string Type {get;}
        public abstract string Describe(ILabelMap labels);
        public virtual bool Disabled {get; set;}
        public abstract bool ShouldBreakOnInstruction(ushort address, byte opcode);
    }
}
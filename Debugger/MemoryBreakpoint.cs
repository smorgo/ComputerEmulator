namespace Debugger
{

    public abstract class MemoryBreakpoint : IBreakpoint
    {
        public int Id {get;set;}
        public abstract string Type {get;}
        public virtual bool Disabled {get; set;}
        public abstract bool ShouldBreakOnMemoryWrite(ushort address, byte value);
        public abstract string Describe(ILabelMap labels);
    }

}
namespace Debugger
{

    public abstract class MemoryBreakpoint : IBreakpoint
    {
        public abstract string Type {get;}
        public abstract string Description {get;}
        public virtual bool Disabled {get; set;}
        public virtual bool ShouldBreakOnMemoryWrite(ushort address, byte value)
        {
            return false;
        }
    }

}
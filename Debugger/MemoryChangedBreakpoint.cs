namespace Debugger
{
    public class MemoryChangedBreakpoint : MemoryBreakpoint
    {
        public ushort Address {get; private set;}
        public uint Size {get; private set;}
        public override string Type => "MEM_CHANGE";
        public MemoryChangedBreakpoint(ushort address, uint size)
        {
            Address = address;
            Size = size;
        }
        public override bool ShouldBreakOnMemoryWrite(ushort address, byte value)
        {
            return !Disabled && (address >= Address) && (address < Address + Size);
        }
        public override string Description 
        {
            get
            {
                var rangeEnd = (Size > 1) ? $"-${(Address + Size -1):X4}" : "";

                return $"{Type} ${Address:X4}{rangeEnd}";
            }
        }
    }
}
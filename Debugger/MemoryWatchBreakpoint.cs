namespace Debugger
{
    public class MemoryWatchBreakpoint : MemoryBreakpoint
    {
        public ushort Address {get; private set;}
        public uint Size {get; private set;}
        public byte Value {get; private set;}
        public override string Type => "MEM_WATCH";
        public MemoryWatchBreakpoint(ushort address, uint size, byte value)
        {
            Address = address;
            Size = size;
            Value = value;
        }
        public override bool ShouldBreakOnMemoryWrite(ushort address, byte value)
        {
            return !Disabled && (address >= Address) && (address < Address + Size) && (Value == value);
        }
        public override string Description 
        {
            get
            {
                var rangeEnd = (Size > 1) ? $"-${(Address + Size -1):X4}" : "";

                return $"{Type} ${Address:X4}{rangeEnd}==${Value:X2} ({Value})";
            }
        }

    }
}
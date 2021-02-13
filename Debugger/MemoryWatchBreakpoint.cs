namespace Debugger
{
    public class MemoryWatchBreakpoint : MemoryBreakpoint
    {
        public int Id = 0;
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
        public override string Describe(ILabelMap labels)
        {
            var rangeEnd = (Size > 1) ? $"-${(Address + Size -1):X4}" : "";

            return $"{Id:D2} {Type} ${Address:X4}{rangeEnd}==${Value:X2} ({Value})";
        }

    }
}
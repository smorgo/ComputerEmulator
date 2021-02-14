namespace Debugger
{
    public class MemoryValueEqualsBreakpoint : MemoryBreakpoint
    {
        public ushort Address {get; private set;}
        public uint Size {get; private set;}
        public byte Value {get; private set;}
        public override string Type => "MEM_EQUALS";
        public MemoryValueEqualsBreakpoint(ushort address, uint size, byte value)
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
            var state = Disabled ? " - Disabled" : "";

            return $"{Id:D2} {Type} ${Address:X4}{rangeEnd}==${Value:X2} ({Value}){state}";
        }

    }
}
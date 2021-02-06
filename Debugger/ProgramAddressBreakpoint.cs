namespace Debugger
{
    public class ProgramAddressBreakpoint : ProgramBreakpoint
    {
        public ushort Address {get; private set;}
        public override string Type => "PC";
        public ProgramAddressBreakpoint(ushort address)
        {
            Address = address;
        }
        public override bool ShouldBreakOnInstruction(ushort address, byte opcode)
        {
            return !Disabled && address == Address;
        }
        public override string Description 
        {
            get
            {
                return $"{Type}==${Address:X4} ({Address})";
            }
        }

    }
}
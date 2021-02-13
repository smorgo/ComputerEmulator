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

        public override string Describe(ILabelMap labels)
        {
            string label = string.Empty;
            
            if (labels.AddressLabels.ContainsKey(Address))
            {
                label = labels.AddressLabels[Address];
            }

            return $"{Id:D2} Break on {Type}==${Address:X4} ({Address}) {label}";
        }

    }
}
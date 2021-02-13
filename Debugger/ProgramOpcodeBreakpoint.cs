namespace Debugger
{
    public class ProgramOpcodeBreakpoint : ProgramBreakpoint
    {
        public byte Opcode {get; private set;}
        public override string Type => "OPCODE";
        public ProgramOpcodeBreakpoint(byte opcode)
        {
            Opcode = opcode;
        }
        public override bool ShouldBreakOnInstruction(ushort address, byte opcode)
        {
            return !Disabled && opcode == Opcode;
        }
        public override string Describe(ILabelMap labels)
        {
            return $"{Id:D2} Break on {Type}==${Opcode:X2}";
        }
    }
}
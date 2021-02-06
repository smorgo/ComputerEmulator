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
        public override string Description 
        {
            get
            {
                return $"{Type}==${Opcode:X2}";
            }
        }

    }
}
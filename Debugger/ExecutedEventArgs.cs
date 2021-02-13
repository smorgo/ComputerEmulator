using HardwareCore;

namespace Debugger
{
    public class ExecutedEventArgs
    {
        public ushort PC {get; private set;}
        public byte Opcode {get; private set;}

        public ExecutedEventArgs(ushort pc, byte opcode)
        {
            PC = pc;
            Opcode = opcode;
        }
    }
}

using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class CLIToken : OpcodeToken
    {
        public CLIToken(Token sourceToken) : base("CLI", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            loader.CLI();
        }
    }
}
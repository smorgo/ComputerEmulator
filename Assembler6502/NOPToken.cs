using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class NOPToken : OpcodeToken
    {
        public NOPToken(Token sourceToken) : base("NOP", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.NOP();
        }
    }

}
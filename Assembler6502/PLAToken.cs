using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class PLAToken : OpcodeToken
    {
        public PLAToken(Token sourceToken) : base("PLA", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.PLA();
        }
    }

}
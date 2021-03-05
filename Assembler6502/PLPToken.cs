using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class PLPToken : OpcodeToken
    {
        public PLPToken(Token sourceToken) : base("PLP", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.PLP();
        }
    }

}
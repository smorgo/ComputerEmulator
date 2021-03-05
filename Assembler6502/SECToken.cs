using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class SECToken : OpcodeToken
    {
        public SECToken(Token sourceToken) : base("SEC", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.SEC();
        }
    }

}
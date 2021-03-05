using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class SEIToken : OpcodeToken
    {
        public SEIToken(Token sourceToken) : base("SEI", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.SEI();
        }
    }

}
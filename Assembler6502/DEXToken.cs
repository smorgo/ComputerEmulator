using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class DEXToken : OpcodeToken
    {
        public override bool HasOperand => false;
        public DEXToken(Token sourceToken) : 
            base("DEX", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.DEX();
        }
    }

}
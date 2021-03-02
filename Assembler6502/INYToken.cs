using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class INYToken : OpcodeToken
    {
        public override bool HasOperand => false;
        public INYToken(Token sourceToken) : 
            base("INY", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            loader.INY();
        }
    }

}
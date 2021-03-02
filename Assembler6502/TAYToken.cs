using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class TAYToken : OpcodeToken
    {
        public override bool HasOperand => false;
        
        public TAYToken(Token sourceToken) : 
            base("TAY", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.TAY();
        }
    }

}
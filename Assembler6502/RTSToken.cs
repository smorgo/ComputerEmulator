using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class RTSToken : OpcodeToken
    {
        public override bool HasOperand => false;
        public RTSToken(Token sourceToken) : 
            base("RTS", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.RTS();
        }
    }

}
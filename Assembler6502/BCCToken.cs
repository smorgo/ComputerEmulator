using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class BCCToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BCCToken(Token sourceToken) : base("BCC", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            if(OperandToken.ProvidesByte)
            {
                loader.BCC(OperandToken.AsByte());
            }
            else if(OperandToken.ProvidesLabel)
            {
                loader.BCC(OperandToken.AsString());
            }
            else
            {
                DidNotEmit();
            }
        }

    }

}
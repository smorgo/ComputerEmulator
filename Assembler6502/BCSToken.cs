using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class BCSToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BCSToken(Token sourceToken) : base("BCS", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken.ProvidesByte)
            {
                loader.BCS(OperandToken.AsByte());
            }
            else if(OperandToken.ProvidesLabel)
            {
                loader.BCS(OperandToken.AsString());
            }
            else
            {
                DidNotEmit();
            }
        }
    }

}
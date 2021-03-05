using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class BEQToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BEQToken(Token sourceToken) : base("BEQ", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken.ProvidesByte)
            {
                loader.BEQ(OperandToken.AsByte());
            }
            else if(OperandToken.ProvidesLabel)
            {
                loader.BEQ(OperandToken.AsString());
            }
            else
            {
                DidNotEmit();
            }
        }
    }

}
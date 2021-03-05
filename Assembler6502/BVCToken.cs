using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class BVCToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BVCToken(Token sourceToken) : base("BVC", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken.ProvidesByte)
            {
                loader.BVC(OperandToken.AsByte());
            }
            else if(OperandToken.ProvidesLabel)
            {
                loader.BVC(OperandToken.AsString());
            }
            else
            {
                DidNotEmit();
            }
        }
    }

}
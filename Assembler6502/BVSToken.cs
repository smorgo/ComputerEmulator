using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class BVSToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BVSToken(Token sourceToken) : base("BVS", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken.ProvidesByte)
            {
                loader.BVS(OperandToken.AsByte());
            }
            else if(OperandToken.ProvidesLabel)
            {
                loader.BVS(OperandToken.AsString());
            }
            else
            {
                DidNotEmit();
            }
        }
    }

}
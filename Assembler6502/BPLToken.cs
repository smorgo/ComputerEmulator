using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class BPLToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BPLToken(Token sourceToken) : base("BPL", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken.ProvidesByte)
            {
                loader.BPL(OperandToken.AsByte());
            }
            else if(OperandToken.ProvidesLabel)
            {
                loader.BPL(OperandToken.AsString());
            }
            else
            {
                DidNotEmit();
            }
        }
    }

}
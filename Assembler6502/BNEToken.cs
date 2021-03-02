using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class BNEToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BNEToken(Token sourceToken) : 
            base("BNE", sourceToken)
        {
        }

        public override bool IsValid 
        {
            get
            {
                return false;
            }
        }

        public override void Emit(ILoader loader)
        {
            if(OperandToken.ProvidesByte)
            {
                loader.BNE(OperandToken.AsByte());
            }
            else if(OperandToken.ProvidesLabel)
            {
                loader.BNE(OperandToken.AsString());
            }
        }
    }

}
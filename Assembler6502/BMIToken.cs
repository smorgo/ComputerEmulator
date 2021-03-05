using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class BMIToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BMIToken(Token sourceToken) : base("BMI", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken.ProvidesByte)
            {
                loader.BMI(OperandToken.AsByte());
            }
            else if(OperandToken.ProvidesLabel)
            {
                loader.BMI(OperandToken.AsString());
            }
            else
            {
                DidNotEmit();
            }
        }
    }

}
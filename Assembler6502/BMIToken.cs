namespace Assembler6502
{
    public class BMIToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BMIToken(Token sourceToken) : base("BMI", sourceToken)
        {
        }
    }

}
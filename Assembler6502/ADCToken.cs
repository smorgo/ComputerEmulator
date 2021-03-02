namespace Assembler6502
{
    public class ADCToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public ADCToken(Token sourceToken) : base("ADC", sourceToken)
        {
        }
    }

}
namespace Assembler6502
{
    public class CPXToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public CPXToken(Token sourceToken) : base("CPX", sourceToken)
        {
        }
    }

}
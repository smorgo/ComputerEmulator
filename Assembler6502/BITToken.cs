namespace Assembler6502
{
    public class BITToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BITToken(Token sourceToken) : base("BIT", sourceToken)
        {
        }
    }

}
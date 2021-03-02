namespace Assembler6502
{
    public class BCSToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BCSToken(Token sourceToken) : base("BCS", sourceToken)
        {
        }
    }

}
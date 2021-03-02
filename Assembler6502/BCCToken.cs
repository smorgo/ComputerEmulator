namespace Assembler6502
{
    public class BCCToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BCCToken(Token sourceToken) : base("BCC", sourceToken)
        {
        }
    }

}
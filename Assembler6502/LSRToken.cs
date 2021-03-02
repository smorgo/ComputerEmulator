namespace Assembler6502
{
    public class LSRToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public LSRToken(Token sourceToken) : base("LSR", sourceToken)
        {
        }
    }

}
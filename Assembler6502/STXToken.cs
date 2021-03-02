namespace Assembler6502
{
    public class STXToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public STXToken(Token sourceToken) : base("STX", sourceToken)
        {
        }
    }

}
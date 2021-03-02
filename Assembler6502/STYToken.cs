namespace Assembler6502
{
    public class STYToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public STYToken(Token sourceToken) : base("STY", sourceToken)
        {
        }
    }

}
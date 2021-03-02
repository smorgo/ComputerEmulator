namespace Assembler6502
{
    public class LDXToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public LDXToken(Token sourceToken) : base("LDX", sourceToken)
        {
        }
    }

}
namespace Assembler6502
{
    public class LDYToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public LDYToken(Token sourceToken) : base("LDY", sourceToken)
        {
        }
    }

}
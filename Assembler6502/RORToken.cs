namespace Assembler6502
{
    public class RORToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public RORToken(Token sourceToken) : base("ROR", sourceToken)
        {
        }
    }

}
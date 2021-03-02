namespace Assembler6502
{
    public class EORToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public EORToken(Token sourceToken) : base("EOR", sourceToken)
        {
        }
    }

}
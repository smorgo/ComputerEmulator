namespace Assembler6502
{
    public class ASLToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public ASLToken(Token sourceToken) : base("ASL", sourceToken)
        {
        }
    }

}
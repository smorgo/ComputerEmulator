namespace Assembler6502
{
    public class SBCToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public SBCToken(Token sourceToken) : base("SBC", sourceToken)
        {
        }
    }

}